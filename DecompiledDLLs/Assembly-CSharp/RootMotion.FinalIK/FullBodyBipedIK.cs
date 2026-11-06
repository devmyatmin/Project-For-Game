using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=7__IafZGwvI&index=1&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Full Body Biped IK")]
public class FullBodyBipedIK : IK
{
	public BipedReferences references = new BipedReferences();

	public IKSolverFullBodyBiped solver = new IKSolverFullBodyBiped();

	[ContextMenu("User Manual")]
	protected override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page8.html");
	}

	[ContextMenu("Scrpt Reference")]
	protected override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_full_body_biped_i_k.html");
	}

	[ContextMenu("TUTORIAL VIDEO (SETUP)")]
	private void OpenSetupTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=7__IafZGwvI");
	}

	[ContextMenu("TUTORIAL VIDEO (INSPECTOR)")]
	private void OpenInspectorTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=tgRMsTphjJo");
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

	public void SetReferences(BipedReferences references, Transform rootNode)
	{
		this.references = references;
		solver.SetToReferences(this.references, rootNode);
	}

	public override IKSolver GetIKSolver()
	{
		return solver;
	}

	public bool ReferencesError(ref string errorMessage)
	{
		if (BipedReferences.SetupError(references, ref errorMessage))
		{
			return true;
		}
		if (references.spine.Length == 0)
		{
			errorMessage = "References has no spine bones assigned, can not initiate the solver.";
			return true;
		}
		if ((Object)(object)solver.rootNode == (Object)null)
		{
			errorMessage = "Root Node bone is null, can not initiate the solver.";
			return true;
		}
		if ((Object)(object)solver.rootNode != (Object)(object)references.pelvis)
		{
			bool flag = false;
			for (int i = 0; i < references.spine.Length; i++)
			{
				if ((Object)(object)solver.rootNode == (Object)(object)references.spine[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				errorMessage = "The Root Node has to be one of the bones in the Spine or the Pelvis, can not initiate the solver.";
				return true;
			}
		}
		return false;
	}

	public bool ReferencesWarning(ref string warningMessage)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (BipedReferences.SetupWarning(references, ref warningMessage))
		{
			return true;
		}
		Vector3 val = references.rightUpperArm.position - references.leftUpperArm.position;
		Vector3 val2 = solver.rootNode.position - references.leftUpperArm.position;
		if (Vector3.Dot(val.normalized, val2.normalized) > 0.95f)
		{
			warningMessage = "The root node, the left upper arm and the right upper arm bones should ideally form a triangle that is as close to equilateral as possible. Currently the root node bone seems to be very close to the line between the left upper arm and the right upper arm bones. This might cause unwanted behaviour like the spine turning upside down when pulled by a hand effector.Please set the root node bone to be one of the lower bones in the spine.";
			return true;
		}
		Vector3 val3 = references.rightThigh.position - references.leftThigh.position;
		Vector3 val4 = solver.rootNode.position - references.leftThigh.position;
		if (Vector3.Dot(val3.normalized, val4.normalized) > 0.95f)
		{
			warningMessage = "The root node, the left thigh and the right thigh bones should ideally form a triangle that is as close to equilateral as possible. Currently the root node bone seems to be very close to the line between the left thigh and the right thigh bones. This might cause unwanted behaviour like the hip turning upside down when pulled by an effector.Please set the root node bone to be one of the higher bones in the spine.";
			return true;
		}
		return false;
	}

	[ContextMenu("Reinitiate")]
	private void Reinitiate()
	{
		SetReferences(references, solver.rootNode);
	}

	[ContextMenu("Auto-detect References")]
	private void AutoDetectReferences()
	{
		references = new BipedReferences();
		BipedReferences.AutoDetectReferences(ref references, ((Component)this).transform, new BipedReferences.AutoDetectParams(legsParentInSpine: true, includeEyes: false));
		solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
		solver.SetToReferences(references, solver.rootNode);
	}
}
