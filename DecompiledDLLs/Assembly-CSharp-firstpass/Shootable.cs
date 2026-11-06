using UnityEngine;

public class Shootable : MonoBehaviour
{
	[SerializeField]
	private DamageUnityEvent _shot;

	public DamageUnityEvent Shot => _shot;
}
