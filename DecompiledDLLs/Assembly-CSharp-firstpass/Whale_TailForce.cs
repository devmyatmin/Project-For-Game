using System.Collections;
using UnityEngine;

public class Whale_TailForce : MonoBehaviour
{
	public IEnumerator DiveForce()
	{
		yield return (object)new WaitForSeconds(4f);
		float num = 15f;
		float num2 = 150f;
		Vector3 position = ((Component)this).transform.position;
		Collider[] array = Physics.OverlapSphere(position, num);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody component = ((Component)array[i]).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.AddExplosionForce(num2, position, num, 3f);
			}
		}
	}
}
