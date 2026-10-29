using UnityEngine;

public class WollieSpawner : MonoBehaviour
{
	[SerializeField]
	private Transform _wolliePosition;

	public Transform WolliePosition => _wolliePosition;
}
