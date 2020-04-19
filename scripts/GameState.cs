using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameState : Node
{
    [Signal]
    public delegate void FollowersChange(int followerCount);
    [Signal]
    public delegate void MoneyChanges(double money);
    [Signal]
    public delegate void DateChanges(long seconds);
    [Signal]
    public delegate void RatesUpdated(double moneyRate, double followerRate);
    [Signal]
    public delegate void Victory();

    private Gui _gui;

    private int _followerCount;
    public double Money {get; set;}

    // Used for displaying rate
    private double _rawFollowerCount;
    private double _oldFollowerCount;
    private double _oldMoney;

    private DateTimeOffset _currentDate;

    private double _realTimePassed;

    private List<Upgrade> _upgrades;

    private AudioStreamPlayer _cashJingle;

    private bool _gameWon;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _followerCount = 0;
        _oldFollowerCount = _followerCount;
        _rawFollowerCount = _followerCount;
        Money = 50;
        _oldMoney = Money;
        _currentDate = new DateTimeOffset(new DateTime(2012, 12, 22));

        _gui = GetParent().GetNode<Gui>("GUI/GUI");
        _cashJingle = GetParent().GetNode<AudioStreamPlayer>("CashJingle");
        GetParent().GetNode<AudioStreamPlayer>("Music").Play();
   
        Connect(nameof(FollowersChange), _gui, "OnFollowerUpdate"); 
        EmitSignal(nameof(FollowersChange), _followerCount);
        Connect(nameof(MoneyChanges), _gui, "OnMoneyUpdate"); 
        EmitSignal(nameof(MoneyChanges), Money);
        Connect(nameof(DateChanges), _gui, "OnDateUpdate"); 
        EmitSignal(nameof(DateChanges), _currentDate.ToUnixTimeSeconds());
        Connect(nameof(RatesUpdated), _gui, "OnRatesUpdated"); 
        Connect(nameof(Victory), _gui, "OnVictory");


        // Setup global upgrades 
        using (var file = new File()) {
			file.Open("res://upgradeData.json", File.ModeFlags.Read);			
    		var upgradeScene = (PackedScene)ResourceLoader.Load("res://scenes/Upgrade.tscn");

            _upgrades = new List<Upgrade>();
			var upgradesData = JsonConvert.DeserializeObject<UpgradeData[]>(file.GetAsText());

			// Initialise upgrades
			foreach (var upgradeData in upgradesData.Where(u => u.Global)) {
				var upgrade = (Upgrade)(upgradeScene.Instance());
				_upgrades.Add(upgrade);
				_gui.UpgradesElement.AddChild(upgrade);
				upgrade.Initialise(upgradeData, this);
			}
		}
    }

    public override void _Process(float delta)
    {
        var ingameDelta = GlobalSettings.GameSpeed*delta; 
        // Increment time
        UpdateDate(ingameDelta);

        MaybeUpdateRates(delta);

    }

    private void UpdateFollowerCount(int delta) {
        var newFollowerCount = Math.Max(_followerCount + delta, 0);
        Console.WriteLine(newFollowerCount);
        EmitSignal(nameof(FollowersChange), newFollowerCount);
        _followerCount = newFollowerCount;
    }


    private void UpdateMoney(double delta) {
        var newMoney = Money + delta;
        // Only update if at least the second decimal place h
        if (Math.Round(newMoney, 2) != Math.Round(Money, 2)) {
            EmitSignal(nameof(MoneyChanges), newMoney);
        }
        Money = newMoney;
    }

    private void UpdateDate(double delta) {
        var newDate = _currentDate.Add(TimeSpan.FromSeconds(delta));
        // Only update UI if the day changes
        if (newDate.Date != _currentDate.Date) {
            EmitSignal(nameof(DateChanges), newDate.ToUnixTimeSeconds());
        }
        _currentDate = newDate;
    }

    private void MaybeUpdateRates(double delta) {
        
        var newTimePassed = _realTimePassed + delta; 

        if (((int)newTimePassed-(int)_realTimePassed) != 0) {
            // change per ingame day

            var moneyRate = (Money - _oldMoney)/GlobalSettings.GameSpeed*24*3600;
            var followerRate = (_rawFollowerCount - _oldFollowerCount)/GlobalSettings.GameSpeed*24*3600;

            EmitSignal(nameof(RatesUpdated), moneyRate, followerRate);
            _oldFollowerCount = _rawFollowerCount;
            _oldMoney = Money;
        }

        _realTimePassed = newTimePassed;
    }

    private void OnPurchase(double cost) {
        UpdateMoney(-cost);
        // Immediately update
        _oldMoney = Money;
        _cashJingle.Play();
    }

    private void OnFollowersChangeInChapter(int change, double raw) {
        UpdateFollowerCount(change);
        _rawFollowerCount += raw;
    }

    private void OnUpgradePurchased(string name) {
		var upgradeToRemove = _upgrades.Find(u => u.Data.Name == name);
		_upgrades.Remove(upgradeToRemove);
		_gui.UpgradesElement.RemoveChild(upgradeToRemove);
		upgradeToRemove.Dispose();

		OnPurchase(upgradeToRemove.Data.Price);

        foreach (City city in GetParent().GetNode("Cities").GetChildren()) {
            city.UpdateRates(upgradeToRemove.Data);
        }
	}

    private void OnCityFullyConverted() {
        _gameWon = true;
        foreach (City city in GetParent().GetNode("Cities").GetChildren()) {
            _gameWon = _gameWon && city.IsFullyConverted(); 
        }

        if (_gameWon) {
            EmitSignal(nameof(Victory));
        }
    }

}
