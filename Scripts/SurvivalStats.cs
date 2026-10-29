using Godot;

/// <summary>
/// Minimal single-player survival loop. Values are intentionally slow for
/// early mobile playtesting; food and water items will refill them later.
/// </summary>
public partial class SurvivalStats : Node
{
    [Export] public float MaxHealth { get; set; } = 100.0f;
    [Export] public float MaxFood { get; set; } = 100.0f;
    [Export] public float MaxWater { get; set; } = 100.0f;
    [Export] public float FoodDrainPerSecond { get; set; } = 0.025f;
    [Export] public float WaterDrainPerSecond { get; set; } = 0.045f;
    [Export] public float StarvationDamagePerSecond { get; set; } = 2.0f;

    public float Health { get; private set; } = 100.0f;
    public float Food { get; private set; } = 100.0f;
    public float Water { get; private set; } = 100.0f;

    private MobileControls? _hud;
    private double _refreshTimer;

    public override void _Ready()
    {
        Health = MaxHealth;
        Food = MaxFood;
        Water = MaxWater;
        _hud = GetTree().Root.FindChild("MobileControls", true, false) as MobileControls;
        RefreshHud();
    }

    public override void _Process(double delta)
    {
        float step = (float)delta;
        Food = Mathf.Max(0.0f, Food - FoodDrainPerSecond * step);
        Water = Mathf.Max(0.0f, Water - WaterDrainPerSecond * step);

        if (Food <= 0.0f || Water <= 0.0f)
            Health = Mathf.Max(0.0f, Health - StarvationDamagePerSecond * step);

        _refreshTimer += delta;
        if (_refreshTimer >= 0.25)
        {
            _refreshTimer = 0.0;
            RefreshHud();
        }
    }

    public void RestoreFood(float amount) => Food = Mathf.Clamp(Food + amount, 0.0f, MaxFood);
    public void RestoreWater(float amount) => Water = Mathf.Clamp(Water + amount, 0.0f, MaxWater);
    public void RestoreHealth(float amount) => Health = Mathf.Clamp(Health + amount, 0.0f, MaxHealth);

    private void RefreshHud()
    {
        _hud?.SetSurvivalStatus(Health, Water, Food);
    }
}
