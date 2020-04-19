using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class City : Node2D
{
	private Random random = new Random();

	private Panel _setupChapterPanel;
	private Panel _statusPanel;
	private bool _hasChapter;
	private int _price;
	private string _name;

	private double _followersPerDay = 1;
	private Label _followerRateLabel;
	private double _followerCount = 0;
	private Label _followerCounter;

    // How much each cultist pays per (ingame) day
    private double _dailyCultFeesPerFollower = 1;

	// Other costs per day
	private double _otherMoneyPerDay;
	private Label _moneyPerDayLabel;

	private TextureButton _purchaseButton;
	private double _populationNotFollowers;

	private List<Upgrade> _upgrades;

	private AudioStreamPlayer _menuSound;

	private bool _panelOpen;

	public void Initialise(CityData cityData) {
		Position = new Vector2(cityData.X, cityData.Y);
		_populationNotFollowers = cityData.Population;
		_price = cityData.Price;
		_name = cityData.Name;
		_hasChapter = cityData.Starting;
	}

	public bool IsFullyConverted() {
		return _populationNotFollowers == 0;
	}

	// Called when the node enters the scene tree for the first time
	public override void _Ready()
	{        
		_setupChapterPanel = GetNode<Panel>("SetupChapter");
		_setupChapterPanel.Visible = false;
		_statusPanel = GetNode<Panel>("Status");
		_statusPanel.Visible = false;
		_statusPanel.GetNode<Label>("VerticalContainer/Name").Text = _name;
		GetNode<Label>("Name").Text = _name;

		_followerCounter = _statusPanel.GetNode<Label>("VerticalContainer/Followers/Background/Count");
		_followerCounter.Text = ((int)_followerCount).ToString();
		_followerRateLabel = _followerCounter.GetNode<Label>("Rate");
		_followerRateLabel.Text = $"({_followersPerDay.ToString("+0;-#")}/day)";
		_moneyPerDayLabel = _statusPanel.GetNode<Label>("VerticalContainer/Money/Background/Rate");
		_moneyPerDayLabel.Text = $"{(Math.Round(_dailyCultFeesPerFollower*_followerCount, 2)).ToString("+0.00;-#.00")}/day";
		
		_menuSound = GetNode<AudioStreamPlayer>("MenuSound");

		_purchaseButton = _setupChapterPanel.GetNode<TextureButton>("Purchase");
		(_statusPanel.GetNode<Label>("VerticalContainer/Population/Background/Count")).Text = _populationNotFollowers.ToString(); 
		(_purchaseButton.GetNode<Label>("Price/Value")).Text = _price.ToString();

		var gameState = GetParent().GetParent().GetNode("Game State");
		Connect(nameof(Purchase), gameState, "OnPurchase");
		Connect(nameof(FollowersChange), gameState, "OnFollowersChangeInChapter"); 	
		Connect(nameof(MoneyIncreased), gameState, "UpdateMoney"); 	
		Connect(nameof(FullyConverted), gameState, "OnCityFullyConverted");

		var  upgradeScene = (PackedScene)ResourceLoader.Load("res://scenes/Upgrade.tscn");

		var upgradesContainer = _statusPanel.GetNode("VerticalContainer/ScrollContainer/Upgrades");
		_upgrades = new List<Upgrade>();

		
		using (var file = new File()) {
			file.Open("res://upgradeData.json", File.ModeFlags.Read);			

			var upgradesData = JsonConvert.DeserializeObject<UpgradeData[]>(file.GetAsText());

			// Initialise upgrades
			foreach (var upgradeData in upgradesData.Where(u => !u.Global)) {
				var upgrade = (Upgrade)(upgradeScene.Instance());
				_upgrades.Add(upgrade);
				upgradesContainer.AddChild(upgrade);
				upgrade.Initialise(upgradeData, this);
			}
		}
	}

	public override void _Process(float delta) {
		
		if (_hasChapter) {
			UpdateFollowerCount(_followersPerDay*delta*GlobalSettings.GameSpeed/(3600*24));
			UpdateMoney((_dailyCultFeesPerFollower*(int)_followerCount+_otherMoneyPerDay)*delta*GlobalSettings.GameSpeed/(3600*24));
		}
		_purchaseButton.Disabled = (GetParent().GetParent().GetNode<GameState>("Game State")).Money < _price;
	}
	
	private void UpdateFollowerCount(double delta) {
		delta = Math.Min(_populationNotFollowers, delta);
		_populationNotFollowers -= delta;


		var newFollowerCount = Math.Max(_followerCount + delta, 0);
		 // Only update if integer has changed
		Console.WriteLine((int)newFollowerCount); 
		var difference = (int)newFollowerCount - (int)_followerCount;
		var rawDifference = newFollowerCount - _followerCount;
		if (difference != 0) {
			_followerCounter.Text = ((int)newFollowerCount).ToString();
			_moneyPerDayLabel.Text = $"{(Math.Round(_dailyCultFeesPerFollower*_followerCount+_otherMoneyPerDay, 2)).ToString("+0.00;-#.00")}/day";
		}

		_followerCount = newFollowerCount;
		
		if (delta != 0) {
			EmitSignal(nameof(FollowersChange), difference, rawDifference);				
		}

		if (IsFullyConverted()) {	
			_followersPerDay = 0;
			_followerRateLabel.Text = "(0/day)";
			EmitSignal(nameof(FullyConverted));
			return;
		}
		
	}

	private void UpdateMoney(double delta) {
		EmitSignal(nameof(MoneyIncreased), delta);
	}

	private void OnMouseEnteredArea()
	{
		if (_hasChapter) {
			_statusPanel.Visible = true;
		} else {
			_setupChapterPanel.Visible = true;
		}

		if (!_panelOpen) {
			_menuSound.Play();
		}

		_panelOpen = true;
	}

	private void OnPanelClosed()
	{
		_setupChapterPanel.Visible = false;
		_statusPanel.Visible = false;
		if (_panelOpen) {
			_menuSound.Play();
		}
		_panelOpen = false;
	}

	private void OnPurchasePressed()
	{
		if (!_hasChapter) {
			_hasChapter = true;
			_setupChapterPanel.Visible = false;
			EmitSignal(nameof(Purchase), _price);
		}
	}

	private void OnUpgradePurchased(string name) {
		var upgradeToRemove = _upgrades.Find(u => u.Data.Name == name);
		_upgrades.Remove(upgradeToRemove);
		var upgradesContainer = GetNode("Status/VerticalContainer/ScrollContainer/Upgrades");
		upgradesContainer.RemoveChild(upgradeToRemove);
		upgradeToRemove.Dispose();

		UpdateRates(upgradeToRemove.Data);

		EmitSignal(nameof(Purchase), upgradeToRemove.Data.Price);
		
	}

	public void UpdateRates(UpgradeData data) {
		_followersPerDay += data.FollowerEffect;
		_followerRateLabel.Text = $"({_followersPerDay.ToString("+0;-#")}/day)";
		_dailyCultFeesPerFollower += data.FeeEffect;
		_otherMoneyPerDay += data.MoneyEffect;
		_moneyPerDayLabel.Text = $"{(Math.Round(_dailyCultFeesPerFollower*_followerCount+_otherMoneyPerDay, 2)).ToString("+0.00;-#.00")}/day";
		
	}

	[Signal]
	public delegate void Purchase(double cost);
	[Signal]
	public delegate void FollowersChange(double change);
	[Signal]
	public delegate void MoneyIncreased(double change);
	[Signal]
	public delegate void FullyConverted();
}

