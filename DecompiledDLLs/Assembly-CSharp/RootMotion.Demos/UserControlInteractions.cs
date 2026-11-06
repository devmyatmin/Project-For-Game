using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class UserControlInteractions : UserControlThirdPerson
{
	public CharacterThirdPerson character;

	public InteractionSystem interactionSystem;

	public bool disableInputInInteraction = true;

	public float enableInputAtProgress = 0.8f;

	protected override void Update()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (disableInputInInteraction && (Object)(object)interactionSystem != (Object)null && (interactionSystem.inInteraction || interactionSystem.IsPaused()))
		{
			float minActiveProgress = interactionSystem.GetMinActiveProgress();
			if (minActiveProgress > 0f && minActiveProgress < enableInputAtProgress)
			{
				state.move = Vector3.zero;
				state.jump = false;
				return;
			}
		}
		base.Update();
	}

	private void OnGUI()
	{
		if (!character.onGround)
		{
			return;
		}
		if (interactionSystem.IsPaused() && interactionSystem.IsInSync())
		{
			GUILayout.Label("Press E to resume interaction", Array.Empty<GUILayoutOption>());
			if (Input.GetKey((KeyCode)101))
			{
				interactionSystem.ResumeAll();
			}
			return;
		}
		int closestTriggerIndex = interactionSystem.GetClosestTriggerIndex();
		if (closestTriggerIndex != -1 && interactionSystem.TriggerEffectorsReady(closestTriggerIndex))
		{
			GUILayout.Label("Press E to start interaction", Array.Empty<GUILayoutOption>());
			if (Input.GetKey((KeyCode)101))
			{
				interactionSystem.TriggerInteraction(closestTriggerIndex, interrupt: false);
			}
		}
	}
}
