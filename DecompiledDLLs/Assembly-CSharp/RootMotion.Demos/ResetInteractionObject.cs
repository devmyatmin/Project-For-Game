using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class ResetInteractionObject : MonoBehaviour
{
	public float resetDelay = 1f;

	private Vector3 defaultPosition;

	private Quaternion defaultRotation;

	private Transform defaultParent;

	private Rigidbody r;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		defaultPosition = ((Component)this).transform.position;
		defaultRotation = ((Component)this).transform.rotation;
		defaultParent = ((Component)this).transform.parent;
		r = ((Component)this).GetComponent<Rigidbody>();
	}

	private void OnPickUp(Transform t)
	{
		if (((Behaviour)this).enabled)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(ResetObject(Time.time + resetDelay));
		}
	}

	private IEnumerator ResetObject(float resetTime)
	{
		while (Time.time < resetTime)
		{
			yield return null;
		}
		Poser component = ((Component)((Component)this).transform.parent).GetComponent<Poser>();
		if ((Object)(object)component != (Object)null)
		{
			component.poseRoot = null;
			component.weight = 0f;
		}
		((Component)this).transform.parent = defaultParent;
		((Component)this).transform.position = defaultPosition;
		((Component)this).transform.rotation = defaultRotation;
		if ((Object)(object)r != (Object)null)
		{
			r.isKinematic = false;
		}
	}
}
