using Godot;

/// <summary>
/// A simple world pickup. Keep this Godot-native so Unity-only asset formats
/// are not required for the first playable prototype.
/// </summary>
public partial class CollectibleResource : StaticBody3D
{
    [Export] public string ItemId { get; set; } = "stick";
    [Export] public string DisplayName { get; set; } = "Stick";
    [Export] public int Amount { get; set; } = 1;

    public bool TryCollect()
    {
        if (Amount <= 0 || !IsInsideTree())
            return false;

        QueueFree();
        return true;
    }
}
