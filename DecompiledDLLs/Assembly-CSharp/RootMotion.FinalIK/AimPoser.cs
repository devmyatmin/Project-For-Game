using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class AimPoser : MonoBehaviour
{
	[Serializable]
	public class Pose
	{
		public bool visualize = true;

		public string name;

		public Vector3 direction;

		public float yaw = 75f;

		public float pitch = 45f;

		private float angleBuffer;

		public bool IsInDirection(Vector3 d)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			if (direction == Vector3.zero)
			{
				return false;
			}
			if (yaw <= 0f || pitch <= 0f)
			{
				return false;
			}
			if (yaw < 180f)
			{
				Vector3 val = new Vector3(direction.x, 0f, direction.z);
				if (val == Vector3.zero)
				{
					val = Vector3.forward;
				}
				if (Vector3.Angle(new Vector3(d.x, 0f, d.z), val) > yaw + angleBuffer)
				{
					return false;
				}
			}
			if (pitch >= 180f)
			{
				return true;
			}
			float num = Vector3.Angle(Vector3.up, direction);
			return Mathf.Abs(Vector3.Angle(Vector3.up, d) - num) < pitch + angleBuffer;
		}

		public void SetAngleBuffer(float value)
		{
			angleBuffer = value;
		}
	}

	public float angleBuffer = 5f;

	public Pose[] poses = new Pose[0];

	public Pose GetPose(Vector3 localDirection)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (poses.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < poses.Length - 1; i++)
		{
			if (poses[i].IsInDirection(localDirection))
			{
				return poses[i];
			}
		}
		return poses[poses.Length - 1];
	}

	public void SetPoseActive(Pose pose)
	{
		for (int i = 0; i < poses.Length; i++)
		{
			poses[i].SetAngleBuffer((poses[i] == pose) ? angleBuffer : 0f);
		}
	}
}
