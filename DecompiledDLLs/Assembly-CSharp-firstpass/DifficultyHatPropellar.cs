using UnityEngine;

public class DifficultyHatPropellar : MonoBehaviour
{
	[SerializeField]
	private Transform _hatPropellar;

	private void Update()
	{
		float num = 80f * Time.deltaTime;
		((Component)_hatPropellar).transform.Rotate(0f, num, 0f);
	}
}
