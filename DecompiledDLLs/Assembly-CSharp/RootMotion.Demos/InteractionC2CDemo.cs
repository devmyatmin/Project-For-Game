using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class InteractionC2CDemo : MonoBehaviour
{
	public InteractionSystem character1;

	public InteractionSystem character2;

	public InteractionObject handShake;

	private void OnGUI()
	{
		if (GUILayout.Button("Shake Hands", Array.Empty<GUILayoutOption>()))
		{
			character1.StartInteraction(FullBodyBipedEffector.RightHand, handShake, interrupt: true);
			character2.StartInteraction(FullBodyBipedEffector.RightHand, handShake, interrupt: true);
		}
	}

	private void LateUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = Vector3.Lerp(character1.ik.solver.rightHandEffector.bone.position, character2.ik.solver.rightHandEffector.bone.position, 0.5f);
		((Component)handShake).transform.position = position;
	}
}
