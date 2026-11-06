using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class HitReactionTrigger : MonoBehaviour
{
	public HitReaction hitReaction;

	public float hitForce = 1f;

	private string colliderName;

	private void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			Ray val = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit val2 = default;
			if (Physics.Raycast(val, ref val2, 100f))
			{
				hitReaction.Hit(val2.collider, val.direction * hitForce, val2.point);
				colliderName = ((Object)val2.collider).name;
			}
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.", Array.Empty<GUILayoutOption>());
		if (colliderName != string.Empty)
		{
			GUILayout.Label("Last Bone Hit: " + colliderName, Array.Empty<GUILayoutOption>());
		}
	}
}
