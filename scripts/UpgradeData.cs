public class UpgradeData {
    public double Price {get; set;}
    public string Name {get; set;}
    public string Description {get; set;}
    public double FollowerEffect {get; set;} 
    public double FeeEffect {get; set;}
    public double MoneyEffect {get; set;}
    public string FollowerEffectText => $"{FollowerEffect.ToString("+0.00;-#.00")}/day per city"; 
    public string FeeEffectText => $"{FeeEffect.ToString("+0.00;-#.00")}/day per follower"; 
    public string MoneyEffectText => $"{MoneyEffect.ToString("+0.00;-#.00")}/day per city"; 

    public bool Global {get; set;}
}