using System.Collections;
using UnityEngine;

namespace RootMotion.Demos;

public class PlatformRotator : MonoBehaviour
{
	public float maxAngle = 70f;

	public float switchRotationTime = 0.5f;

	public float random = 0.5f;

	public float rotationSpeed = 50f;

	public Vector3 movePosition;

	public float moveSpeed = 5f;

	public int characterLayer;

	private Quaternion defaultRotation;

	private Quaternion targetRotation;

	private Vector3 targetPosition;

	private Vector3 velocity;

	private Rigidbody r;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		defaultRotation = ((Component)this).transform.rotation;
		targetPosition = ((Component)this).transform.position + movePosition;
		r = ((Component)this).GetComponent<Rigidbody>();
		((MonoBehaviour)this).StartCoroutine(SwitchRotation());
	}

	private void FixedUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		r.MovePosition(Vector3.SmoothDamp(r.position, targetPosition, ref velocity, 1f, moveSpeed));
		if (Vector3.Distance(((Component)this).GetComponent<Rigidbody>().position, targetPosition) < 0.1f)
		{
			movePosition = -movePosition;
			targetPosition += movePosition;
		}
		r.MoveRotation(Quaternion.RotateTowards(r.rotation, targetRotation, rotationSpeed * Time.deltaTime));
	}

	private IEnumerator SwitchRotation()
	{
		while (true)
		{
			float num = Random.Range(0f - maxAngle, maxAngle);
			Vector3 onUnitSphere = Random.onUnitSphere;
			targetRotation = Quaternion.AngleAxis(num, onUnitSphere) * defaultRotation;
			yield return (object)new WaitForSeconds(switchRotationTime + Random.value * random);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == characterLayer)
		{
			CharacterThirdPerson component = collision.gameObject.GetComponent<CharacterThirdPerson>();
			if (!((Object)(object)component == (Object)null) && component.smoothPhysics)
			{
				component.smoothPhysics = false;
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.layer == characterLayer)
		{
			CharacterThirdPerson component = collision.gameObject.GetComponent<CharacterThirdPerson>();
			if (!((Object)(object)component == (Object)null))
			{
				component.smoothPhysics = true;
			}
		}
	}
}
