using UnityEngine;

namespace Beam.UI;

public class NullRadialMenuViewAdapterBase : RadialMenuViewAdapterBase<NullRadialMenuModel, NullRadialMenuElement>
{
	private static readonly NullRadialMenuViewAdapterBase _Instance = new NullRadialMenuViewAdapterBase();

	public static NullRadialMenuViewAdapterBase Instance => _Instance;

	public override ITransformViewAdapter ElementPlaceholderPrefab => NullObject<NullTransformViewAdapter>.Instance;

	public override ITransformViewAdapter ElementContainer => NullObject<NullTransformViewAdapter>.Instance;

	public override NullRadialMenuElement ElementPrefab => NullObject<NullRadialMenuElement>.Instance;

	public override ITransformViewAdapter PooledElementContainer => NullObject<NullTransformViewAdapter>.Instance;

	public override IImageViewAdapter SelectorImage => NullViewAdapter<NullImageViewAdapter>.Instance;

	public override ILabelViewAdapter TitleLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ILabelViewAdapter DescriptionLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter CanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter DetailsCanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override Texture2D BlurMaskTexture => Texture2D.blackTexture;
}
