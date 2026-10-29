using Beam.PostEffects;

namespace Beam.UI;

public class QuickCrafterRadialMenuPresenter : RadialMenuPresenterBase<CraftingData, QuickCrafterRadialMenuElement, QuickCrafterRadialMenuViewAdapterBase>, INeedPlayerComponent
{
	private IPlayer _player;

	private IMenuEffectsController _menuEffects;

	public string PreviousComboName { get; set; }

	IPlayer INeedPlayerComponent.Player => _player;

	protected override void Start()
	{
		_menuEffects = _menuEffects ?? NullObject<NullMenuEffectsController>.Instance;
		base.Start();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		Input = _player.Input;
		IMenuEffectsController component = _player.PlayerCamera.Camera.GetComponent<MainMenuEffectsController>();
		_menuEffects = component ?? NullObject<NullMenuEffectsController>.Instance;
	}

	public override void Show()
	{
		_menuEffects.SetMaskTexture(base.View.BlurMaskTexture);
		_menuEffects.Show();
		base.Show();
	}

	public override void Hide()
	{
		_menuEffects?.Hide();
		base.Hide();
	}

	protected override void Layout()
	{
		base.Layout();
		foreach (QuickCrafterRadialMenuElement activeElement in base.ActiveElements)
		{
			if (activeElement.SelectedValue.Value.IsDynamic)
			{
				activeElement.ElementType = QuickCrafterRadialElementType.Pile;
			}
			else
			{
				activeElement.ElementType = ((activeElement.SelectedValue.Value.Name == PreviousComboName) ? QuickCrafterRadialElementType.Repeat : QuickCrafterRadialElementType.Base);
			}
		}
	}
}
