using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_SnapToLayer : PlacementModifier_SnapBase
{
	[SerializeField]
	private Vector3 _axis = Vector3.one;

	[SerializeField]
	private float _normalOffset;

	[SerializeField]
	private bool _snapRotation;

	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 originPoint = ray.OriginPoint;
		Vector3 direction = ray.Direction;
		float length = ray.Length;
		LayerMask layermask = Layermask;
		RaycastHit val = default;
		Snapped = Physics.Raycast(originPoint, direction, ref val, length, layermask.value, (QueryTriggerInteraction)1);
		Transform transform = val.transform;
		SnappedObject = ((transform != null) ? ((Component)transform).gameObject : null);
		if (Snapped)
		{
			Vector3 val2 = Vector3Tools.Mask(val.normal, _axis);
			val2.Normalize();
			placement.Position = val.point + val.normal * _normalOffset;
			placement.Rotation = Quaternion.FromToRotation(((Component)this).transform.forward, val2);
		}
		return placement;
	}

	public PlacementModifier_SnapToLayer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
