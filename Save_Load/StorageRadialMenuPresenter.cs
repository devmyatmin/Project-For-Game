using System.Collections.Generic;
using Beam.Language;
using Beam.PostEffects;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class StorageRadialMenuPresenter : RadialMenuPresenterBase<InventoryData, InventoryRadialMenuElement, InventoryRadialMenuViewAdapterBase>, INeedPlayerComponent
{
	private IPlayer _player;

	private IMenuEffectsController _menuEffects;

	private InventoryRadialMenuElement _backpackElement;

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

	protected override void Layout()
	{
		if (_backpackElement == null)
		{
			_backpackElement = Object.Instantiate(base.View.EmptyHandElementPrefab);
			_backpackElement.Initialize(new List<InventoryData>
			{
				new BackpackInventoryData()
			});
		}
		InsertElement(_backpackElement, 0);
		base.Layout();
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

	protected override void Clear()
	{
		base.Clear();
		base.View.ClassificationLabel.Text = string.Empty;
		base.View.HealthLabel.Text = string.Empty;
	}

	protected override void Element_Enter(InventoryRadialMenuElement element)
	{
		base.Element_Enter(element);
		if (element.SelectedValue != null)
		{
			base.View.ClassificationLabel.Text = element.SelectedValue.GetClassification();
			base.View.HealthLabel.Text = string_safemethods.Format(Localization.GetLanguageHandler().Localize(element.SelectedValue.GetHealth()), element.SelectedValue.GetHealthValue());
		}
	}
}
