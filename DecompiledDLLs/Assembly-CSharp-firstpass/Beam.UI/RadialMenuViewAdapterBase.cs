using UnityEngine;

namespace Beam.UI;

public abstract class RadialMenuViewAdapterBase<TModel, TElement> : ViewAdapterBase where TModel : class, IRadialMenuModel where TElement : class, IRadialMenuElement<TModel>
{
	public abstract ITransformViewAdapter ElementPlaceholderPrefab { get; }

	public abstract ITransformViewAdapter ElementContainer { get; }

	public abstract TElement ElementPrefab { get; }

	public abstract ITransformViewAdapter PooledElementContainer { get; }

	public abstract IImageViewAdapter SelectorImage { get; }

	public abstract ILabelViewAdapter TitleLabel { get; }

	public abstract ILabelViewAdapter DescriptionLabel { get; }

	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract ICanvasGroupViewAdapter DetailsCanvasGroup { get; }

	public abstract Texture2D BlurMaskTexture { get; }
}
