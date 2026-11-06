using System;
using UnityEngine;

namespace RootMotion.Demos;

public class FKOffset : MonoBehaviour
{
	[Serializable]
	public class Offset
	{
		[HideInInspector]
		public string name;

		public HumanBodyBones bone;

		public Vector3 rotationOffset;

		private Transform t;

		public void Apply(Animator animator)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)t == (Object)null)
			{
				t = animator.GetBoneTransform(bone);
			}
			if (!((Object)(object)t == (Object)null))
			{
				Transform val = t;
				val.localRotation *= Quaternion.Euler(rotationOffset);
			}
		}
	}

	public Offset[] offsets;

	private Animator animator;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
	}

	private void LateUpdate()
	{
		Offset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(animator);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Offset[] array = offsets;
		foreach (Offset obj in array)
		{
			obj.name = ((object)obj.bone/*cast due to constrained. prefix*/).ToString();
		}
	}
}
