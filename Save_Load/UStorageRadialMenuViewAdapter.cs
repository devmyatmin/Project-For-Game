using UnityEngine;

namespace Beam.UI;

public class UStorageRadialMenuViewAdapter : InventoryRadialMenuViewAdapterBase
{
	[SerializeField]
	private InventoryRadialMenuElement _elementPrefab;

	[SerializeField]
	private TransformViewAdapter _elementPlaceholderPrefab;

	[SerializeField]
	private InventoryRadialMenuElement _emptyHandElementPrefab;

	[SerializeField]
	private InventoryRadialMenuElement _storageElementPrefab;

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
	private TMPLabelViewAdapter _classificationLabel;

	[SerializeField]
	private TMPLabelViewAdapter _healthLabel;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UCanvasGroupViewAdapter _detailsCanvasGroup;

	[SerializeField]
	private Texture2D _blurMaskTexture;

	public override InventoryRadialMenuElement ElementPrefab => _elementPrefab;

	public override ITransformViewAdapter ElementPlaceholderPrefab => _elementPlaceholderPrefab;

	public override InventoryRadialMenuElement EmptyHandElementPrefab => _emptyHandElementPrefab;

	public override InventoryRadialMenuElement StorageElementPrefab => _storageElementPrefab;

	public override ITransformViewAdapter ElementContainer => _elementContainer;

	public override ITransformViewAdapter PooledElementContainer => _pooledElementContainer;

	public override IImageViewAdapter SelectorImage => _selector;

	public override ILabelViewAdapter TitleLabel => _titleLabel;

	public override ILabelViewAdapter DescriptionLabel => _descriptionLabel;

	public override ILabelViewAdapter ClassificationLabel => _classificationLabel;

	public override ILabelViewAdapter HealthLabel => _healthLabel;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override ICanvasGroupViewAdapter DetailsCanvasGroup => _detailsCanvasGroup;

	public override Texture2D BlurMaskTexture => _blurMaskTexture;
}
