using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class SingleFish : FishBase
{
	public float _interestRadius = 1f;

	public override bool CheckPositionInBounds(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = FishRenderer.Bounds;
		return bounds.Contains(position);
	}

	public override bool CheckPositionInInterestRadius(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - position;
		return val.sqrMagnitude < BeamMath.Sqr(_interestRadius);
	}

	public override Bounds GetBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return FishRenderer.Bounds;
	}
}
