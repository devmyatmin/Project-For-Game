using System.Collections;
using Beam.Rendering;
using Beam.Serialization.Json;
using Beam.Utilities;
using Ceto;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam;

public class CaptureMode : MonoBehaviour
{
	private bool m_showGui;

	private GameObject m_camera;

	private void Awake()
	{
		Options.Loaded += Options_Loaded;
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private void OnDestroy()
	{
		Options.Loaded -= Options_Loaded;
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			((MonoBehaviour)this).StartCoroutine(Init_Sequence());
		}
	}

	private IEnumerator Init_Sequence()
	{
		yield return (object)new WaitForSeconds(2f);
		LodManager.LOD_BIAS = 100f;
		float[] layerCullDistances = new float[32];
		foreach (IGameCamera allCamera in Cameras.AllCameras)
		{
			allCamera.Camera.layerCullDistances = layerCullDistances;
		}
		ProjectedGrid projectedGrid = Object.FindObjectOfType<ProjectedGrid>();
		if ((Object)(object)projectedGrid != (Object)null)
		{
			projectedGrid.resolution = MESH_RESOLUTION.ULTRA;
		}
		WaveSpectrum waveSpectrum = Object.FindObjectOfType<WaveSpectrum>();
		if ((Object)(object)waveSpectrum != (Object)null)
		{
			waveSpectrum.fourierSize = FOURIER_SIZE.HIGH_128_CPU;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)93))
		{
			m_showGui = !m_showGui;
		}
		if ((Object)(object)Terrain.activeTerrain != (Object)null)
		{
			Terrain.activeTerrain.heightmapPixelError = 1f;
		}
	}

	private void Options_Loaded(JObject data)
	{
		Options.GraphicsSettings = GraphicsPreset.ULTRA;
		Options.InputSettings.MouseSmoothness = 1f;
	}

	private void OnGUI()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (m_showGui)
		{
			string text = (((Object)(object)m_camera == (Object)null) ? "ENABLE CAPTURE CAMERA" : "DISABLE CAPTURE CAMERA");
			if (GUI.Button(new Rect(20f, 20f, 200f, 60f), text))
			{
				ToggleCaptureCamera();
			}
			if (GUI.Button(new Rect(20f, 100f, 200f, 60f), "Toggle Flocks"))
			{
				ToggleFlocks();
			}
			if (GUI.Button(new Rect(20f, 180f, 200f, 60f), "Toggle Weather"))
			{
				ToggleWeather();
			}
			if (GUI.Button(new Rect(20f, 260f, 200f, 60f), "Toggle Players"))
			{
				TogglePlayers();
			}
			if (GUI.Button(new Rect(20f, 340f, 200f, 60f), "Toggle Player UI"))
			{
				TogglePlayerUI();
			}
			if (GUI.Button(new Rect(20f, 420f, 200f, 60f), "Toggle Game UI"))
			{
				ToggleGameUI();
			}
			if (GUI.Button(new Rect(20f, 500f, 200f, 60f), "Toggle Pause Time"))
			{
				TogglePauseTime();
			}
			if (GUI.Button(new Rect(20f, 580f, 200f, 60f), "TOGGLE ALL"))
			{
				ToggleAll();
			}
		}
	}

	private void ToggleCaptureCamera()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_camera != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(m_camera.gameObject);
			return;
		}
		GameObject val = Resources.Load<GameObject>("Prefabs/CAPTURE_CAMERA");
		if ((Object)(object)val != (Object)null)
		{
			Vector3 val2 = Vector3.zero;
			IPlayer player = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(PlayerRegistry.AllPlayers);
			if (!BeamTeam.IsNullOrDestroyed(player))
			{
				val2 = ((Component)player.PlayerCamera.Camera).transform.position;
			}
			m_camera = Object.Instantiate<GameObject>(val, val2, Quaternion.identity);
		}
	}

	private void ToggleFlocks()
	{
		GameObject val = GameObject.Find("Manager - Flocks");
		if (!((Object)(object)val == (Object)null))
		{
			val.gameObject.SetActive(!val.gameObject.activeSelf);
		}
	}

	private void ToggleWeather()
	{
		AtmosphereStorm instance = Singleton<AtmosphereStorm>.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			((Behaviour)instance).enabled = !((Behaviour)instance).enabled;
		}
	}

	private void TogglePlayers()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			((Component)allPlayer.transform).gameObject.SetActive(!((Component)allPlayer.transform).gameObject.activeSelf);
			((Component)allPlayer.PlayerCamera).gameObject.SetActive(!((Component)allPlayer.PlayerCamera).gameObject.activeSelf);
		}
	}

	private void TogglePlayerUI()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Camera componentInChildren = ((Component)allPlayer.PlayerUI).GetComponentInChildren<Camera>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((Behaviour)componentInChildren).enabled = !((Behaviour)componentInChildren).enabled;
			}
		}
	}

	private void ToggleGameUI()
	{
		GameObject val = GameObject.Find("GameUI");
		if (!((Object)(object)val == (Object)null))
		{
			Camera componentInChildren = val.GetComponentInChildren<Camera>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((Behaviour)componentInChildren).enabled = !((Behaviour)componentInChildren).enabled;
			}
		}
	}

	private void TogglePauseTime()
	{
		Singleton<GameTime>.Instance.Paused = !Singleton<GameTime>.Instance.Paused;
	}

	private void ToggleAll()
	{
		ToggleFlocks();
		ToggleWeather();
		TogglePlayers();
		TogglePlayerUI();
		ToggleGameUI();
	}
}
