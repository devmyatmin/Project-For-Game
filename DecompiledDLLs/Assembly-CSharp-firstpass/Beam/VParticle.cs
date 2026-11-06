using UnityEngine;

namespace Beam;

public class VParticle
{
	public Vector3 Position;

	public Vector3 OldPosition;

	public bool Free;

	public VParticle(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
		OldPosition = position;
	}
}
