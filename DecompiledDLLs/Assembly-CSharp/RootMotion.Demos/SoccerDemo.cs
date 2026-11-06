using System.Collections;
using UnityEngine;

namespace RootMotion.Demos;

public class SoccerDemo : MonoBehaviour
{
	private Animator animator;

	private Vector3 defaultPosition;

	private Quaternion defaultRotation;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		animator = ((Component)this).GetComponent<Animator>();
		defaultPosition = ((Component)this).transform.position;
		defaultRotation = ((Component)this).transform.rotation;
		((MonoBehaviour)this).StartCoroutine(ResetDelayed());
	}

	private IEnumerator ResetDelayed()
	{
		while (true)
		{
			yield return (object)new WaitForSeconds(3f);
			((Component)this).transform.position = defaultPosition;
			((Component)this).transform.rotation = defaultRotation;
			animator.CrossFade("SoccerKick", 0f, 0, 0f);
			yield return null;
		}
	}
}
