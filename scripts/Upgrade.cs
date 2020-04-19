using Godot;

public class Upgrade : Node
{    
    public UpgradeData Data {get; set;}
    public void Initialise(UpgradeData upgradeData, Node receiver) {
        Data = upgradeData;
        ((Label)GetNode("Panel/VBoxContainer/Name")).Text = Data.Name;
        ((Label)GetNode("Panel/VBoxContainer/Description")).Text = Data.Description; 
        ((UpgradeButton)GetNode("Panel/VBoxContainer/Purchase")).Initialise(Data.Name, Data.Price, receiver);

        var followerEffect = ((HBoxContainer)GetNode("Panel/VBoxContainer/FollowerBenefit"));
        if (Data.FollowerEffect != 0) {
            (followerEffect.GetNode<Label>("Background/Amount")).Text = Data.FollowerEffectText;
        } else {
            followerEffect.Visible = false;
        }

        var feeEffect = ((HBoxContainer)GetNode("Panel/VBoxContainer/MoneyFollowerRate"));
        if (Data.FeeEffect != 0) {
             (feeEffect.GetNode<Label>("Background/Amount")).Text = Data.FeeEffectText;
        } else {
            feeEffect.Visible = false;
        }

        var moneyEffect = ((HBoxContainer)GetNode("Panel/VBoxContainer/MoneyRate"));
        if (Data.MoneyEffect != 0) {
             (moneyEffect.GetNode<Label>("Background/Amount")).Text = Data.MoneyEffectText;
        } else {
            moneyEffect.Visible = false;
        }
    }
 
}
