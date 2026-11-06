using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Events;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Photon.Bolt;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.Intro;

public class PrivateJet : MonoBehaviour
{
	[Serializable]
	public class SeatingPositionData
	{
		public Transform SittingCameraTarget;

		public Transform PlayerControllerPosition;
	}

	private GameObject _sceneContainer;

	private AudioSource _audioSource;

	private int _originalPixelLights;

	private bool _canSkipIntro;

	[SerializeField]
	private GameObject _jetCrashed;

	[SerializeField]
	private GameObject _jetNonCrashed;

	[SerializeField]
	private Transform _MpCharacterPos;

	[SerializeField]
	private SeatingPositionData[] _seatingPositionsData;

	[SerializeField]
	private GameObject _jetComplete;

	[SerializeField]
	private GameObject _jetDamaged;

	[SerializeField]
	private GameObject _explosion;

	[SerializeField]
	private Transform _explosionPos;

	[SerializeField]
	private GameObject _monitor;

	[SerializeField]
	private GameObject _backLights;

	[SerializeField]
	private Light _backLight;

	[SerializeField]
	private Transform _playerCrashPosition;

	[SerializeField]
	private AudioSource _randomTurbulence;

	[SerializeField]
	private AudioSource _JetAmbience;

	[SerializeField]
	private AudioSource _jetExplosion;

	[SerializeField]
	private AudioClip _wakeAudio;

	[SerializeField]
	private AudioClip _crashAudio;

	[SerializeField]
	private AudioClip _beltWarning;

	[SerializeField]
	private AudioClip _endGameCrashAudio;

	[SerializeField]
	private GameObject _crashedMonitor;

	[SerializeField]
	private Transform _underwaterProps;

	[SerializeField]
	private GameObject explosion;

	[SerializeField]
	private GameObject[] explosionPositions;

	[SerializeField]
	private AudioSource _jetEngine;

	[SerializeField]
	private Transform _finalExplosionPos;

	[SerializeField]
	private GameObject _femaleDummy;

	[SerializeField]
	private GameObject _maleDummy;

	[SerializeField]
	private GameObject _femaleDummy2;

	[SerializeField]
	private GameObject _maleDummy2;

	[SerializeField]
	private GameObject _rubberDuckEasterEgg;

	private CoroutineHandle _loadNewGame;

	private CoroutineHandle _wakeUpSequence;

	private CoroutineHandle _loosingAltitudeSequence;

	private CoroutineHandle _toggleLightingSequence;

	private CoroutineHandle _crashImpactWaterSequence;

	private CoroutineHandle _landedInWaterSequence;

	private CoroutineHandle _landingCrashExplosionsSequence;

	private CoroutineHandle _endSceneSequence;

	private bool _time;

	private float _timer;

	private bool _canPause = true;

	private void PauseMusic(MainMenuOpenedEvent e)
	{
		if (e.Opened)
		{
			_jetExplosion.Pause();
		}
		else if (!e.Opened)
		{
			_jetExplosion.UnPause();
		}
	}

