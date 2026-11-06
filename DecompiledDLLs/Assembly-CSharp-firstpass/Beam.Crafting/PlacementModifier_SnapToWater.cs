using System;
using Ceto;
using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_SnapToWater : PlacementModifier
{
	[SerializeField]
	private float _depth = 0.4f;

	[SerializeField]
	private float _offset = 0.4f;

	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(new Ray(placement.Position + Vector3.up * 0.1f, Vector3.down), ref val, float.PositiveInfinity, 1 << Layers.TERRAIN) && val.point.y > 0f - _depth)
		{
			return placement;
		}
		return SnapToWater(placement);
	}

	private Placement SnapToWater(Placement placement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = placement.Position;
		position.y = Ceto.Ocean.Instance.QueryWaves(position.x, position.z) + _offset;
		float num = Mathf.Cos(Time.time * ((float)Math.PI * 2f)) * 2f;
		float num2 = Mathf.Sin(Time.time * ((float)Math.PI * 2f)) * 2f;
		placement.Position = position;
		placement.Rotation *= Quaternion.Euler(new Vector3(num2, 0f, num));
		return placement;
	}
}
