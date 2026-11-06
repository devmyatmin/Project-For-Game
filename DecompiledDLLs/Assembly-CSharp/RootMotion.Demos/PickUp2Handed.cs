using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public abstract class PickUp2Handed : MonoBehaviour
{
	public int GUIspace;

	public InteractionSystem interactionSystem;

	public InteractionObject obj;

	public Transform pivot;

	public Transform holdPoint;

	public float pickUpTime = 0.3f;

	private float holdWeight;

	private float holdWeightVel;

	private Vector3 pickUpPosition;

	private Quaternion pickUpRotation;

	private bool holding
	{
		get
		{
			if (!holdingLeft)
			{
				return holdingRight;
			}
			return true;
		}
	}

	private bool holdingLeft
	{
		get
		{
			if (interactionSystem.IsPaused(FullBodyBipedEffector.LeftHand))
			{
				return (Object)(object)interactionSystem.GetInteractionObject(FullBodyBipedEffector.LeftHand) == (Object)(object)obj;
			}
			return false;
		}
	}

	private bool holdingRight
	{
		get
		{
			if (interactionSystem.IsPaused(FullBodyBipedEffector.RightHand))
			{
				return (Object)(object)interactionSystem.GetInteractionObject(FullBodyBipedEffector.RightHand) == (Object)(object)obj;
			}
			return false;
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space((float)GUIspace);
		if (!holding)
		{
			if (GUILayout.Button("Pick Up " + ((Object)obj).name, Array.Empty<GUILayoutOption>()))
			{
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, obj, interrupt: false);
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, obj, interrupt: false);
			}
		}
		else
		{
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			if (holdingRight && GUILayout.Button("Release Right", Array.Empty<GUILayoutOption>()))
			{
				interactionSystem.ResumeInteraction(FullBodyBipedEffector.RightHand);
			}
			if (holdingLeft && GUILayout.Button("Release Left", Array.Empty<GUILayoutOption>()))
			{
				interactionSystem.ResumeInteraction(FullBodyBipedEffector.LeftHand);
			}
			if (GUILayout.Button("Drop " + ((Object)obj).name, Array.Empty<GUILayoutOption>()))
			{
				interactionSystem.ResumeAll();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	}

	protected abstract void RotatePivot();

	private void Start()
	{
		InteractionSystem interactionSystem = this.interactionSystem;
		interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnStart));
		InteractionSystem interactionSystem2 = this.interactionSystem;
		interactionSystem2.OnInteractionPause = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem2.OnInteractionPause, new InteractionSystem.InteractionDelegate(OnPause));
		InteractionSystem interactionSystem3 = this.interactionSystem;
		interactionSystem3.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Combine(interactionSystem3.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnDrop));
	}

	private void OnPause(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (effectorType == FullBodyBipedEffector.LeftHand && !((Object)(object)interactionObject != (Object)(object)obj))
		{
			((Component)obj).transform.parent = ((Component)interactionSystem).transform;
			Rigidbody component = ((Component)obj).GetComponent<Rigidbody>();
			if ((Object)(object)component != (Object)null)
			{
				component.isKinematic = true;
			}
			pickUpPosition = ((Component)obj).transform.position;
			pickUpRotation = ((Component)obj).transform.rotation;
			holdWeight = 0f;
			holdWeightVel = 0f;
		}
	}

	private void OnStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (effectorType == FullBodyBipedEffector.LeftHand && !((Object)(object)interactionObject != (Object)(object)obj))
		{
			RotatePivot();
			holdPoint.rotation = ((Component)obj).transform.rotation;
		}
	}

	private void OnDrop(FullBodyBipedEffector effectorType, InteractionObject interactionObject)
	{
		if (!holding && !((Object)(object)interactionObject != (Object)(object)obj))
		{
			((Component)obj).transform.parent = null;
			if ((Object)(object)((Component)obj).GetComponent<Rigidbody>() != (Object)null)
			{
				((Component)obj).GetComponent<Rigidbody>().isKinematic = false;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (holding)
		{
			holdWeight = Mathf.SmoothDamp(holdWeight, 1f, ref holdWeightVel, pickUpTime);
			((Component)obj).transform.position = Vector3.Lerp(pickUpPosition, holdPoint.position, holdWeight);
			((Component)obj).transform.rotation = Quaternion.Lerp(pickUpRotation, holdPoint.rotation, holdWeight);
		}
	}

	private void OnDestroy()
	{
		if (!((Object)(object)this.interactionSystem == (Object)null))
		{
			InteractionSystem interactionSystem = this.interactionSystem;
			interactionSystem.OnInteractionStart = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem.OnInteractionStart, new InteractionSystem.InteractionDelegate(OnStart));
			InteractionSystem interactionSystem2 = this.interactionSystem;
			interactionSystem2.OnInteractionPause = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem2.OnInteractionPause, new InteractionSystem.InteractionDelegate(OnPause));
			InteractionSystem interactionSystem3 = this.interactionSystem;
			interactionSystem3.OnInteractionResume = (InteractionSystem.InteractionDelegate)Delegate.Remove(interactionSystem3.OnInteractionResume, new InteractionSystem.InteractionDelegate(OnDrop));
		}
	}
}
