using UnityEngine;

namespace Beam.Crafting;

public abstract class PlacementModifier_SnapBase : PlacementModifier
{
	[SerializeField]
	private LayerMask _layermask;

	private bool _snapped;

	private GameObject _snappedObject;

	protected LayerMask Layermask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _layermask;
		}
	}

	public bool Snapped
	{
		get
		{
			return _snapped;
		}
		protected set
		{
			_snapped = value;
		}
	}

	public GameObject SnappedObject
	{
		get
		{
			return _snappedObject;
		}
		protected set
		{
			_snappedObject = value;
		}
	}
}
