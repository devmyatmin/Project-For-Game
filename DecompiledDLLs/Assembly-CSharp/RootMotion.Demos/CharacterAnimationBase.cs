using UnityEngine;

namespace RootMotion.Demos;

public abstract class CharacterAnimationBase : MonoBehaviour
{
	public bool smoothFollow = true;

	public float smoothFollowSpeed = 20f;

	protected bool animatePhysics;

	private Vector3 lastPosition;

	private Vector3 localPosition;

	private Quaternion localRotation;

	private Quaternion lastRotation;

	public virtual bool animationGrounded => true;

	public virtual Vector3 GetPivotPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public float GetAngleFromForward(Vector3 worldDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformDirection(worldDirection);
		return Mathf.Atan2(val.x, val.z) * 57.29578f;
	}

	protected virtual void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)((Component)this).transform.parent).GetComponent<CharacterBase>() == (Object)null)
		{
			Debug.LogWarning((object)"Animation controllers should be parented to character controllers!", (Object)(object)((Component)this).transform);
		}
		lastPosition = ((Component)this).transform.position;
		localPosition = ((Component)this).transform.localPosition;
		lastRotation = ((Component)this).transform.rotation;
		localRotation = ((Component)this).transform.localRotation;
	}

	protected virtual void LateUpdate()
	{
		if (!animatePhysics)
		{
			SmoothFollow();
		}
	}

	protected virtual void FixedUpdate()
	{
		if (animatePhysics)
		{
			SmoothFollow();
		}
	}

	private void SmoothFollow()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (smoothFollow)
		{
			((Component)this).transform.position = Vector3.Lerp(lastPosition, ((Component)this).transform.parent.TransformPoint(localPosition), Time.deltaTime * smoothFollowSpeed);
			((Component)this).transform.rotation = Quaternion.Lerp(lastRotation, ((Component)this).transform.parent.rotation * localRotation, Time.deltaTime * smoothFollowSpeed);
		}
		else
		{
			((Component)this).transform.localPosition = localPosition;
			((Component)this).transform.localRotation = localRotation;
		}
		lastPosition = ((Component)this).transform.position;
		lastRotation = ((Component)this).transform.rotation;
	}
}
