using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Buoyancy/BuoyantStructure")]
public class BuoyantStructure : MonoBehaviour
{
	public float maxAngularVelocity = 0.05f;

	private Buoyancy[] m_buoyancy;

	private void Start()
	{
		m_buoyancy = ((Component)this).GetComponentsInChildren<Buoyancy>();
		int num = m_buoyancy.Length;
		for (int i = 0; i < num; i++)
		{
			m_buoyancy[i].PartOfStructure = true;
		}
	}

	private void FixedUpdate()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
		}
		float num = 0f;
		int num2 = m_buoyancy.Length;
		for (int i = 0; i < num2; i++)
		{
			if (((Behaviour)m_buoyancy[i]).enabled)
			{
				m_buoyancy[i].UpdateProperties();
				num += m_buoyancy[i].Mass;
			}
		}
		val.mass = num;
		Vector3 position = ((Component)this).transform.position;
		Vector3 val2 = Vector3.zero;
		Vector3 val3 = Vector3.zero;
		for (int j = 0; j < num2; j++)
		{
			if (((Behaviour)m_buoyancy[j]).enabled)
			{
				m_buoyancy[j].UpdateForces(val);
				Vector3 position2 = ((Component)m_buoyancy[j]).transform.position;
				Vector3 totalForces = m_buoyancy[j].TotalForces;
				Vector3 val4 = position2 - position;
				val2 += totalForces;
				val3 += Vector3.Cross(val4, totalForces);
			}
		}
		val.maxAngularVelocity = maxAngularVelocity;
		val.AddForce(val2);
		val.AddTorque(val3);
	}
}
