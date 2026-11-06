using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class TouchWalls : MonoBehaviour
{
	[Serializable]
	public class EffectorLink
	{
		public bool enabled = true;

		public FullBodyBipedEffector effectorType;

		public InteractionObject interactionObject;

		public Transform spherecastFrom;

		public float spherecastRadius = 0.1f;

		public float minDistance = 0.3f;

		public float distanceMlp = 1f;

		public LayerMask touchLayers;

		public float lerpSpeed = 10f;

		public float minSwitchTime = 0.2f;

		public float releaseDistance = 0.4f;

		public bool sliding;

		private Vector3 raycastDirectionLocal;

		private float raycastDistance;

		private bool inTouch;

		private RaycastHit hit;

		private Vector3 targetPosition;

		private Quaternion targetRotation;

		private bool initiated;

		private float nextSwitchTime;

		private float speedF;

		public void Initiate(InteractionSystem interactionSystem)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			raycastDirectionLocal = spherecastFrom.InverseTransformDirection(((Component)interactionObject).transform.position - spherecastFrom.position);
			raycastDistance = Vector3.Distance(spherecastFrom.position, ((Component)interactionObject).transform.position);
			interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnInteractionStart));
			interactionSystem.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnInteractionResume));
			interactionSystem.OnInteractionStop = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem.OnInteractionStop, new InteractionSystem.InteractionDelegate(OnInteractionStop));
			hit.normal = Vector3.forward;
			targetPosition = ((Component)interactionObject).transform.position;
			targetRotation = ((Component)interactionObject).transform.rotation;
			initiated = true;
		}

		private bool FindWalls(Vector3 direction)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (!enabled)
			{
				return false;
			}
			bool result = Physics.SphereCast(spherecastFrom.position, spherecastRadius, direction, ref hit, raycastDistance * distanceMlp, LayerMask.op_Implicit(touchLayers));
			if (hit.distance < minDistance)
			{
				result = false;
			}
			return result;
		}

		public void Update(InteractionSystem interactionSystem)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			if (!initiated)
			{
				return;
			}
			Vector3 val = spherecastFrom.TransformDirection(raycastDirectionLocal);
			hit.point = spherecastFrom.position + val;
			bool flag = FindWalls(val);
			if (!inTouch)
			{
				if (flag && Time.time > nextSwitchTime)
				{
					((Component)interactionObject).transform.parent = null;
					interactionSystem.StartInteraction(effectorType, interactionObject, interrupt: true);
					nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;
					targetPosition = hit.point;
					targetRotation = Quaternion.LookRotation(-hit.normal);
					((Component)interactionObject).transform.position = targetPosition;
					((Component)interactionObject).transform.rotation = targetRotation;
				}
			}
			else
			{
				if (!flag)
				{
					StopTouch(interactionSystem);
				}
				else if (!interactionSystem.IsPaused(effectorType) || sliding)
				{
					targetPosition = hit.point;
					targetRotation = Quaternion.LookRotation(-hit.normal);
				}
				if (Vector3.Distance(((Component)interactionObject).transform.position, hit.point) > releaseDistance)
				{
					if (flag)
					{
						targetPosition = hit.point;
						targetRotation = Quaternion.LookRotation(-hit.normal);
					}
					else
					{
						StopTouch(interactionSystem);
					}
				}
			}
			float num = ((!inTouch || (interactionSystem.IsPaused(effectorType) && ((Component)interactionObject).transform.position == targetPosition)) ? 0f : 1f);
			speedF = Mathf.Lerp(speedF, num, Time.deltaTime * 3f * interactionSystem.speed);
			float num2 = Time.deltaTime * lerpSpeed * speedF * interactionSystem.speed;
			((Component)interactionObject).transform.position = Vector3.Lerp(((Component)interactionObject).transform.position, targetPosition, num2);
			((Component)interactionObject).transform.rotation = Quaternion.Slerp(((Component)interactionObject).transform.rotation, targetRotation, num2);
		}

		private void StopTouch(InteractionSystem interactionSystem)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			((Component)interactionObject).transform.parent = ((Component)interactionSystem).transform;
			nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;
			if (interactionSystem.IsPaused(effectorType))
			{
				interactionSystem.ResumeInteraction(effectorType);
				return;
			}
			speedF = 0f;
			targetPosition = hit.point;
			targetRotation = Quaternion.LookRotation(-hit.normal);
		}

		private void OnInteractionStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == this.effectorType && !((Object)(object)interactionObject != (Object)(object)this.interactionObject))
			{
				inTouch = true;
			}
		}

		private void OnInteractionResume(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == this.effectorType && !((Object)(object)interactionObject != (Object)(object)this.interactionObject))
			{
				inTouch = false;
			}
		}

		private void OnInteractionStop(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
		{
			if (effectorType == this.effectorType && !((Object)(object)interactionObject != (Object)(object)this.interactionObject))
			{
				inTouch = false;
			}
		}

		public void Destroy(InteractionSystem interactionSystem)
		{
			if (initiated)
			{
				interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnInteractionStart));
				interactionSystem.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnInteractionResume));
				interactionSystem.OnInteractionStop = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem.OnInteractionStop, new InteractionSystem.InteractionDelegate(OnInteractionStop));
			}
		}
	}

	public InteractionSystem interactionSystem;

	public EffectorLink[] effectorLinks;

	private void Start()
	{
		EffectorLink[] array = effectorLinks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(interactionSystem);
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < effectorLinks.Length; i++)
		{
			effectorLinks[i].Update(interactionSystem);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)interactionSystem != (Object)null)
		{
			for (int i = 0; i < effectorLinks.Length; i++)
			{
				effectorLinks[i].Destroy(interactionSystem);
			}
		}
	}
}
