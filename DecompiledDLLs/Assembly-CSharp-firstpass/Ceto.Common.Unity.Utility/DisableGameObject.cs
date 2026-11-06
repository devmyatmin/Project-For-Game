using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class DisableGameObject : MonoBehaviour
{
	private void Update()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
