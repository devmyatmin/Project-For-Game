using UnityEngine;

namespace Beam.UI;

public class TMPHudViewAdapter : HudViewAdapterBase
{
	[SerializeField]
	private UImageViewAdapter _defaultCursor;

	[SerializeField]
	private UImageViewAdapter _interactingCursor;

	[SerializeField]
	private TMPLabelViewAdapter _itemNameLabel;

	[SerializeField]
	private TMPActionHudElementViewAdapter _climbUI;

	[SerializeField]
	private TMPActionHudElementViewAdapter _actionHudElementPrefab;

	[SerializeField]
	private TransformViewAdapter _actionHudElementsContainer;

	[SerializeField]
	private TransformViewAdapter _pooledActionHudElementsContainer;

	[SerializeField]
	private UCanvasGroupViewAdapter _itemActionGroup;

	public override IImageViewAdapter DefaultCursor => _defaultCursor;

	public override IImageViewAdapter InteractingCursor => _interactingCursor;

	public override ILabelViewAdapter ItemNameLabel => _itemNameLabel;

	public override ActionHudElementViewAdapterBase ClimbUI => _climbUI;

	public override ActionHudElementViewAdapterBase ActionHudElementPrefab => _actionHudElementPrefab;

	public override ITransformViewAdapter ActionHudElementsContainer => _actionHudElementsContainer;

	public override ITransformViewAdapter PooledActionHudElementsContainer => _pooledActionHudElementsContainer;

	public override ICanvasGroupViewAdapter ItemActionGroup => _itemActionGroup;
}
