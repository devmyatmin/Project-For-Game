using Godot;

/// <summary>
/// Godot-native, single-player movement controller.
/// This is a clean port foundation; Unity source files remain untouched as reference.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 4.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    [Export] public float GravityMultiplier { get; set; } = 1.0f;

    private float _gravity;

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity", 9.8f);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (GlobalTransform.Basis * new Vector3(input.X, 0.0f, input.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            Velocity = new Vector3(direction.X * MoveSpeed, Velocity.Y, direction.Z * MoveSpeed);
        }
        else
        {
            Velocity = new Vector3(0.0f, Velocity.Y, 0.0f);
        }

        if (IsOnFloor())
        {
            if (Velocity.Y < 0.0f)
                Velocity = new Vector3(Velocity.X, 0.0f, Velocity.Z);

            if (Input.IsActionJustPressed("ui_accept"))
                Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
        }
        else
        {
            Velocity = new Vector3(
                Velocity.X,
                Velocity.Y - _gravity * GravityMultiplier * (float)delta,
                Velocity.Z
            );
        }

        MoveAndSlide();
    }
}
