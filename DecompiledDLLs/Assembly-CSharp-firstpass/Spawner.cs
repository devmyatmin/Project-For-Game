using Beam.Utilities;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private Transform[] _positions;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _positions.Length; i++)
		{
			Object.Instantiate<GameObject>(_prefab, _positions[i].position, Quaternion.identity);
		}
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}
}
