using Godot;
using Newtonsoft.Json;

public class InitialiseCities : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private PackedScene cityScene = (PackedScene)ResourceLoader.Load("res://scenes/City.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        using (var file = new File()) {
			file.Open("res://cityData.json", File.ModeFlags.Read);			

			var citiesData = JsonConvert.DeserializeObject<CityData[]>(file.GetAsText());
			
			// Initialise cities
			foreach (var cityData in citiesData) {
			    var city = (City)cityScene.Instance();
                city.Initialise(cityData);
                AddChild(city);
			}
		}
    }

}
