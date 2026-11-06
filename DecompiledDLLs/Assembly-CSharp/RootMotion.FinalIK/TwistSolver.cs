using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class TwistSolver
{
	[Tooltip("The transform that this solver operates on.")]
	public Transform transform;

	[Tooltip("If this is the forearm roll bone, the parent should be the forearm bone. If null, will be found automatically.")]
	public Transform parent;

	[Tooltip("If this is the forearm roll bone, the child should be the hand bone. If null, will attempt to find automatically. Assign the hand manually if the hand bone is not a child of the roll bone.")]
	public Transform[] children = new Transform[0];

	[Tooltip("The weight of relaxing the twist of this Transform")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("If 0.5, this Transform will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
	[Range(0f, 1f)]
	public float parentChildCrossfade = 0.5f;

	[Tooltip("Rotation offset around the twist axis.")]
	[Range(-180f, 180f)]
	public float twistAngleOffset;

	private Vector3 twistAxis = Vector3.right;

	private Vector3 axis = Vector3.forward;

	private Vector3 axisRelativeToParentDefault;

	private Vector3 axisRelativeToChildDefault;

	private Quaternion[] childRotations;

	private bool inititated;

	private Quaternion defaultLocalRotation = Quaternion.identity;

	private Quaternion[] defaultChildLocalRotations;

	public TwistSolver()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		weight = 1f;
		parentChildCrossfade = 0.5f;
	}

	public void Initiate()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		if (inititated)
		{
			return;
		}
		if ((Object)(object)transform == (Object)null)
		{
			Debug.LogError((object)"TwistRelaxer solver has unassigned Transform. TwistRelaxer.cs was restructured for FIK v2.0 to support multiple relaxers on the same body part and TwistRelaxer components need to be set up again, sorry for the inconvenience!", (Object)(object)transform);
			return;
		}
		if ((Object)(object)parent == (Object)null)
		{
			parent = transform.parent;
		}
		if (children.Length == 0)
		{
			if (transform.childCount == 0)
			{
				Transform[] componentsInChildren = ((Component)parent).GetComponentsInChildren<Transform>();
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					if ((Object)(object)componentsInChildren[i] != (Object)(object)transform)
					{
						componentsInChildren = new Transform[1] { componentsInChildren[i] };
						break;
					}
				}
			}
			else
			{
				children = new Transform[1] { transform.GetChild(0) };
			}
		}
		if (children.Length == 0 || (Object)(object)children[0] == (Object)null)
		{
			Debug.LogError((object)"TwistRelaxer has no children assigned.", (Object)(object)transform);
			return;
		}
		twistAxis = transform.InverseTransformDirection(children[0].position - transform.position);
		axis = new Vector3(twistAxis.y, twistAxis.z, twistAxis.x);
		Vector3 val = transform.rotation * axis;
		axisRelativeToParentDefault = Quaternion.Inverse(parent.rotation) * val;
		axisRelativeToChildDefault = Quaternion.Inverse(children[0].rotation) * val;
		childRotations = new Quaternion[children.Length];
		defaultLocalRotation = transform.localRotation;
		defaultChildLocalRotations = new Quaternion[children.Length];
		for (int j = 0; j < children.Length; j++)
		{
			defaultChildLocalRotations[j] = children[j].localRotation;
		}
		inititated = true;
	}

	public void FixTransforms()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		transform.localRotation = defaultLocalRotation;
		for (int i = 0; i < children.Length; i++)
		{
			children[i].localRotation = defaultChildLocalRotations[i];
		}
	}

	public void Relax()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (inititated && !(weight <= 0f))
		{
			Quaternion rotation = transform.rotation;
			Quaternion val = Quaternion.AngleAxis(twistAngleOffset, rotation * twistAxis);
			rotation = val * rotation;
			Vector3 val2 = val * parent.rotation * axisRelativeToParentDefault;
			Vector3 val3 = val * children[0].rotation * axisRelativeToChildDefault;
			Vector3 val4 = Vector3.Slerp(val2, val3, parentChildCrossfade);
			val4 = Quaternion.Inverse(Quaternion.LookRotation(rotation * axis, rotation * twistAxis)) * val4;
			float num = Mathf.Atan2(val4.x, val4.z) * 57.29578f;
			for (int i = 0; i < children.Length; i++)
			{
				childRotations[i] = children[i].rotation;
			}
			transform.rotation = Quaternion.AngleAxis(num * weight, rotation * twistAxis) * rotation;
			for (int j = 0; j < children.Length; j++)
			{
				children[j].rotation = childRotations[j];
			}
		}
	}
}
