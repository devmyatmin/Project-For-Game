using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class Turret : MonoBehaviour
{
	[Serializable]
	public class Part
	{
		public Transform transform;

		private RotationLimit rotationLimit;

		public void AimAt(Transform target)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			transform.LookAt(target.position, transform.up);
			if ((Object)(object)rotationLimit == (Object)null)
			{
				rotationLimit = ((Component)transform).GetComponent<RotationLimit>();
				rotationLimit.Disable();
			}
			rotationLimit.Apply();
		}
	}

	public Transform target;

	public Part[] parts;

	private void Update()
	{
		Part[] array = parts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AimAt(target);
		}
	}
}
