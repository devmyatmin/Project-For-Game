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
		base.Snapped = Physics.Raycast(placement.Position + Vector3.up, Vector3.down, out var hitInfo, 10f, base.Layermask.value, QueryTriggerInteraction.Ignore);
		base.SnappedObject = hitInfo.transform?.gameObject;
		if (base.Snapped)
		{
			Vector3 position = placement.Position;
			position.y = hitInfo.point.y;
			position += _snapOffset;
			placement.Position = position;
			if (_snapRotation)
			{
				placement.Rotation = Quaternion.Lerp(placement.Rotation, Quaternion.FromToRotation(base.transform.up, hitInfo.normal) * placement.Rotation, _snapBias);
			}
		}
		return placement;
	}
}
