using UnityEngine;

namespace RootMotion;

public class TQ
{
	public Vector3 t;

	public Quaternion q;

	public TQ()
	{
	}

	public TQ(Vector3 translation, Quaternion rotation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		t = translation;
		q = rotation;
	}
}
