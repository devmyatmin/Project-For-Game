using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionEffector
{
	private Poser poser;

	private IKEffector effector;

	private float timer;

	private float length;

	private float weight;

	private float fadeInSpeed;

	private float defaultPositionWeight;

	private float defaultRotationWeight;

	private float defaultPull;

	private float defaultReach;

	private float defaultPush;

	private float defaultPushParent;

	private float defaultBendGoalWeight;

	private float resetTimer;

	private bool positionWeightUsed;

	private bool rotationWeightUsed;

	private bool pullUsed;

	private bool reachUsed;

	private bool pushUsed;

	private bool pushParentUsed;

	private bool bendGoalWeightUsed;

	private bool pickedUp;

	private bool defaults;

	private bool pickUpOnPostFBBIK;

	private Vector3 pickUpPosition;

	private Vector3 pausePositionRelative;

	private Quaternion pickUpRotation;

	private Quaternion pauseRotationRelative;

	private InteractionTarget interactionTarget;

	private Transform target;

	private List<bool> triggered = new List<bool>();

	private InteractionSystem interactionSystem;

	private bool started;

	public FullBodyBipedEffector effectorType { get; private set; }

	public bool isPaused { get; private set; }

	public InteractionObject interactionObject { get; private set; }

	public bool inInteraction => (Object)(object)interactionObject != (Object)null;

	public float progress
	{
		get
		{
			if (!inInteraction)
			{
				return 0f;
			}
			if (length == 0f)
			{
				return 0f;
			}
			return timer / length;
		}
	}

	public InteractionEffector(FullBodyBipedEffector effectorType)
	{
		this.effectorType = effectorType;
	}

	public void Initiate(InteractionSystem interactionSystem)
	{
		this.interactionSystem = interactionSystem;
		effector = interactionSystem.ik.solver.GetEffector(effectorType);
		poser = ((Component)effector.bone).GetComponent<Poser>();
		StoreDefaults();
	}

	private void StoreDefaults()
	{
		defaultPositionWeight = interactionSystem.ik.solver.GetEffector(effectorType).positionWeight;
		defaultRotationWeight = interactionSystem.ik.solver.GetEffector(effectorType).rotationWeight;
		defaultPull = interactionSystem.ik.solver.GetChain(effectorType).pull;
		defaultReach = interactionSystem.ik.solver.GetChain(effectorType).reach;
		defaultPush = interactionSystem.ik.solver.GetChain(effectorType).push;
		defaultPushParent = interactionSystem.ik.solver.GetChain(effectorType).pushParent;
		defaultBendGoalWeight = interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight;
	}

	public bool ResetToDefaults(float speed, float deltaTime)
	{
		if (inInteraction)
		{
			return false;
		}
		if (isPaused)
		{
			return false;
		}
		if (defaults)
		{
			return false;
		}
		resetTimer = Mathf.MoveTowards(resetTimer, 0f, deltaTime * speed);
		if (effector.isEndEffector)
		{
			if (pullUsed)
			{
				interactionSystem.ik.solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, interactionSystem.ik.solver.GetChain(effectorType).pull, resetTimer);
			}
			if (reachUsed)
			{
				interactionSystem.ik.solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, interactionSystem.ik.solver.GetChain(effectorType).reach, resetTimer);
			}
			if (pushUsed)
			{
				interactionSystem.ik.solver.GetChain(effectorType).push = Mathf.Lerp(defaultPush, interactionSystem.ik.solver.GetChain(effectorType).push, resetTimer);
			}
			if (pushParentUsed)
			{
				interactionSystem.ik.solver.GetChain(effectorType).pushParent = Mathf.Lerp(defaultPushParent, interactionSystem.ik.solver.GetChain(effectorType).pushParent, resetTimer);
			}
			if (bendGoalWeightUsed)
			{
				interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight = Mathf.Lerp(defaultBendGoalWeight, interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight, resetTimer);
			}
		}
		if (positionWeightUsed)
		{
			effector.positionWeight = Mathf.Lerp(defaultPositionWeight, effector.positionWeight, resetTimer);
		}
		if (rotationWeightUsed)
		{
			effector.rotationWeight = Mathf.Lerp(defaultRotationWeight, effector.rotationWeight, resetTimer);
		}
		if (resetTimer <= 0f)
		{
			pullUsed = false;
			reachUsed = false;
			pushUsed = false;
			pushParentUsed = false;
			positionWeightUsed = false;
			rotationWeightUsed = false;
			bendGoalWeightUsed = false;
			defaults = true;
		}
		return true;
	}

	public bool Pause()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!inInteraction)
		{
			return false;
		}
		isPaused = true;
		pausePositionRelative = target.InverseTransformPoint(effector.position);
		pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;
		if (interactionSystem.OnInteractionPause != null)
		{
			interactionSystem.OnInteractionPause(effectorType, interactionObject);
		}
		return true;
	}

	public bool Resume()
	{
		if (!inInteraction)
		{
			return false;
		}
		isPaused = false;
		if (interactionSystem.OnInteractionResume != null)
		{
			interactionSystem.OnInteractionResume(effectorType, interactionObject);
		}
		return true;
	}

	public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
	{
		target = interactionObject.GetTarget(effectorType, tag);
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		interactionTarget = ((Component)target).GetComponent<InteractionTarget>();
		return Start(interactionObject, interactionTarget, fadeInTime, interrupt);
	}

	public bool Start(InteractionObject interactionObject, InteractionTarget interactionTarget, float fadeInTime, bool interrupt)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		this.interactionTarget = interactionTarget;
		if (!inInteraction)
		{
			effector.position = effector.bone.position;
			effector.rotation = effector.bone.rotation;
		}
		else
		{
			if (!interrupt)
			{
				return false;
			}
			defaults = false;
		}
		if ((Object)(object)interactionTarget == (Object)null)
		{
			return false;
		}
		target = ((Component)interactionTarget).transform;
		this.interactionObject = interactionObject;
		if (interactionSystem.OnInteractionStart != null)
		{
			interactionSystem.OnInteractionStart(effectorType, interactionObject);
		}
		interactionObject.OnStartInteraction(interactionSystem);
		triggered.Clear();
		for (int i = 0; i < interactionObject.events.Length; i++)
		{
			triggered.Add(item: false);
		}
		if ((Object)(object)poser != (Object)null)
		{
			if ((Object)(object)poser.poseRoot == (Object)null)
			{
				poser.weight = 0f;
			}
			if ((Object)(object)interactionTarget != (Object)null)
			{
				poser.poseRoot = ((Component)target).transform;
			}
			else
			{
				poser.poseRoot = null;
			}
			poser.AutoMapping();
		}
		positionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
		rotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
		pullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
		reachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
		pushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
		pushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
		bendGoalWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.BendGoalWeight);
		if (defaults)
		{
			StoreDefaults();
		}
		timer = 0f;
		weight = 0f;
		fadeInSpeed = ((fadeInTime > 0f) ? (1f / fadeInTime) : 1000f);
		length = interactionObject.length;
		isPaused = false;
		pickedUp = false;
		pickUpPosition = Vector3.zero;
		pickUpRotation = Quaternion.identity;
		if ((Object)(object)interactionTarget != (Object)null)
		{
			interactionTarget.RotateTo(effector.bone);
		}
		started = true;
		return true;
	}

	public void Update(Transform root, float speed, float deltaTime)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		if (!inInteraction)
		{
			if (started)
			{
				isPaused = false;
				pickedUp = false;
				defaults = false;
				resetTimer = 1f;
				started = false;
			}
			return;
		}
		if ((Object)(object)interactionTarget != (Object)null && !interactionTarget.rotateOnce)
		{
			interactionTarget.RotateTo(effector.bone);
		}
		if (isPaused)
		{
			if (!pickedUp)
			{
				effector.position = target.TransformPoint(pausePositionRelative);
				effector.rotation = target.rotation * pauseRotationRelative;
			}
			interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight);
			return;
		}
		timer += deltaTime * speed * (((Object)(object)interactionTarget != (Object)null) ? interactionTarget.interactionSpeedMlp : 1f);
		weight = Mathf.Clamp(weight + deltaTime * fadeInSpeed * speed, 0f, 1f);
		bool pickUp = false;
		bool pause = false;
		TriggerUntriggeredEvents(checkTime: true, out pickUp, out pause);
		Vector3 val = (pickedUp ? ((Component)interactionSystem).transform.TransformPoint(pickUpPosition) : target.position);
		Quaternion val2 = (pickedUp ? (((Component)interactionSystem).transform.rotation * pickUpRotation) : target.rotation);
		effector.position = Vector3.Lerp(effector.bone.position, val, weight);
		effector.rotation = Quaternion.Lerp(effector.bone.rotation, val2, weight);
		interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight);
		if (pickUp)
		{
			PickUp(root);
		}
		if (pause)
		{
			Pause();
		}
		float value = interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, interactionTarget, timer);
		if ((Object)(object)poser != (Object)null)
		{
			poser.weight = Mathf.Lerp(poser.weight, value, weight);
		}
		else if (value > 0f)
		{
			Warning.Log("InteractionObject " + ((Object)interactionObject).name + " has a curve/multipler for Poser Weight, but the bone of effector " + effectorType.ToString() + " has no HandPoser/GenericPoser attached.", effector.bone);
		}
		if (timer >= length)
		{
			Stop();
		}
	}

	private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause)
	{
		pickUp = false;
		pause = false;
		for (int i = 0; i < triggered.Count; i++)
		{
			if (triggered[i] || (checkTime && !(interactionObject.events[i].time < timer)))
			{
				continue;
			}
			interactionObject.events[i].Activate(effector.bone);
			if (interactionObject.events[i].pickUp)
			{
				if (timer >= interactionObject.events[i].time)
				{
					timer = interactionObject.events[i].time;
				}
				pickUp = true;
			}
			if (interactionObject.events[i].pause)
			{
				if (timer >= interactionObject.events[i].time)
				{
					timer = interactionObject.events[i].time;
				}
				pause = true;
			}
			if (interactionSystem.OnInteractionEvent != null)
			{
				interactionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
			}
			triggered[i] = true;
		}
	}

	private void PickUp(Transform root)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		pickUpPosition = root.InverseTransformPoint(effector.position);
		pickUpRotation = Quaternion.Inverse(((Component)interactionSystem).transform.rotation) * effector.rotation;
		pickUpOnPostFBBIK = true;
		pickedUp = true;
		Rigidbody component = ((Component)interactionObject.targetsRoot).GetComponent<Rigidbody>();
		if ((Object)(object)component != (Object)null)
		{
			if (!component.isKinematic)
			{
				component.isKinematic = true;
			}
			Collider component2 = ((Component)root).GetComponent<Collider>();
			if ((Object)(object)component2 != (Object)null)
			{
				Collider[] componentsInChildren = ((Component)interactionObject.targetsRoot).GetComponentsInChildren<Collider>();
				foreach (Collider val in componentsInChildren)
				{
					if (!val.isTrigger && val.enabled)
					{
						Physics.IgnoreCollision(component2, val);
					}
				}
			}
		}
		if (interactionSystem.OnInteractionPickUp != null)
		{
			interactionSystem.OnInteractionPickUp(effectorType, interactionObject);
		}
	}

	public bool Stop()
	{
		if (!inInteraction)
		{
			return false;
		}
		bool pickUp = false;
		bool pause = false;
		TriggerUntriggeredEvents(checkTime: false, out pickUp, out pause);
		if (interactionSystem.OnInteractionStop != null)
		{
			interactionSystem.OnInteractionStop(effectorType, interactionObject);
		}
		if ((Object)(object)interactionTarget != (Object)null)
		{
			interactionTarget.ResetRotation();
		}
		interactionObject = null;
		weight = 0f;
		timer = 0f;
		isPaused = false;
		target = null;
		defaults = false;
		resetTimer = 1f;
		pickedUp = false;
		started = false;
		return true;
	}

	public void OnPostFBBIK()
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (inInteraction)
		{
			float num = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;
			if (num > 0f)
			{
				Quaternion val = (pickedUp ? (((Component)interactionSystem).transform.rotation * pickUpRotation) : effector.rotation);
				Quaternion val2 = Quaternion.Slerp(effector.bone.rotation, val, num * num);
				effector.bone.localRotation = Quaternion.Inverse(effector.bone.parent.rotation) * val2;
			}
			if (pickUpOnPostFBBIK)
			{
				Vector3 position = effector.bone.position;
				effector.bone.position = ((Component)interactionSystem).transform.TransformPoint(pickUpPosition);
				interactionObject.targetsRoot.parent = effector.bone;
				effector.bone.position = position;
				pickUpOnPostFBBIK = false;
			}
		}
	}
}
