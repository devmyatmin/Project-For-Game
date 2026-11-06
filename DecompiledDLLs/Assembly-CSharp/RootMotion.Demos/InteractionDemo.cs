using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class InteractionDemo : MonoBehaviour
{
	public InteractionSystem interactionSystem;

	public bool interrupt;

	public InteractionObject ball;

	public InteractionObject benchMain;

	public InteractionObject benchHands;

	public InteractionObject button;

	public InteractionObject cigarette;

	public InteractionObject door;

	private bool isSitting;

	private void OnGUI()
	{
		interrupt = GUILayout.Toggle(interrupt, "Interrupt", Array.Empty<GUILayoutOption>());
		if (isSitting)
		{
			if (!interactionSystem.inInteraction && GUILayout.Button("Stand Up", Array.Empty<GUILayoutOption>()))
			{
				interactionSystem.ResumeAll();
				isSitting = false;
			}
			return;
		}
		if (GUILayout.Button("Pick Up Ball", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, ball, interrupt);
		}
		if (GUILayout.Button("Button Left Hand", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, button, interrupt);
		}
		if (GUILayout.Button("Button Right Hand", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, button, interrupt);
		}
		if (GUILayout.Button("Put Out Cigarette", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightFoot, cigarette, interrupt);
		}
		if (GUILayout.Button("Open Door", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, door, interrupt);
		}
		if (!interactionSystem.inInteraction && GUILayout.Button("Sit Down", Array.Empty<GUILayoutOption>()))
		{
			interactionSystem.StartInteraction(FullBodyBipedEffector.Body, benchMain, interrupt);
			interactionSystem.StartInteraction(FullBodyBipedEffector.LeftThigh, benchMain, interrupt);
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightThigh, benchMain, interrupt);
			interactionSystem.StartInteraction(FullBodyBipedEffector.LeftFoot, benchMain, interrupt);
			interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, benchHands, interrupt);
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, benchHands, interrupt);
			isSitting = true;
		}
	}
}
