using Beam;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
	[SerializeField]
	private float explosionRadius = 5f;

	[SerializeField]
	private float explosionPower = 10f;

	[SerializeField]
	private float shakeDuration = 0.5f;

	[SerializeField]
	private float shakeMagnitude = 0.05f;

	private void Start()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		PlayerRegistry.LocalPlayer.PlayerCamera.CameraMovement.ShakeCamera(shakeDuration, shakeMagnitude);
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, explosionRadius);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody component = ((Component)array[i]).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.AddExplosionForce(explosionPower, ((Component)this).transform.position, explosionRadius, 3f);
			}
		}
	}
}
