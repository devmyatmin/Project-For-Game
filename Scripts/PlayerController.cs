using Godot;

/// <summary>
/// Godot-native single-player controller. Supports keyboard input for testing
/// and the on-screen mobile HUD when it is present in the active scene.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 4.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    [Export] public float GravityMultiplier { get; set; } = 1.0f;
    [Export] public float LookSensitivity { get; set; } = 1.0f;

    private float _gravity;
    private Node3D? _cameraPivot;
    private Camera3D? _camera;
    private MobileControls? _mobileControls;
    private bool _jumpRequested;
    private float _cameraPitch;

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity", 9.8f);
        _cameraPivot = GetNodeOrNull<Node3D>("CameraPivot");
        _camera = _cameraPivot?.GetNodeOrNull<Camera3D>("Camera3D");
        _mobileControls = GetTree().Root.FindChild("MobileControls", true, false) as MobileControls;

        if (_mobileControls != null)
            _mobileControls.JumpRequested += OnJumpRequested;
    }

    public override void _ExitTree()
    {
        if (_mobileControls != null)
            _mobileControls.JumpRequested -= OnJumpRequested;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            GetTree().Quit();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (_mobileControls != null && _mobileControls.MovementInput.Length() > 0.08f)
            input = _mobileControls.MovementInput;

        Vector3 direction = (GlobalTransform.Basis * new Vector3(input.X, 0.0f, input.Y)).Normalized();
        if (direction != Vector3.Zero)
            Velocity = new Vector3(direction.X * MoveSpeed, Velocity.Y, direction.Z * MoveSpeed);
        else
            Velocity = new Vector3(0.0f, Velocity.Y, 0.0f);

        if (_mobileControls != null)
        {
            Vector2 look = _mobileControls.ConsumeLookDelta() * LookSensitivity;
            if (look != Vector2.Zero)
            {
                RotateY(-look.X);
                _cameraPitch = Mathf.Clamp(_cameraPitch - look.Y, -1.15f, 1.15f);
                if (_cameraPivot != null)
                    _cameraPivot.Rotation = new Vector3(_cameraPitch, 0.0f, 0.0f);
            }
        }

        if (IsOnFloor())
        {
            if (Velocity.Y < 0.0f)
                Velocity = new Vector3(Velocity.X, 0.0f, Velocity.Z);

            if (Input.IsActionJustPressed("ui_accept") || _jumpRequested)
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

        _jumpRequested = false;
        MoveAndSlide();
    }

    private void OnJumpRequested() => _jumpRequested = true;
}
