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
		for (int i = 0; i < _positions.Length; i++)
		{
			Object.Instantiate(_prefab, _positions[i].position, Quaternion.identity);
		}
		BeamTeam.Destroy(base.gameObject);
	}
}
