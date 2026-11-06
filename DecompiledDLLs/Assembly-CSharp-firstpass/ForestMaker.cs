using UnityEngine;

public class ForestMaker : MonoBehaviour
{
	public GameObject m_treePrefab;

	public int m_amount;

	public GameObject m_ground;

	public float m_radiusDistance;

	private void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_treePrefab == (Object)null)
		{
			return;
		}
		m_ground.transform.localScale = new Vector3((float)(m_amount * 10), 1f, (float)(m_amount * 5) * 1.866f);
		for (int i = -m_amount / 2; i <= m_amount / 2; i++)
		{
			for (int j = -m_amount / 2; j <= m_amount / 2; j++)
			{
				if (!(Random.Range(0f, 1f) > 0.5f))
				{
					GameObject val = Object.Instantiate<GameObject>(m_treePrefab);
					Vector3 zero = Vector3.zero;
					zero.x = ((float)i + (float)j * 0.5f - (float)(int)(((j < 0) ? ((float)j - 1f) : ((float)j)) / 2f)) * 2f * m_radiusDistance;
					zero.z = (float)j * 1.866f * m_radiusDistance;
					val.transform.position = zero;
					float num = Random.Range(1f, 1.5f);
					val.transform.localScale = Vector3.one * num;
					val.transform.Rotate(Random.Range(-10f, 10f), Random.Range(-180f, 180f), Random.Range(-10f, 10f));
				}
			}
		}
	}
}
