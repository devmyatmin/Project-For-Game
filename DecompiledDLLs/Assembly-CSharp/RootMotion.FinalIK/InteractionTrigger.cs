using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
public class InteractionTrigger : MonoBehaviour
{
	[Serializable]
	public class CharacterPosition
	{
		[Tooltip("If false, will not care where the character stands, as long as it is in contact with the trigger collider.")]
		public bool use;

		[Tooltip("The offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.")]
		public Vector2 offset;

		[Tooltip("Angle offset from the default forward direction.")]
		[Range(-180f, 180f)]
		public float angleOffset;

		[Tooltip("Max angular offset of the character's forward from the direction of this trigger.")]
		[Range(0f, 180f)]
		public float maxAngle = 45f;

		[Tooltip("Max offset of the character's position from this range's center.")]
		public float radius = 0.5f;

		[Tooltip("If true, will rotate the trigger around its Y axis relative to the position of the character, so the object can be interacted with from all sides.")]
		public bool orbit;

		[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object. For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on.")]
		public bool fixYAxis;

		public Vector3 offset3D
		{
			get
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return new Vector3(offset.x, 0f, offset.y);
			}
		}

		public Vector3 direction3D
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				return Quaternion.AngleAxis(angleOffset, Vector3.up) * Vector3.forward;
			}
		}

		public bool IsInRange(Transform character, Transform trigger, out float error)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			error = 0f;
			if (!use)
			{
				return true;
			}
			error = 180f;
			if (radius <= 0f)
			{
				return false;
			}
			if (maxAngle <= 0f)
			{
				return false;
			}
			Vector3 forward = trigger.forward;
			if (fixYAxis)
			{
				forward.y = 0f;
			}
			if (forward == Vector3.zero)
			{
				return false;
			}
			Vector3 val = (fixYAxis ? Vector3.up : trigger.up);
			Quaternion val2 = Quaternion.LookRotation(forward, val);
			Vector3 val3 = trigger.position + val2 * offset3D;
			Vector3 val4 = (orbit ? trigger.position : val3);
			Vector3 val5 = character.position - val4;
			Vector3.OrthoNormalize(ref val, ref val5);
			Vector3 val6 = val5;
			Vector3 val7 = Vector3.Project(character.position - val4, val5);
			val5 = val6 * val7.magnitude;
			if (orbit)
			{
				float magnitude = offset.magnitude;
				float magnitude2 = val5.magnitude;
				if (magnitude2 < magnitude - radius || magnitude2 > magnitude + radius)
				{
					return false;
				}
			}
			else if (val5.magnitude > radius)
			{
				return false;
			}
			Vector3 val8 = val2 * direction3D;
			Vector3.OrthoNormalize(ref val, ref val8);
			if (orbit)
			{
				Vector3 val9 = val3 - trigger.position;
				if (val9 == Vector3.zero)
				{
					val9 = Vector3.forward;
				}
				val5 = Quaternion.Inverse(Quaternion.LookRotation(val9, val)) * val5;
				val8 = Quaternion.AngleAxis(Mathf.Atan2(val5.x, val5.z) * 57.29578f, val) * val8;
			}
			float num = Vector3.Angle(val8, character.forward);
			if (num > maxAngle)
			{
				return false;
			}
			error = num / maxAngle * 180f;
			return true;
		}
	}

	[Serializable]
	public class CameraPosition
	{
		[Tooltip("What the camera should be looking at to trigger the interaction? If null, this camera position will not be used.")]
		public Collider lookAtTarget;

		[Tooltip("The direction from the lookAtTarget towards the camera (in lookAtTarget's space).")]
		public Vector3 direction = -Vector3.forward;

		[Tooltip("Max distance from the lookAtTarget to the camera.")]
		public float maxDistance = 0.5f;

		[Tooltip("Max angle between the direction and the direction towards the camera.")]
		[Range(0f, 180f)]
		public float maxAngle = 45f;

		[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.")]
		public bool fixYAxis;

		public Quaternion GetRotation()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			Vector3 forward = ((Component)lookAtTarget).transform.forward;
			if (fixYAxis)
			{
				forward.y = 0f;
			}
			if (forward == Vector3.zero)
			{
				return Quaternion.identity;
			}
			Vector3 val = (fixYAxis ? Vector3.up : ((Component)lookAtTarget).transform.up);
			return Quaternion.LookRotation(forward, val);
		}

		public bool IsInRange(Transform raycastFrom, RaycastHit hit, Transform trigger, out float error)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			error = 0f;
			if ((Object)(object)lookAtTarget == (Object)null)
			{
				return true;
			}
			error = 180f;
			if ((Object)(object)raycastFrom == (Object)null)
			{
				return false;
			}
			if ((Object)(object)hit.collider != (Object)(object)lookAtTarget)
			{
				return false;
			}
			if (hit.distance > maxDistance)
			{
				return false;
			}
			if (direction == Vector3.zero)
			{
				return false;
			}
			if (maxDistance <= 0f)
			{
				return false;
			}
			if (maxAngle <= 0f)
			{
				return false;
			}
			Vector3 val = GetRotation() * direction;
			float num = Vector3.Angle(raycastFrom.position - hit.point, val);
			if (num > maxAngle)
			{
				return false;
			}
			error = num / maxAngle * 180f;
			return true;
		}

		public CameraPosition()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Range
	{
		[Serializable]
		public class Interaction
		{
			[Tooltip("The InteractionObject to interact with.")]
			public InteractionObject interactionObject;

			[Tooltip("The effectors to interact with.")]
			public FullBodyBipedEffector[] effectors;
		}

		[HideInInspector]
		public string name;

		[HideInInspector]
		public bool show = true;

		[Tooltip("The range for the character's position and rotation.")]
		public CharacterPosition characterPosition;

		[Tooltip("The range for the character camera's position and rotation.")]
		public CameraPosition cameraPosition;

		[Tooltip("Definitions of the interactions associated with this range.")]
		public Interaction[] interactions;

		public bool IsInRange(Transform character, Transform raycastFrom, RaycastHit raycastHit, Transform trigger, out float maxError)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			maxError = 0f;
			float error = 0f;
			float error2 = 0f;
			if (!characterPosition.IsInRange(character, trigger, out error))
			{
				return false;
			}
			if (!cameraPosition.IsInRange(raycastFrom, raycastHit, trigger, out error2))
			{
				return false;
			}
			maxError = Mathf.Max(error, error2);
			return true;
		}
	}

	[Tooltip("The valid ranges of the character's and/or its camera's position for triggering interaction when the character is in contact with the collider of this trigger.")]
	public Range[] ranges = new Range[0];

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_trigger.html");
	}

	[ContextMenu("TUTORIAL VIDEO")]
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

	public int GetBestRangeIndex(Transform character, Transform raycastFrom, RaycastHit raycastHit)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)this).GetComponent<Collider>() == (Object)null)
		{
			Warning.Log("Using the InteractionTrigger requires a Collider component.", ((Component)this).transform);
			return -1;
		}
		int result = -1;
		float num = 180f;
		float maxError = 0f;
		for (int i = 0; i < ranges.Length; i++)
		{
			if (ranges[i].IsInRange(character, raycastFrom, raycastHit, ((Component)this).transform, out maxError) && maxError <= num)
			{
				num = maxError;
				result = i;
			}
		}
		return result;
	}
}
