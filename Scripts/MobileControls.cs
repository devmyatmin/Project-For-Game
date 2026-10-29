using Godot;

/// <summary>
/// Lightweight touch HUD for the single-player mobile prototype.
/// The left stick drives movement; dragging the right side looks around.
/// </summary>
public partial class MobileControls : CanvasLayer
{
    [Export] public float LookSensitivity { get; set; } = 0.0045f;

    public Vector2 MovementInput { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public bool CrouchToggled { get; private set; }

    private Control _stickBase = null!;
    private Control _stickKnob = null!;
    private Control _lookArea = null!;
    private Button _jumpButton = null!;
    private Label _statusLabel = null!;
    private Label _inventoryLabel = null!;
    private bool _stickActive;
    private int _stickTouch = -1;
    private int _lookTouch = -1;
        private const float StickRadius = 58.0f;

    public override void _Ready()
    {
        var root = new Control
        {
            Name = "TouchHud",
            AnchorRight = 1,
            AnchorBottom = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        AddChild(root);

        // Add the look zone first so buttons and joystick remain on top of it.
        _lookArea = new Control
        {
            Name = "LookArea",
            AnchorLeft = 0.38f,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        root.AddChild(_lookArea);
        _lookArea.GuiInput += OnLookInput;

        _stickBase = MakeCircle("JoystickBase", new Vector2(28, -164), new Vector2(164, -28), new Color(0.06f, 0.09f, 0.11f, 0.48f));
        _stickBase.AnchorLeft = 0;
        _stickBase.AnchorRight = 0;
        _stickBase.AnchorTop = 1;
        _stickBase.AnchorBottom = 1;
        root.AddChild(_stickBase);
        _stickBase.GuiInput += OnStickInput;
        
        _stickKnob = MakeCircle("JoystickKnob", new Vector2(39, 39), new Vector2(97, 97), new Color(0.78f, 0.86f, 0.88f, 0.62f));
        _stickKnob.MouseFilter = Control.MouseFilterEnum.Ignore;
        _stickBase.AddChild(_stickKnob);

        var interact = MakeButton(root, "InteractButton", "USE", new Vector2(-104, -178), new Vector2(-24, -98), 1, 1);
        interact.Pressed += () => EmitSignal(SignalName.InteractRequested);

        var crouch = MakeButton(root, "CrouchButton", "↓", new Vector2(-184, -118), new Vector2(-112, -46), 1, 1);
        crouch.Pressed += () =>
        {
            CrouchToggled = !CrouchToggled;
            EmitSignal(SignalName.CrouchChanged, CrouchToggled);
        };

        _jumpButton = MakeButton(root, "JumpButton", "↑", new Vector2(-100, -96), new Vector2(-20, -16), 1, 1);
        _jumpButton.Pressed += () => EmitSignal(SignalName.JumpRequested);

        var hint = new Label
        {
            Name = "HudHint",
            Text = "SURVIVE",
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 0,
            AnchorBottom = 0,
            OffsetLeft = 24,
            OffsetTop = 20,
            OffsetRight = 180,
            OffsetBottom = 48,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        hint.AddThemeColorOverride("font_color", new Color(0.88f, 0.94f, 0.91f, 0.88f));
        hint.AddThemeFontSizeOverride("font_size", 18);
        root.AddChild(hint);

        _statusLabel = new Label
        {
            Name = "SurvivalStatus",
            Text = "♥ 100     WATER 100     FOOD 100",
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 0,
            AnchorBottom = 0,
            OffsetLeft = 24,
            OffsetTop = 52,
            OffsetRight = 300,
            OffsetBottom = 76,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.91f, 0.94f, 0.88f, 0.8f));
        _statusLabel.AddThemeFontSizeOverride("font_size", 12);
        root.AddChild(_statusLabel);

        _inventoryLabel = new Label
        {
            Name = "InventoryStatus",
            Text = "ITEMS  0",
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 0,
            AnchorBottom = 0,
            OffsetLeft = 24,
            OffsetTop = 78,
            OffsetRight = 300,
            OffsetBottom = 100,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _inventoryLabel.AddThemeColorOverride("font_color", new Color(0.91f, 0.94f, 0.88f, 0.8f));
        _inventoryLabel.AddThemeFontSizeOverride("font_size", 12);
        root.AddChild(_inventoryLabel);
    }

    [Signal] public delegate void JumpRequestedEventHandler();
    [Signal] public delegate void InteractRequestedEventHandler();
    [Signal] public delegate void CrouchChangedEventHandler(bool isCrouching);

    public void SetSurvivalStatus(float health, float water, float food)
    {
        if (_statusLabel != null)
            _statusLabel.Text = $"♥ {Mathf.RoundToInt(health)}   WATER {Mathf.RoundToInt(water)}   FOOD {Mathf.RoundToInt(food)}";
    }

    public void SetInventoryStatus(string itemName, int amountCollected, int total)
    {
        if (_inventoryLabel != null)
            _inventoryLabel.Text = $"PICKED UP: {itemName} +{amountCollected}  (TOTAL {total})";
    }

    public Vector2 ConsumeLookDelta()
    {
        Vector2 value = LookDelta;
        LookDelta = Vector2.Zero;
        return value;
    }

    private Control MakeCircle(string nodeName, Vector2 topLeft, Vector2 bottomRight, Color color)
    {
        var panel = new Panel
        {
            Name = nodeName,
            OffsetLeft = topLeft.X,
            OffsetTop = topLeft.Y,
            OffsetRight = bottomRight.X,
            OffsetBottom = bottomRight.Y,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        var style = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = 100,
            CornerRadiusTopRight = 100,
            CornerRadiusBottomLeft = 100,
            CornerRadiusBottomRight = 100,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.86f, 0.94f, 0.94f, 0.25f)
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    private Button MakeButton(Control parent, string nodeName, string label, Vector2 topLeft, Vector2 bottomRight, float anchorX, float anchorY)
    {
        var button = new Button
        {
            Name = nodeName,
            Text = label,
            AnchorLeft = anchorX,
            AnchorRight = anchorX,
            AnchorTop = anchorY,
            AnchorBottom = anchorY,
            OffsetLeft = topLeft.X,
            OffsetTop = topLeft.Y,
            OffsetRight = bottomRight.X,
            OffsetBottom = bottomRight.Y,
            FocusMode = Control.FocusModeEnum.None
        };
        var normal = new StyleBoxFlat
        {
            BgColor = new Color(0.07f, 0.12f, 0.14f, 0.64f),
            CornerRadiusTopLeft = 40,
            CornerRadiusTopRight = 40,
            CornerRadiusBottomLeft = 40,
            CornerRadiusBottomRight = 40,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.85f, 0.94f, 0.92f, 0.45f)
        };
        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = new Color(0.22f, 0.37f, 0.35f, 0.88f);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", normal);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeColorOverride("font_color", new Color(0.95f, 0.98f, 0.96f, 1));
        button.AddThemeFontSizeOverride("font_size", label == "USE" ? 13 : 24);
        parent.AddChild(button);
        return button;
    }

    private void OnStickInput(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed && _stickTouch == -1)
            {
                _stickTouch = touch.Index;
                _stickActive = true;
                UpdateStick(touch.Position);
            }
            else if (!touch.Pressed && touch.Index == _stickTouch)
            {
                ResetStick();
            }
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == _stickTouch)
        {
            UpdateStick(drag.Position);
        }
        else if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            _stickActive = mouse.Pressed;
            if (_stickActive) UpdateStick(mouse.Position);
            else ResetStick();
        }
        else if (@event is InputEventMouseMotion motion && _stickActive && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            UpdateStick(motion.Position);
        }
    }

    private void UpdateStick(Vector2 screenPosition)
    {
        Vector2 local = screenPosition;
        Vector2 center = _stickBase.Size * 0.5f;
        Vector2 offset = local - center;
        if (offset.Length() > StickRadius) offset = offset.Normalized() * StickRadius;
        MovementInput = new Vector2(offset.X / StickRadius, offset.Y / StickRadius);
        _stickKnob.Position = center + offset - (_stickKnob.Size * 0.5f);
    }

    private void ResetStick()
    {
        _stickActive = false;
        _stickTouch = -1;
        MovementInput = Vector2.Zero;
        _stickKnob.Position = (_stickBase.Size - _stickKnob.Size) * 0.5f;
    }

    private void OnLookInput(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed && _lookTouch == -1)
                _lookTouch = touch.Index;
            else if (!touch.Pressed && touch.Index == _lookTouch)
                _lookTouch = -1;
        }
        else if (@event is InputEventScreenDrag drag && drag.Index == _lookTouch)
        {
            LookDelta += drag.Relative * LookSensitivity;
        }
        else if (@event is InputEventMouseMotion motion && Input.IsMouseButtonPressed(MouseButton.Right))
        {
            LookDelta += motion.Relative * LookSensitivity;
        }
    }
}