	private void SetupCharacterDummy()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		bool flag = PlayerUtilities.GetServerCharacterGender() == EGameGenderMode.Male;
		_maleDummy.SetActive(flag);
		_femaleDummy.SetActive(!flag);
		if (MPExtensions.IsSinglePlayer(Game.Mode))
		{
			_maleDummy2.SetActive(false);
			_femaleDummy2.SetActive(false);
		}
		else
		{
			if ((!MPExtensions.IsMultiplayer(Game.Mode) && !MPExtensions.IsLocalCoop(Game.Mode)) || PlayerRegistry.AllPlayers.Count <= 1)
			{
				return;
			}
			bool flag2 = PlayerUtilities.GetClientCharacterGender() == EGameGenderMode.Male;
			_maleDummy2.SetActive(flag2);
			_femaleDummy2.SetActive(!flag2);
			GameObject val = (flag2 ? _maleDummy2 : _femaleDummy2);
			val.transform.position = _MpCharacterPos.position;
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				if (MPExtensions.IsServer(Game.Mode))
				{
					val = ((PlayerUtilities.GetClientCharacterGender() == EGameGenderMode.Female) ? _femaleDummy2 : _maleDummy2);
				}
				else
				{
					val = ((PlayerUtilities.GetServerCharacterGender() == EGameGenderMode.Female) ? _femaleDummy : _maleDummy);
				}
				Transform val2 = global::ExtensionMethods.FindInChildren(val.transform, "head");
				if ((Object)(object)val2 != (Object)null)
				{
					((Component)val2).gameObject.SetActive(true);
				}
			}
			else
			{
				if (!MPExtensions.IsLocalCoop(Game.Mode))
				{
					return;
				}
				foreach (IPlayer player in PlayerRegistry.AllPlayers)
				{
					player.PlayerCamera.RenderingCameraChanged += (bool ourCamera) =>
					{
						if (player.Id == 0)
						{
							((Component)global::ExtensionMethods.FindInChildren(_maleDummy2.transform, "head")).gameObject.SetActive(ourCamera);
							((Component)global::ExtensionMethods.FindInChildren(_femaleDummy2.transform, "head")).gameObject.SetActive(ourCamera);
						}
						else
						{
							((Component)global::ExtensionMethods.FindInChildren(_maleDummy.transform, "head")).gameObject.SetActive(ourCamera);
							((Component)global::ExtensionMethods.FindInChildren(_femaleDummy.transform, "head")).gameObject.SetActive(ourCamera);
						}
					};
				}
			}
		}
	}

	private IEnumerator Start()
	{
		MainMenuPresenter mainMenuPresenter = Object.FindObjectOfType<MainMenuPresenter>();
		if (Object.op_Implicit((Object)(object)mainMenuPresenter))
		{
			mainMenuPresenter.QuittingToMenu += MainMenuPresenter_QuittingToMenu;
		}
		EventManager.AddListener<MainMenuOpenedEvent>(PauseMusic);
		MultiplayerMng.AddListener<GameStateChangeGlobalEvent>((MultiplayerMng.Listener<GameStateChangeGlobalEvent>)Listener_GameStateChangeGlobalEvent);
		_sceneContainer = ((Component)((Component)this).transform.parent).gameObject;
		_JetAmbience.outputAudioMixerGroup = ((AudioManager)AudioManager.GetAudioPlayer()).GetAudioMixerGroup(AudioMixerChannels.Environment);
		while (!LevelLoader.IsPlayable())
		{
			yield return null;
		}
		SetupControls();
		SetupScene();
		SetupCharacterDummy();
		if ((int)Game.State != 1)
		{
			yield break;
		}
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer.Id];
			((Component)allPlayer.PlayerCamera).transform.position = seatingPositionData.PlayerControllerPosition.position;
		}
		SetupPlayers();
		_wakeUpSequence = Timing.RunCoroutine(WakeUp());
	}

	private void OnDestroy()
	{
		MainMenuPresenter mainMenuPresenter = Object.FindObjectOfType<MainMenuPresenter>();
		if (Object.op_Implicit((Object)(object)mainMenuPresenter))
		{
			mainMenuPresenter.QuittingToMenu -= MainMenuPresenter_QuittingToMenu;
		}
		EventManager.RemoveListener<MainMenuOpenedEvent>(PauseMusic);
		MultiplayerMng.RemoveListener<GameStateChangeGlobalEvent>((MultiplayerMng.Listener<GameStateChangeGlobalEvent>)Listener_GameStateChangeGlobalEvent);
	}

	private void SetupPlayers()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.Movement.IsBusy = default;
			allPlayer.Movement.IntroSceneSetup();
			allPlayer.Statistics.Invincible = true;
			SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer.Id];
			allPlayer.PlayerCamera.CameraMovement.SetCameraTarget(seatingPositionData.SittingCameraTarget);
			allPlayer.transform.localScale = allPlayer.transform.localScale * 0.1f;
			Vector3 position = seatingPositionData.PlayerControllerPosition.position + new Vector3(0f, 0f, 10f);
			allPlayer.transform.position = position;
			allPlayer.transform.SetAsFirstSibling();
		}
	}

	private void SetupControls()
	{
		CursorUtils.Visible = false;
		CursorUtils.LockState = (CursorLockMode)1;
		_originalPixelLights = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 10;
	}

	private void Update()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Invalid comparison between Unknown and I4
		if (LevelLoader.IsLoadingScene())
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		if (player == null)
		{
			return;
		}
		if (_time)
		{
			_timer += Time.deltaTime;
			if (_timer > 0.25f || !player.GetButton(39))
			{
				_time = false;
				if (_timer < 0.25f)
				{
					if (_canPause)
					{
						MainMenuPresenter mainMenuPresenter = Object.FindObjectOfType<MainMenuPresenter>();
						if (Object.op_Implicit((Object)(object)mainMenuPresenter) && !Singleton<Fader>.Instance.IsFading)
						{
							mainMenuPresenter.Show(player);
						}
					}
				}
				else if (_canSkipIntro && !LevelLoader.IsLoading())
				{
					LoadNewGame();
				}
				return;
			}
		}
		if (LevelLoader.IsPlayable() && (int)Game.State == 1 && player.GetButtonDown(39) && _canSkipIntro)
		{
			_time = true;
			_timer = 0f;
		}
	}

	private void SetupScene()
	{
		_jetEngine.pitch = 0.75f;
		_backLights.SetActive(false);
		_audioSource = ((Component)this).GetComponent<AudioSource>();
	}

	public void SetupEndingScene()
	{
		_jetDamaged.SetActive(true);
		_jetComplete.SetActive(false);
	}

	public float EndingSequence()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(EndGameSequence()));
	}

	private IEnumerator<float> EndGameSequence()
	{
		_JetAmbience.Play();
		SetupPlayers();
		_audioSource.PlayOneShot(_beltWarning);
		if (PlayerUtilities.GetServerCharacterGender() == EGameGenderMode.Male)
		{
			_audioSource.PlayOneShot(_wakeAudio);
		}
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer.Id];
			((Component)allPlayer.PlayerCamera).transform.forward = seatingPositionData.SittingCameraTarget.forward;
			((Component)allPlayer.PlayerCamera.MouseLook).transform.forward = seatingPositionData.SittingCameraTarget.forward;
			Vector3 forward = allPlayer.transform.forward;
			forward.Normalize();
			allPlayer.PlayerCamera.MouseLook.ClampViewToPos(forward);
			allPlayer.PlayerCamera.MouseLook.ClampIntro = true;
			((Behaviour)allPlayer.PlayerCamera.MouseLook).enabled = allPlayer.IsOwner;
			allPlayer.PlayerCamera.CameraMovement.SetCameraTarget(seatingPositionData.SittingCameraTarget);
			++allPlayer.Movement.IsBusy;
		}
		_audioSource.PlayOneShot(_crashAudio);
		yield return Timing.WaitForSeconds(0.5f);
		Object.Instantiate<GameObject>(_explosion, _explosionPos.position, Quaternion.identity).transform.parent = _sceneContainer.transform;
		CameraSpeedEffect(-25f, 0.2f);
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			allPlayer2.PlayerCamera.CameraMovement.ShakeCamera(0.2f, 1f);
		}
		yield return Timing.WaitForSeconds(0.2f);
		CameraSpeedEffect(25f, 0.35f);
		_rubberDuckEasterEgg.SetActive(true);
		ChangeGravity(new Vector3(-3f, 3f, 10f));
		foreach (IPlayer allPlayer3 in PlayerRegistry.AllPlayers)
		{
			_seatingPositionsData[allPlayer3.Id].SittingCameraTarget.localEulerAngles = new Vector3(-12f, 0f, 5f);
			allPlayer3.PlayerCamera.CameraMovement.ShakeCamera(19f, 0.07f);
		}
		_toggleLightingSequence = Timing.RunCoroutine(ToggleLighting());
		yield return Timing.WaitForSeconds(9f);
		CameraTiltEffect(45f, 9f);
		ChangeGravity(new Vector3(-9f, 9f, 1f));
		yield return Timing.WaitForSeconds(6f);
		AudioManager.GetAudioPlayer().Play2D(_endGameCrashAudio, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(4f);
		foreach (IPlayer allPlayer4 in PlayerRegistry.AllPlayers)
		{
			allPlayer4.Statistics.ApplyDamage(0f);
			allPlayer4.PlayerCamera.CameraMovement.ShakeCamera(1f, 0.15f);
		}
		ChangeGravity(new Vector3(3f, 0f, -12f));
		yield return Timing.WaitForSeconds(0.5f);
		float timer = 0f;
		while (timer < 0.5f)
		{
			foreach (IPlayer allPlayer5 in PlayerRegistry.AllPlayers)
			{
				SeatingPositionData seatingPositionData2 = _seatingPositionsData[allPlayer5.Id];
				seatingPositionData2.SittingCameraTarget.position = Vector3.Lerp(seatingPositionData2.SittingCameraTarget.position, seatingPositionData2.PlayerControllerPosition.position + seatingPositionData2.PlayerControllerPosition.forward, Time.deltaTime * 6f);
			}
			timer += Time.deltaTime;
			yield return 0f;
		}
		Timing.KillCoroutines(_toggleLightingSequence);
		ResetGravity();
		yield return 0f;
	}

	private IEnumerator<float> WakeUp()
	{
		_JetAmbience.Play();
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer.Id];
			((Component)allPlayer.PlayerCamera).transform.forward = seatingPositionData.SittingCameraTarget.forward;
			((Component)allPlayer.PlayerCamera.MouseLook).transform.forward = seatingPositionData.SittingCameraTarget.forward;
			Vector3 forward = allPlayer.transform.forward;
			forward.Normalize();
			allPlayer.PlayerCamera.MouseLook.ClampViewToPos(forward);
			allPlayer.PlayerCamera.MouseLook.ClampIntro = true;
			((Behaviour)allPlayer.PlayerCamera.MouseLook).enabled = false;
		}
		yield return Timing.WaitForSeconds(2f);
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			allPlayer2.PlayerCamera.CameraMovement.ShakeCamera(0.5f, 0.05f);
		}
		if (PlayerUtilities.GetServerCharacterGender() == EGameGenderMode.Male)
		{
			_audioSource.PlayOneShot(_wakeAudio);
		}
		yield return Timing.WaitForSeconds(2f);
		_audioSource.PlayOneShot(_beltWarning);
		_canSkipIntro = true;
		yield return Timing.WaitForSeconds(0.5f);
		foreach (IPlayer allPlayer3 in PlayerRegistry.AllPlayers)
		{
			((Behaviour)allPlayer3.PlayerCamera.MouseLook).enabled = allPlayer3.IsOwner;
		}
		_backLight.intensity = 0f;
		_backLights.SetActive(true);
		while (_backLight.intensity < 1f)
		{
			Light backLight = _backLight;
			backLight.intensity += Time.deltaTime * 0.1f;
		}
		yield return Timing.WaitForSeconds(1f);
		_loosingAltitudeSequence = Timing.RunCoroutine(LoosingAltitude());
	}

	private IEnumerator<float> LoosingAltitude()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer.Id];
			((Component)allPlayer.PlayerCamera).transform.position = seatingPositionData.PlayerControllerPosition.position;
			allPlayer.PlayerCamera.CameraMovement.SetCameraTarget(seatingPositionData.SittingCameraTarget);
			++allPlayer.Movement.IsBusy;
		}
		yield return Timing.WaitForSeconds(8f);
		_randomTurbulence.Play();
		yield return Timing.WaitForSeconds(1f);
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			allPlayer2.PlayerCamera.CameraMovement.ShakeCamera(2.4f, 0.1f);
		}
		yield return Timing.WaitForSeconds(2f);
		_audioSource.PlayOneShot(_crashAudio);
		yield return Timing.WaitForSeconds(0.5f);
		Object.Instantiate<GameObject>(_explosion, _explosionPos.position, Quaternion.identity).transform.parent = _sceneContainer.transform;
		CameraSpeedEffect(-25f, 0.2f);
		foreach (IPlayer allPlayer3 in PlayerRegistry.AllPlayers)
		{
			allPlayer3.PlayerCamera.CameraMovement.ShakeCamera(0.2f, 1f);
		}
		yield return Timing.WaitForSeconds(0.2f);
		CameraSpeedEffect(25f, 0.35f);
		_jetDamaged.SetActive(true);
		_jetComplete.SetActive(false);
		ChangeGravity(new Vector3(-3f, 3f, 10f));
		foreach (IPlayer allPlayer4 in PlayerRegistry.AllPlayers)
		{
			_seatingPositionsData[allPlayer4.Id].SittingCameraTarget.localEulerAngles = new Vector3(-12f, 0f, 5f);
			allPlayer4.PlayerCamera.CameraMovement.ShakeCamera(19f, 0.07f);
		}
		_toggleLightingSequence = Timing.RunCoroutine(ToggleLighting());
		yield return Timing.WaitForSeconds(9f);
		CameraTiltEffect(45f, 9f);
		ChangeGravity(new Vector3(-9f, 9f, 1f));
		yield return Timing.WaitForSeconds(10f);
		_crashImpactWaterSequence = Timing.RunCoroutine(CrashImpactWater());
	}

	private IEnumerator<float> CrashImpactWater()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.Statistics.ApplyDamage(0f);
			allPlayer.PlayerCamera.CameraMovement.ShakeCamera(1f, 0.15f);
		}
		ChangeGravity(new Vector3(3f, 0f, -12f));
		yield return Timing.WaitForSeconds(0.5f);
		_canPause = false;
		Singleton<Fader>.Instance.FadeOut(0.3f);
		float timer = 0f;
		while (timer < 0.5f)
		{
			foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
			{
				SeatingPositionData seatingPositionData = _seatingPositionsData[allPlayer2.Id];
				seatingPositionData.SittingCameraTarget.position = Vector3.Lerp(seatingPositionData.SittingCameraTarget.position, seatingPositionData.PlayerControllerPosition.position + seatingPositionData.PlayerControllerPosition.forward, Time.deltaTime * 6f);
			}
			timer += Time.deltaTime;
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(3f);
		_landedInWaterSequence = Timing.RunCoroutine(LandedInWater());
	}

	private IEnumerator<float> LandedInWater()
	{
		Timing.KillCoroutines(_toggleLightingSequence);
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			((Component)allPlayer.transform).gameObject.SetActive(true);
			allPlayer.transform.position = _playerCrashPosition.position;
			((Behaviour)allPlayer.PlayerCamera.MouseLook).enabled = false;
			allPlayer.PlayerCamera.CameraMovement.ResetCameraTarget();
			allPlayer.transform.localScale = Vector3.one;
		}
		ResetGravity();
		_jetNonCrashed.SetActive(false);
		_jetCrashed.SetActive(true);
		yield return Singleton<Fader>.Instance.FadeIn_Async(2f);
		_canPause = true;
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			allPlayer2.Movement.IsBusy = default;
			((Behaviour)allPlayer2.PlayerCamera.MouseLook).enabled = allPlayer2.IsOwner;
			allPlayer2.PlayerCamera.MouseLook.ClampIntro = false;
			allPlayer2.PlayerCamera.CameraMovement.ShakeCamera(1f, 0.05f);
		}
		_landingCrashExplosionsSequence = Timing.RunCoroutine(LandedCrashExplosions());
		Singleton<PrivateJetTutorial>.Instance.FindLifeRaft = true;
		float random = Random.Range(0f, 2.1474836E+09f);
		bool playerNearRaft = false;
		while (!playerNearRaft)
		{
			playerNearRaft = Beam.Utilities.ExtensionMethods.Any_NonAlloc(PlayerRegistry.AllPlayers, (IPlayer p) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				return Vector3.Distance(p.transform.position, new Vector3(-6.7f, 0f, 1.78f)) < 3f;
			});
			foreach (Transform underwaterProp in _underwaterProps)
			{
				underwaterProp.Rotate(new Vector3(16f * Time.deltaTime, 8f * Time.deltaTime, 0f), (Space)1);
			}
			foreach (IPlayer allPlayer3 in PlayerRegistry.AllPlayers)
			{
				if (allPlayer3.Movement.InWater)
				{
					((Behaviour)((Component)_jetEngine).GetComponent<AudioLowPassFilter>()).enabled = !allPlayer3.Movement.isFloating;
				}
			}
			float num = Mathf.PerlinNoise(random, Time.time * 2f);
			float num2 = Mathf.Lerp(0f, 1f, num);
			_crashedMonitor.SetActive(num2 > 0.5f);
			yield return 0f;
		}
		_endSceneSequence = Timing.RunCoroutine(EndScene());
	}

	private IEnumerator<float> LandedCrashExplosions()
	{
		while (true)
		{
			yield return Timing.WaitForSeconds((float)Random.Range(5, 15));
			Object.Instantiate<GameObject>(explosion, explosionPositions[Random.Range(0, explosionPositions.Length)].transform.position, Quaternion.identity).transform.parent = _sceneContainer.transform;
			foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
			{
				allPlayer.PlayerCamera.CameraMovement.ShakeCamera(1.5f, 0.015f);
			}
		}
	}

	private IEnumerator<float> EndScene()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			++allPlayer.Movement.IsBusy;
			((Behaviour)allPlayer.PlayerCamera.MouseLook).enabled = false;
		}
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			allPlayer2.PlayerCamera.CameraMovement.ShakeCamera(1f, 0.5f);
		}
		CameraSpeedEffect(25f, 0.2f);
		Object.Instantiate<GameObject>(explosion, _finalExplosionPos.position, Quaternion.identity).transform.parent = _sceneContainer.transform;
		_jetEngine.Stop();
		yield return Timing.WaitForSeconds(0.3f);
		_canPause = false;
		foreach (IPlayer allPlayer3 in PlayerRegistry.AllPlayers)
		{
			allPlayer3.Statistics.ApplyDamage(0f);
		}
		Singleton<Fader>.Instance.FadeOut(0.25f);
		while (AudioListener.volume > 0f)
		{
			AudioListener.volume -= 0.25f * Time.deltaTime;
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(1f);
		PlayerPrefs.SetInt("Completed_Tutorial", 1);
		LoadNewGame();
	}

	private void CameraSpeedEffect(float fov, float duration)
	{
		LeanTween.value(((Component)this).gameObject, (Action<float>)UpdateCameraSpeedEffect, Camera.main.fieldOfView, Camera.main.fieldOfView + fov, duration);
	}

	private void UpdateCameraSpeedEffect(float fov)
	{
		Camera.main.fieldOfView = fov;
	}

	private void CameraTiltEffect(float maxTilt, float duration)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		float z = _seatingPositionsData[0].SittingCameraTarget.localEulerAngles.z;
		LeanTween.value(((Component)this).gameObject, (Action<float>)UpdateCameraTiltEffect, z, maxTilt, duration);
	}

	private void UpdateCameraTiltEffect(float maxTilt)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			_seatingPositionsData[allPlayer.Id].SittingCameraTarget.localEulerAngles = new Vector3(-12f, 0f, maxTilt);
		}
	}

	private IEnumerator<float> ToggleLighting()
	{
		float random = Random.Range(0f, 2.1474836E+09f);
		while (true)
		{
			float num = Mathf.PerlinNoise(random, Time.time * 3f);
			float num2 = Mathf.Lerp(0f, 1f, num);
			_backLights.SetActive(num2 > 0.5f);
			_monitor.SetActive(num2 > 0.5f);
			yield return 0f;
		}
	}

	private void ChangeGravity(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Physics.gravity = value;
	}

	private void ResetGravity()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ChangeGravity(new Vector3(0f, -9.81f, 0f));
	}

	private void LoadNewGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerMng.SendGameStateChangeGlobalEvent(Game.State, (GameState)2);
	}

	private void Listener_GameStateChangeGlobalEvent(GameStateChangeGlobalEvent evnt)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (evnt.FromState == 1 && evnt.ToState == 2)
		{
			MultiplayerMng.Log($"Listener_GameStateChangeGlobalEvent From {(object)(GameState)evnt.FromState} To {(object)(GameState)evnt.ToState}");
			_loadNewGame = Timing.RunCoroutine(LoadNewGame_Sequence());
		}
	}

	private void MainMenuPresenter_QuittingToMenu()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)this))
		{
			Timing.KillCoroutines(_loadNewGame);
			Timing.KillCoroutines(_wakeUpSequence);
			Timing.KillCoroutines(_loosingAltitudeSequence);
			Timing.KillCoroutines(_toggleLightingSequence);
			Timing.KillCoroutines(_crashImpactWaterSequence);
			Timing.KillCoroutines(_landedInWaterSequence);
			Timing.KillCoroutines(_landingCrashExplosionsSequence);
			Timing.KillCoroutines(_endSceneSequence);
			ResetGravity();
			QualitySettings.pixelLightCount = _originalPixelLights;
			BeamTeam.Destroy<GameObject>(_sceneContainer);
		}
	}

	private IEnumerator<float> LoadNewGame_Sequence()
	{
		Timing.KillCoroutines(_wakeUpSequence);
		Timing.KillCoroutines(_loosingAltitudeSequence);
		Timing.KillCoroutines(_toggleLightingSequence);
		Timing.KillCoroutines(_crashImpactWaterSequence);
		Timing.KillCoroutines(_landedInWaterSequence);
		Timing.KillCoroutines(_landingCrashExplosionsSequence);
		Timing.KillCoroutines(_endSceneSequence);
		PlayerUtilities.SetPlayerBundlesActive(active: false);
		_sceneContainer.AddComponent<DestroyPlayersOnDestroy>();
		BeamTeam.Destroy<GameObject>(_sceneContainer);
		yield return BeamTiming.WaitForPlayersToUnspawn();
		ResetGravity();
		QualitySettings.pixelLightCount = _originalPixelLights;
		BeamTeam.Destroy<GameObject>(_sceneContainer);
		Game.State = (GameState)2;
		Singleton<Fader>.Instance.FadeOut(0f);
		Object.FindObjectOfType<MainMenuPresenter>()?.LoadLevel(3, (LoadSceneMode)1);
	}
}
