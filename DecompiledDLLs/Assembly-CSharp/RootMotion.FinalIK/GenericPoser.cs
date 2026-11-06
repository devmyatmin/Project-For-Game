using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class GenericPoser : Poser
{
	[Serializable]
	public class Map
	{
		public Transform bone;

		public Transform target;

		private Vector3 defaultLocalPosition;

		private Quaternion defaultLocalRotation;

		public Map(Transform bone, Transform target)
		{
			this.bone = bone;
			this.target = target;
			StoreDefaultState();
		}

		public void StoreDefaultState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			defaultLocalPosition = bone.localPosition;
			defaultLocalRotation = bone.localRotation;
		}

		public void FixTransform()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			bone.localPosition = defaultLocalPosition;
			bone.localRotation = defaultLocalRotation;
		}

		public void Update(float localRotationWeight, float localPositionWeight)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			bone.localRotation = Quaternion.Lerp(bone.localRotation, target.localRotation, localRotationWeight);
			bone.localPosition = Vector3.Lerp(bone.localPosition, target.localPosition, localPositionWeight);
		}
	}

	public Map[] maps;

	[ContextMenu("Auto-Mapping")]
	public override void AutoMapping()
	{
		if ((Object)(object)poseRoot == (Object)null)
		{
			maps = new Map[0];
			return;
		}
		maps = new Map[0];
		Transform[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<Transform>();
		Transform[] componentsInChildren2 = ((Component)poseRoot).GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			Transform targetNamed = GetTargetNamed(((Object)componentsInChildren[i]).name, componentsInChildren2);
			if ((Object)(object)targetNamed != (Object)null)
			{
				Array.Resize(ref maps, maps.Length + 1);
				maps[maps.Length - 1] = new Map(componentsInChildren[i], targetNamed);
			}
		}
		StoreDefaultState();
	}

	protected override void InitiatePoser()
	{
		StoreDefaultState();
	}

	protected override void UpdatePoser()
	{
		if (!(weight <= 0f) && (!(localPositionWeight <= 0f) || !(localRotationWeight <= 0f)) && !((Object)(object)poseRoot == (Object)null))
		{
			float num = localRotationWeight * weight;
			float num2 = localPositionWeight * weight;
			for (int i = 0; i < maps.Length; i++)
			{
				maps[i].Update(num, num2);
			}
		}
	}

	protected override void FixPoserTransforms()
	{
		for (int i = 0; i < maps.Length; i++)
		{
			maps[i].FixTransform();
		}
	}

	private void StoreDefaultState()
	{
		for (int i = 0; i < maps.Length; i++)
		{
			maps[i].StoreDefaultState();
		}
	}

	private Transform GetTargetNamed(string tName, Transform[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (((Object)array[i]).name == tName)
			{
				return array[i];
			}
		}
		return null;
	}
}
