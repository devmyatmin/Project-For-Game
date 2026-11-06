using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/VR IK")]
public class VRIK : IK
{
	[Serializable]
	public class References
	{
		public Transform root;

		[LargeHeader("Spine")]
		public Transform pelvis;

		public Transform spine;

		[Tooltip("Optional")]
		public Transform chest;

		[Tooltip("Optional")]
		public Transform neck;

		public Transform head;

		[LargeHeader("Left Arm")]
		[Tooltip("Optional")]
		public Transform leftShoulder;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform leftUpperArm;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform leftForearm;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform leftHand;

		[LargeHeader("Right Arm")]
		[Tooltip("Optional")]
		public Transform rightShoulder;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform rightUpperArm;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform rightForearm;

		[Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
		public Transform rightHand;

		[LargeHeader("Left Leg")]
		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform leftThigh;

		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform leftCalf;

		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform leftFoot;

		[Tooltip("Optional")]
		public Transform leftToes;

		[LargeHeader("Right Leg")]
		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform rightThigh;

		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform rightCalf;

		[Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
		public Transform rightFoot;

		[Tooltip("Optional")]
		public Transform rightToes;

		public bool isFilled
		{
			get
			{
				if ((Object)(object)root == (Object)null || (Object)(object)pelvis == (Object)null || (Object)(object)spine == (Object)null || (Object)(object)head == (Object)null)
				{
					return false;
				}
				bool flag = (Object)(object)leftUpperArm == (Object)null && (Object)(object)leftForearm == (Object)null && (Object)(object)leftHand == (Object)null && (Object)(object)rightUpperArm == (Object)null && (Object)(object)rightForearm == (Object)null && (Object)(object)rightHand == (Object)null;
				bool flag2 = (Object)(object)leftUpperArm == (Object)null || (Object)(object)leftForearm == (Object)null || (Object)(object)leftHand == (Object)null || (Object)(object)rightUpperArm == (Object)null || (Object)(object)rightForearm == (Object)null || (Object)(object)rightHand == (Object)null;
				bool flag3 = (Object)(object)leftThigh == (Object)null && (Object)(object)leftCalf == (Object)null && (Object)(object)leftFoot == (Object)null && (Object)(object)rightThigh == (Object)null && (Object)(object)rightCalf == (Object)null && (Object)(object)rightFoot == (Object)null;
				if (((Object)(object)leftThigh == (Object)null || (Object)(object)leftCalf == (Object)null || (Object)(object)leftFoot == (Object)null || (Object)(object)rightThigh == (Object)null || (Object)(object)rightCalf == (Object)null || (Object)(object)rightFoot == (Object)null) && !flag3)
				{
					return false;
				}
				if (flag2 && !flag)
				{
					return false;
				}
				return true;
			}
		}

		public bool isEmpty
		{
			get
			{
				if ((Object)(object)root != (Object)null || (Object)(object)pelvis != (Object)null || (Object)(object)spine != (Object)null || (Object)(object)chest != (Object)null || (Object)(object)neck != (Object)null || (Object)(object)head != (Object)null || (Object)(object)leftShoulder != (Object)null || (Object)(object)leftUpperArm != (Object)null || (Object)(object)leftForearm != (Object)null || (Object)(object)leftHand != (Object)null || (Object)(object)rightShoulder != (Object)null || (Object)(object)rightUpperArm != (Object)null || (Object)(object)rightForearm != (Object)null || (Object)(object)rightHand != (Object)null || (Object)(object)leftThigh != (Object)null || (Object)(object)leftCalf != (Object)null || (Object)(object)leftFoot != (Object)null || (Object)(object)leftToes != (Object)null || (Object)(object)rightThigh != (Object)null || (Object)(object)rightCalf != (Object)null || (Object)(object)rightFoot != (Object)null || (Object)(object)rightToes != (Object)null)
				{
					return false;
				}
				return true;
			}
		}

		public References()
		{
		}

		public References(BipedReferences b)
		{
			root = b.root;
			pelvis = b.pelvis;
			spine = b.spine[0];
			chest = ((b.spine.Length > 1) ? b.spine[1] : null);
			head = b.head;
			leftShoulder = b.leftUpperArm.parent;
			leftUpperArm = b.leftUpperArm;
			leftForearm = b.leftForearm;
			leftHand = b.leftHand;
			rightShoulder = b.rightUpperArm.parent;
			rightUpperArm = b.rightUpperArm;
			rightForearm = b.rightForearm;
			rightHand = b.rightHand;
			leftThigh = b.leftThigh;
			leftCalf = b.leftCalf;
			leftFoot = b.leftFoot;
			leftToes = b.leftFoot.GetChild(0);
			rightThigh = b.rightThigh;
			rightCalf = b.rightCalf;
			rightFoot = b.rightFoot;
			rightToes = b.rightFoot.GetChild(0);
		}

		public Transform[] GetTransforms()
		{
			return new Transform[22]
			{
				root, pelvis, spine, chest, neck, head, leftShoulder, leftUpperArm, leftForearm, leftHand,
				rightShoulder, rightUpperArm, rightForearm, rightHand, leftThigh, leftCalf, leftFoot, leftToes, rightThigh, rightCalf,
				rightFoot, rightToes
			};
		}

		public static bool AutoDetectReferences(Transform root, out References references)
		{
			references = new References();
			Animator componentInChildren = ((Component)root).GetComponentInChildren<Animator>();
			if ((Object)(object)componentInChildren == (Object)null || !componentInChildren.isHuman)
			{
				Debug.LogWarning((object)"VRIK needs a Humanoid Animator to auto-detect biped references. Please assign references manually.");
				return false;
			}
			references.root = root;
			references.pelvis = componentInChildren.GetBoneTransform((HumanBodyBones)0);
			references.spine = componentInChildren.GetBoneTransform((HumanBodyBones)7);
			references.chest = componentInChildren.GetBoneTransform((HumanBodyBones)8);
			references.neck = componentInChildren.GetBoneTransform((HumanBodyBones)9);
			references.head = componentInChildren.GetBoneTransform((HumanBodyBones)10);
			references.leftShoulder = componentInChildren.GetBoneTransform((HumanBodyBones)11);
			references.leftUpperArm = componentInChildren.GetBoneTransform((HumanBodyBones)13);
			references.leftForearm = componentInChildren.GetBoneTransform((HumanBodyBones)15);
			references.leftHand = componentInChildren.GetBoneTransform((HumanBodyBones)17);
			references.rightShoulder = componentInChildren.GetBoneTransform((HumanBodyBones)12);
			references.rightUpperArm = componentInChildren.GetBoneTransform((HumanBodyBones)14);
			references.rightForearm = componentInChildren.GetBoneTransform((HumanBodyBones)16);
			references.rightHand = componentInChildren.GetBoneTransform((HumanBodyBones)18);
			references.leftThigh = componentInChildren.GetBoneTransform((HumanBodyBones)1);
			references.leftCalf = componentInChildren.GetBoneTransform((HumanBodyBones)3);
			references.leftFoot = componentInChildren.GetBoneTransform((HumanBodyBones)5);
			references.leftToes = componentInChildren.GetBoneTransform((HumanBodyBones)19);
			references.rightThigh = componentInChildren.GetBoneTransform((HumanBodyBones)2);
			references.rightCalf = componentInChildren.GetBoneTransform((HumanBodyBones)4);
			references.rightFoot = componentInChildren.GetBoneTransform((HumanBodyBones)6);
			references.rightToes = componentInChildren.GetBoneTransform((HumanBodyBones)20);
			return true;
		}
	}

	[ContextMenuItem("Auto-detect References", "AutoDetectReferences")]
	[Tooltip("Bone mapping. Right-click on the component header and select 'Auto-detect References' of fill in manually if not a Humanoid character. Chest, neck, shoulder and toe bones are optional. VRIK also supports legless characters. If you do not wish to use legs, leave all leg references empty.")]
	public References references = new References();

	[Tooltip("The VRIK solver.")]
	public IKSolverVR solver = new IKSolverVR();

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page16.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_v_r_i_k.html");
	}

	[ContextMenu("TUTORIAL VIDEO (STEAMVR SETUP)")]
	private void OpenSetupTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=6Pfx7lYQiIA&feature=youtu.be");
	}

	[ContextMenu("Auto-detect References")]
	public void AutoDetectReferences()
	{
		References.AutoDetectReferences(((Component)this).transform, out references);
	}

	[ContextMenu("Guess Hand Orientations")]
	public void GuessHandOrientations()
	{
		solver.GuessHandOrientations(references, onlyIfZero: false);
	}

	public override IKSolver GetIKSolver()
	{
		return solver;
	}

	protected override void InitiateSolver()
	{
		if (references.isEmpty)
		{
			AutoDetectReferences();
		}
		if (references.isFilled)
		{
			solver.SetToReferences(references);
		}
		base.InitiateSolver();
	}

	protected override void UpdateSolver()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)references.root != (Object)null && references.root.localScale == Vector3.zero)
		{
			Debug.LogError((object)"VRIK Root Transform's scale is zero, can not update VRIK. Make sure you have not calibrated the character to a zero scale.", (Object)(object)((Component)this).transform);
			((Behaviour)this).enabled = false;
		}
		else
		{
			base.UpdateSolver();
		}
	}
}
