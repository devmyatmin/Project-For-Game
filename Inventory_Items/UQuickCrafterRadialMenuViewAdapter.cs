using UnityEngine;

namespace Beam.UI;

public class UQuickCrafterRadialMenuViewAdapter : QuickCrafterRadialMenuViewAdapterBase
{
	[SerializeField]
	private QuickCrafterRadialMenuElement _elementPrefab;

	[SerializeField]
	private TransformViewAdapter _elementPlaceholderPrefab;

	[SerializeField]
	private TransformViewAdapter _elementContainer;

	[SerializeField]
	private TransformViewAdapter _pooledElementContainer;

	[SerializeField]
	private UImageViewAdapter _selector;

	[SerializeField]
	private TMPLabelViewAdapter _titleLabel;

	[SerializeField]
	private TMPLabelViewAdapter _descriptionLabel;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UCanvasGroupViewAdapter _detailsCanvasGroup;

	[SerializeField]
	private Texture2D _blurMaskTexture;

	public override QuickCrafterRadialMenuElement ElementPrefab => _elementPrefab;

	public override ITransformViewAdapter ElementPlaceholderPrefab => _elementPlaceholderPrefab;

	public override ITransformViewAdapter ElementContainer => _elementContainer;

	public override ITransformViewAdapter PooledElementContainer => _pooledElementContainer;

	public override IImageViewAdapter SelectorImage => _selector;

	public override ILabelViewAdapter TitleLabel => _titleLabel;

	public override ILabelViewAdapter DescriptionLabel => _descriptionLabel;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override ICanvasGroupViewAdapter DetailsCanvasGroup => _detailsCanvasGroup;

	public override Texture2D BlurMaskTexture => _blurMaskTexture;
}
