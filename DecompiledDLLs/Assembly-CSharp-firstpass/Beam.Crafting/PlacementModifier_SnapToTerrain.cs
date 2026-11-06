using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_SnapToTerrain : PlacementModifier_SnapBase
{
	[SerializeField]
	private Vector3 _snapOffset = new Vector3(0f, 0.1f, 0f);

	[SerializeField]
	private bool _snapRotation;

	[Range(0f, 1f)]
	[SerializeField]
	private float _snapBias = 0.9f;

	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = placement.Position + Vector3.up;
		Vector3 down = Vector3.down;
		LayerMask layermask = Layermask;
		RaycastHit val2 = default;
		Snapped = Physics.Raycast(val, down, ref val2, 10f, layermask.value, (QueryTriggerInteraction)1);
		Transform transform = val2.transform;
		SnappedObject = ((transform != null) ? ((Component)transform).gameObject : null);
		if (Snapped)
		{
			Vector3 position = placement.Position;
			position.y = val2.point.y;
			position += _snapOffset;
			placement.Position = position;
			if (_snapRotation)
			{
				placement.Rotation = Quaternion.Lerp(placement.Rotation, Quaternion.FromToRotation(((Component)this).transform.up, val2.normal) * placement.Rotation, _snapBias);
			}
		}
		return placement;
	}

	public PlacementModifier_SnapToTerrain()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
