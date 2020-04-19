using Godot;
using System;

public class Gui : MarginContainer
{
	private Label _followerCounter;
	private Label _followerRate;

	private Label _moneyCounter;
	private Label _moneyRate;
	
	private Label _date;

	public Node UpgradesElement => GetNode("HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/ScrollContainer/Upgrades");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_followerCounter = GetNode<Label>("HBoxContainer/PanelContainer/Bars/Followers/Count");
		_followerRate = GetNode<Label>("HBoxContainer/PanelContainer/Bars/Followers/Rate");

		_moneyCounter = GetNode<Label>("HBoxContainer/PanelContainer/Bars/Money/Count");
		_moneyRate = GetNode<Label>("HBoxContainer/PanelContainer/Bars/Money/Rate");

		_date = GetNode<Label>("HBoxContainer/DateContainer/Date");
	}

	public void OnFollowerUpdate(int followerCount) {
		_followerCounter.Text = followerCount.ToString();
	}

	public void OnMoneyUpdate(double money) {
		_moneyCounter.Text = Math.Round(money, 2).ToString();
	}

	public void OnDateUpdate(long unixTimeSeconds) {
		_date.Text = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).ToString("d MMM yyyy");
	}

	private void OnRatesUpdated(double moneyRate, double followerRate) {
		_moneyRate.Text = $"({moneyRate.ToString("+0.00;-#.00")}/day)";
		_followerRate.Text = $"({followerRate.ToString("+0.00;-#.00")}/day)";
	}

	private void OnVictory() {
		GetNode<PanelContainer>("Node2D/CenterContainer/Victory Screen").Visible = true;
	}

	private void OnOkPressed() {
		GetNode<PanelContainer>("Node2D/CenterContainer/Start Screen").Visible = false;
	}
}
