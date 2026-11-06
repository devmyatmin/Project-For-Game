using Beam.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnLoad : MonoBehaviour
{
	private void OnEnable()
	{
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 1)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}
}
