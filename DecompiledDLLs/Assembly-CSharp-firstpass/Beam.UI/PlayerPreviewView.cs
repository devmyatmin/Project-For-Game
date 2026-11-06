using Beam.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class PlayerPreviewView : PlayerPreviewViewBase
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private GameObject _maleCharacterContainer;

	[SerializeField]
	private GameObject _femaleCharacterContainer;

	private void Awake()
	{
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	public override void SetState(PlayerModelPreviewFlags flags)
	{
		bool flag = flags.HasFlag(PlayerModelPreviewFlags.Male);
		_maleCharacterContainer.SetActive(flag);
		_femaleCharacterContainer.SetActive(!flag);
	}

	public override void Show()
	{
		_container.SetActive(true);
		base.Show();
	}

	public override void Hide()
	{
		_container.SetActive(false);
		base.Hide();
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 1)
		{
			if ((Object)(object)_camera != (Object)null)
			{
				BeamTeam.Destroy<GameObject>(((Component)_camera).gameObject);
			}
			if ((Object)(object)_canvas != (Object)null)
			{
				BeamTeam.Destroy<GameObject>(((Component)_canvas).gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}
}
