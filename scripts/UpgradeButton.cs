using Godot;
using Godot.Collections;
public class UpgradeButton : TextureButton
{
    private GameState _gameState;
    private double _price;

    public void Initialise(string name, double price, Node receiver) {
        _price = price;
        ((Label)GetNode("Price/Value")).Text = string.Format("{0:n}", _price);
        var bindParams = new Array(new object[] {name});

        Connect("pressed", receiver, "OnUpgradePurchased", bindParams);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameState = (GetTree().Root.GetNode<GameState>("Root/Game State"));
    }


    public override void _Process(float delta)
    {
        Disabled = _gameState.Money < _price;
    }
}
