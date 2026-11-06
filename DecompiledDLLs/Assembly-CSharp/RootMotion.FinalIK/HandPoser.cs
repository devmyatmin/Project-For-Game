using UnityEngine;

namespace RootMotion.FinalIK;

public class HandPoser : Poser
{
	protected Transform[] children;

	private Transform _poseRoot;

	private Transform[] poseChildren;

	private Vector3[] defaultLocalPositions;

	private Quaternion[] defaultLocalRotations;

	public override void AutoMapping()
	{
		if ((Object)(object)poseRoot == (Object)null)
		{
			poseChildren = new Transform[0];
		}
		else
		{
			poseChildren = ((Component)poseRoot).GetComponentsInChildren<Transform>();
		}
		_poseRoot = poseRoot;
	}

	protected override void InitiatePoser()
	{
		children = ((Component)this).GetComponentsInChildren<Transform>();
		StoreDefaultState();
	}

	protected override void FixPoserTransforms()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < children.Length; i++)
		{
			children[i].localPosition = defaultLocalPositions[i];
			children[i].localRotation = defaultLocalRotations[i];
		}
	}

	protected override void UpdatePoser()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f || (localPositionWeight <= 0f && localRotationWeight <= 0f))
		{
			return;
		}
		if ((Object)(object)_poseRoot != (Object)(object)poseRoot)
		{
			AutoMapping();
		}
		if ((Object)(object)poseRoot == (Object)null)
		{
			return;
		}
		if (children.Length != poseChildren.Length)
		{
			Warning.Log("Number of children does not match with the pose", ((Component)this).transform);
			return;
		}
		float num = localRotationWeight * weight;
		float num2 = localPositionWeight * weight;
		for (int i = 0; i < children.Length; i++)
		{
			if ((Object)(object)children[i] != (Object)(object)((Component)this).transform)
			{
				children[i].localRotation = Quaternion.Lerp(children[i].localRotation, poseChildren[i].localRotation, num);
				children[i].localPosition = Vector3.Lerp(children[i].localPosition, poseChildren[i].localPosition, num2);
			}
		}
	}

	protected void StoreDefaultState()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		defaultLocalPositions = new Vector3[children.Length];
		defaultLocalRotations = new Quaternion[children.Length];
		for (int i = 0; i < children.Length; i++)
		{
			defaultLocalPositions[i] = children[i].localPosition;
			defaultLocalRotations[i] = children[i].localRotation;
		}
	}
}
