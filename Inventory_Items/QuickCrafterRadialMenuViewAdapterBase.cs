using UnityEngine;

namespace Beam.UI;

public abstract class QuickCrafterRadialMenuViewAdapterBase : RadialMenuViewAdapterBase<CraftingData, QuickCrafterRadialMenuElement>
{
	[SerializeField]
	private GameObject _menuGameObject;

	public abstract override ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract override ICanvasGroupViewAdapter DetailsCanvasGroup { get; }

	public abstract override ITransformViewAdapter ElementContainer { get; }

	public abstract override QuickCrafterRadialMenuElement ElementPrefab { get; }

	public abstract override ITransformViewAdapter ElementPlaceholderPrefab { get; }

	public abstract override ITransformViewAdapter PooledElementContainer { get; }

	public abstract override IImageViewAdapter SelectorImage { get; }

	public abstract override ILabelViewAdapter TitleLabel { get; }

	public abstract override ILabelViewAdapter DescriptionLabel { get; }

	public abstract override Texture2D BlurMaskTexture { get; }

	public override void Show()
	{
		if (!_menuGameObject.activeSelf)
		{
			_menuGameObject.SetActive(value: true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_menuGameObject.activeSelf)
		{
			_menuGameObject.SetActive(value: false);
			base.Hide();
		}
	}
}
