using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
public class InteractionTarget : MonoBehaviour
{
	[Serializable]
	public enum RotationMode
	{
		TwoDOF,
		ThreeDOF
	}

	[Serializable]
	public class Multiplier
	{
		[Tooltip("The curve type (InteractionObject.WeightCurve.Type).")]
		public InteractionObject.WeightCurve.Type curve;

		[Tooltip("Multiplier of the curve's value.")]
		public float multiplier;
	}

	[Tooltip("The type of the FBBIK effector.")]
	public FullBodyBipedEffector effectorType;

	[Tooltip("InteractionObject weight curve multipliers for this effector target.")]
	public Multiplier[] multipliers;

	[Tooltip("The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.")]
	public float interactionSpeedMlp = 1f;

	[Tooltip("The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.")]
	public Transform pivot;

	[Tooltip("2 or 3 degrees of freedom to match this InteractionTarget's rotation to the effector bone rotation.")]
	public RotationMode rotationMode;

	[Tooltip("The axis of twisting the interaction target (blue line).")]
	public Vector3 twistAxis = Vector3.up;

	[Tooltip("The weight of twisting the interaction target towards the effector bone in the start of the interaction.")]
	public float twistWeight = 1f;

	[Tooltip("The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.")]
	public float swingWeight;

	[Tooltip("The weight of rotating this InteractionTarget to the effector bone in the start of the interaction (and during if 'Rotate Once' is disabled")]
	[Range(0f, 1f)]
	public float threeDOFWeight = 1f;

	[Tooltip("If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.")]
	public bool rotateOnce = true;

	private Quaternion defaultLocalRotation;

	private Transform lastPivot;

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
	private void OpenTutorial1()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
	private void OpenTutorial2()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
	private void OpenTutorial3()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
	private void OpenTutorial4()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("Support Group")]
	private void SupportGroup()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	private void ASThread()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public float GetValue(InteractionObject.WeightCurve.Type curveType)
	{
		for (int i = 0; i < multipliers.Length; i++)
		{
			if (multipliers[i].curve == curveType)
			{
				return multipliers[i].multiplier;
			}
		}
		return 1f;
	}

	public void ResetRotation()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pivot != (Object)null)
		{
			pivot.localRotation = defaultLocalRotation;
		}
	}

	public void RotateTo(Transform bone)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pivot == (Object)null)
		{
			return;
		}
		if ((Object)(object)pivot != (Object)(object)lastPivot)
		{
			defaultLocalRotation = pivot.localRotation;
			lastPivot = pivot;
		}
		pivot.localRotation = defaultLocalRotation;
		switch (rotationMode)
		{
		case RotationMode.TwoDOF:
			if (twistWeight > 0f)
			{
				Vector3 fromDirection = ((Component)this).transform.position - pivot.position;
				Vector3 val2 = pivot.rotation * twistAxis;
				Vector3 val3 = val2;
				Vector3.OrthoNormalize(ref val3, ref fromDirection);
				val3 = val2;
				Vector3 toDirection = bone.position - pivot.position;
				Vector3.OrthoNormalize(ref val3, ref toDirection);
				Quaternion val4 = QuaTools.FromToAroundAxis(fromDirection, toDirection, val2);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, val4, twistWeight) * pivot.rotation;
			}
			if (swingWeight > 0f)
			{
				Quaternion val5 = Quaternion.FromToRotation(((Component)this).transform.position - pivot.position, bone.position - pivot.position);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, val5, swingWeight) * pivot.rotation;
			}
			break;
		case RotationMode.ThreeDOF:
			if (!(threeDOFWeight <= 0f))
			{
				Quaternion val = QuaTools.FromToRotation(((Component)this).transform.rotation, bone.rotation);
				if (threeDOFWeight >= 1f)
				{
					pivot.rotation = val * pivot.rotation;
				}
				else
				{
					pivot.rotation = Quaternion.Slerp(Quaternion.identity, val, threeDOFWeight) * pivot.rotation;
				}
			}
			break;
		}
	}

	public InteractionTarget()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
