using Godot;

/// <summary>
/// Creates a few lightweight placeholder pickups for the first interaction test.
/// Replace these primitives with converted, licensed art after import validation.
/// </summary>
public partial class WorldBootstrap : Node3D
{
    public override void _Ready()
    {
        SpawnPickup("stick", "Stick", new Vector3(-1.0f, 0.12f, -2.6f), false);
        SpawnPickup("rock", "Rock", new Vector3(1.0f, 0.16f, -3.2f), true);
        SpawnPickup("stick", "Stick", new Vector3(0.0f, 0.12f, -4.2f), false);
    }

    private void SpawnPickup(string itemId, string displayName, Vector3 position, bool isRock)
    {
        var pickup = new CollectibleResource
        {
            Name = $"{displayName}Pickup",
            ItemId = itemId,
            DisplayName = displayName,
            Amount = 1,
            Position = position
        };

        var shape = new CollisionShape3D();
        if (isRock)
        {
            shape.Shape = new SphereShape3D { Radius = 0.28f };
        }
        else
        {
            shape.Shape = new BoxShape3D { Size = new Vector3(0.18f, 0.12f, 0.65f) };
        }
        pickup.AddChild(shape);

        var meshInstance = new MeshInstance3D();
        if (isRock)
        {
            meshInstance.Mesh = new SphereMesh { Radius = 0.28f, Height = 0.56f };
            meshInstance.Position = new Vector3(0, 0.05f, 0);
        }
        else
        {
            meshInstance.Mesh = new BoxMesh { Size = new Vector3(0.18f, 0.12f, 0.65f) };
            meshInstance.Rotation = new Vector3(0.08f, 0.4f, 0.18f);
        }

        var material = new StandardMaterial3D
        {
            AlbedoColor = isRock
                ? new Color(0.38f, 0.39f, 0.35f)
                : new Color(0.42f, 0.28f, 0.15f),
            Roughness = 0.95f
        };
        meshInstance.MaterialOverride = material;
        pickup.AddChild(meshInstance);
        AddChild(pickup);
    }
}
