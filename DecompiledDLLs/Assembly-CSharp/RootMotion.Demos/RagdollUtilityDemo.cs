using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class RagdollUtilityDemo : MonoBehaviour
{
	public RagdollUtility ragdollUtility;

	public Transform root;

	public Rigidbody pelvis;

	private void OnGUI()
	{
		GUILayout.Label(" Press R to switch to ragdoll. \n Weigh in one of the FBBIK effectors to make kinematic changes to the ragdoll pose.\n A to blend back to animation", Array.Empty<GUILayoutOption>());
	}

	private void Update()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)114))
		{
			ragdollUtility.EnableRagdoll();
		}
		if (Input.GetKeyDown((KeyCode)97))
		{
			Vector3 val = pelvis.position - root.position;
			Transform val2 = root;
			val2.position += val;
			Transform transform = ((Component)pelvis).transform;
			transform.position -= val;
			ragdollUtility.DisableRagdoll();
		}
	}
}
