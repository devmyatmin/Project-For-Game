using System;
using System.Collections.Generic;
using Beam.Events;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Ceto;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Intro;

public class AircraftVFX : ReplicableMonoBehaviour, ISaveable
{
	public class ReplicateEnterCockpit : MultiplayerMessage
	{
		[Replicate]
		public AircraftVFX aircraftVFX;

		public override void OnPeer()
		{
			aircraftVFX.EnterCockpit();
		}
	}

	private MiniGuid _referenceId;

	private IPlayer _player1;

	private IPlayer _player2;

	private IPlayerSpeech _player1Speech;

	[SerializeField]
	private string _gyroPartID = "aa5bb8d8-fd1b-4902-83a9-94b375d0d0dc";

	[SerializeField]
	private Transform[] _gyroRewardPiecePositions;

	[SerializeField]
	private GameObject _aircraftStation;

	[SerializeField]
	private GameObject _cockpitInteraction;

	private bool _tightCameraClamp;

	[Header("SOUND FX")]
	[SerializeField]
	private AudioClip _enterHangarClip;

	[SerializeField]
	private AudioClip _enterCockpitClip;

	[SerializeField]
	private AudioClip _cockpitCheckClip;

	[SerializeField]
	private AudioClip _cockpitStartEngineClip;

	[SerializeField]
	private AudioClip _cockpitCloseClip;

	[SerializeField]
	private AudioClip _cockpitRailClip;

	[SerializeField]
	private AudioClip _cockpitThrottleClip;

	[SerializeField]
	private AudioClip _cockpitLaunchClip;

	[SerializeField]
	private AudioClip _cockpitCrashClip;

	[SerializeField]
	private AudioClip _objectiveCompletedClip;

	[SerializeField]
	private AudioClip _objectivesCompletedClip;

	[Header("SOUND ENGINE")]
	[SerializeField]
	private AudioSource _idleEngineAudio;

	[SerializeField]
	private AudioLowPassFilter _idleEngineAudioFilter;

	[SerializeField]
	private AudioSource _throttledEngineAudio;

	[Header("ANIMATION")]
	[SerializeField]
	private Transform _enterHangarPosition;

	[SerializeField]
	private Transform _endHangarPosition;

	[SerializeField]
	private Transform _cockpitOperatingPosition;

	[SerializeField]
	private Transform _backseatOperatingPosition;

	[SerializeField]
	private Animator _aircraftAnimator;

	[SerializeField]
	private Animation _aircraftTakeOffAnimation;

	[SerializeField]
	private Animation _crashAnimation;

	[SerializeField]
	private Transform _crashAircraftTrans;

	[SerializeField]
	private Transform _crashPrivateJetTrans;

	[Header("VISUALS")]
	[SerializeField]
	private Transform _aircraft;

	[SerializeField]
	private Transform _aircraftRail;

	[SerializeField]
	private Transform _rail;

	[SerializeField]
	private Transform _engingeHatch;

	[SerializeField]
	private Transform _fuelHatch1;

	[SerializeField]
	private Transform _fuelHatch2;

	[SerializeField]
	private GameObject _trolley;

	[SerializeField]
	private GameObject _enginePart;

	[SerializeField]
	private GameObject _propellerPart;

	[SerializeField]
	private Transform _propeller;

	[SerializeField]
	private GameObject _cockpitLights;

	[SerializeField]
	private GameObject _crashImpact;

	[SerializeField]
	private Light _ambientLight;

	private AircraftManager _aircraftManager;

	private CoroutineHandle _enterHangarSequence;

	private CoroutineHandle _enterCockpitSequence;

	private CoroutineHandle _completedObjectiveSequence;

	private CoroutineHandle _completedObjectivesSequence;

	private CoroutineHandle _interactionSequence;

	private CoroutineHandle _flyingSequence;

	private CoroutineHandle _crashSequence;

	private CoroutineHandle _rotatePropeller;

	private CoroutineHandle _clampCamera;

	private bool _enteredHangar;

	private bool _completed;

	[Header("EVENTS")]
	public UnityEvent ObjectivesComplete;

	[SerializeField]
	private PrivateJet_Assetloader _introAssetLoader;

	private float _curPropellerSpeed;

	private float _maxPropellerSpeed;

	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault())
			{
				_referenceId = MiniGuid.INTERACTIVE_AIRCRAFT_VFX;
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_player1 = (MPExtensions.IsMultiplayer(Game.Mode) ? PlayerRegistry.ServerPeer.Player : PlayerRegistry.AllPlayers[0]);
		if (MPExtensions.GetPlayersCount(Game.Mode) > 1 && PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers[1] != null)
		{
			_player2 = PlayerRegistry.AllPlayers[1];
		}
		EventManager.AddListener<PlayerDisconnectedEvent>(RemotePlayerDisconnected_Event);
		AudioTools.FixMixerGroups(_idleEngineAudio);
		AudioTools.FixMixerGroups(_throttledEngineAudio);
		IPlayerSpeech playerSpeech = _player1.PlayerSpeech;
		_player1Speech = playerSpeech ?? NullObject<NullPlayerSpeech>.Instance;
		((Component)_crashAircraftTrans).gameObject.SetActive(false);
		((Component)_crashPrivateJetTrans).gameObject.SetActive(false);
		((Component)_crashAnimation).gameObject.SetActive(false);
		_cockpitInteraction.gameObject.SetActive(false);
		_cockpitLights.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_enterHangarSequence);
		Timing.KillCoroutines(_enterCockpitSequence);
		Timing.KillCoroutines(_completedObjectiveSequence);
		Timing.KillCoroutines(_completedObjectivesSequence);
		Timing.KillCoroutines(_interactionSequence);
		Timing.KillCoroutines(_flyingSequence);
		Timing.KillCoroutines(_crashSequence);
		Timing.KillCoroutines(_rotatePropeller);
		Timing.KillCoroutines(_clampCamera);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(RemotePlayerDisconnected_Event);
	}

	public void RemotePlayerDisconnected_Event(PlayerDisconnectedEvent e)
	{
		if (e.PlayerId == 1)
		{
			_player2 = null;
		}
	}

	public void Initialize(AircraftManager aircraftManager)
	{
		_aircraftManager = aircraftManager;
	}

	private void PlayAircraftAnimation(string animationName)
	{
		_aircraftAnimator.SetTrigger(animationName);
	}

	private void PlayCharacterAnimation(IPlayer player, string animationName)
	{
		player.Character.PlayEventAnimation(animationName);
	}

	private void SetCharacterPosition(IPlayer player, Transform trans)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((Component)player.Movement).transform.position = trans.position;
		((Component)player.Movement).transform.rotation = trans.rotation;
		player.Character.SetPosition(trans.position);
		player.Character.SetRotation(trans.rotation);
	}

	private void SetPlayer2()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_player2 != null)
		{
			FreezePlayer(_player2);
			SetCharacterPosition(_player2, _backseatOperatingPosition);
			((Component)_player2.Character).transform.parent = _backseatOperatingPosition;
			((Component)_player2.Character).transform.localPosition = Vector3.zero;
			_player2.Character.PlayingEventAnimation = true;
			PlayCharacterAnimation(_player2, "EndGame_Cockpit_Idle");
		}
	}

	private void FreezePlayer(IPlayer player)
	{
		if (player.IsOwner)
		{
			++player.Movement.IsBusy;
			player.Movement.IsInCutscene = true;
			((Collider)player.Movement.CharacterController).enabled = false;
		}
	}

	private void ResetCharacter(IPlayer player)
	{
		if (player.IsOwner)
		{
			--player.Movement.IsBusy;
			player.Movement.IsInCutscene = false;
			((Collider)player.Movement.CharacterController).enabled = true;
		}
	}

	private void CameraShakeEffect(float magnitude, float duration)
	{
		_player1.PlayerCamera.CameraMovement.ShakeCamera(duration, magnitude);
		if (_player2 != null)
		{
			_player2.PlayerCamera.CameraMovement.ShakeCamera(duration, magnitude);
		}
	}

	private void CameraSpeedEffect(float fov, float duration)
	{
		LeanTween.value(((Component)this).gameObject, (Action<float>)UpdateCameraSpeedEffect, _player1.PlayerCamera.Camera.fieldOfView, fov, duration);
	}

	private void UpdateCameraSpeedEffect(float fov)
	{
		_player1.PlayerCamera.Camera.fieldOfView = fov;
		if (_player2 != null)
		{
			_player2.PlayerCamera.Camera.fieldOfView = fov;
		}
	}

	private IPlayer GetClosestPlayer(Vector3 position)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		IPlayer result = PlayerRegistry.AllPlayers[0];
		if (MPExtensions.GetPlayersCount(Game.Mode) == 1)
		{
			return result;
		}
		float num = float.PositiveInfinity;
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			Vector3 val = position - player.transform.position;
			float magnitude = val.magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				result = player;
			}
		}
		return result;
	}

	public void EnterHangar()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (_enteredHangar)
		{
			PlayTrack(TrackType.Ambience_Carrier);
			return;
		}
		_enteredHangar = true;
		_enterHangarSequence = Timing.RunCoroutine(EnterHangar_Sequence());
	}

	private IEnumerator<float> EnterHangar_Sequence()
	{
		SpawnGyroRewardPieces();
		IPlayer closestPlayer = GetClosestPlayer(_enterHangarPosition.position);
		closestPlayer.Character.PlayingEventAnimation = true;
		SetCharacterPosition(closestPlayer, _enterHangarPosition);
		PlayCharacterAnimation(closestPlayer, "EndGame_EnterHangar");
		FreezePlayer(closestPlayer);
		closestPlayer.PlayerCamera.CameraMovement.SetRotation(closestPlayer.Character.CharacterFirstPerson.CameraRigPosition);
		closestPlayer.PlayerCamera.CameraMovement.SetCameraTarget(closestPlayer.Character.CharacterFirstPerson.CameraRigPosition);
		((Behaviour)closestPlayer.PlayerCamera.MouseLook).enabled = false;
		AudioManager.GetAudioPlayer().Play2D(_enterHangarClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(8f);
		closestPlayer.PlayerCamera.CameraMovement.ResetCameraTarget();
		closestPlayer.PlayerCamera.CameraMovement.SetPosition(closestPlayer.Character.CharacterFirstPerson.CameraRigPosition);
		((Behaviour)closestPlayer.PlayerCamera.MouseLook).enabled = closestPlayer.IsOwner;
		SetCharacterPosition(closestPlayer, _endHangarPosition);
		closestPlayer.Character.PlayingEventAnimation = false;
		ResetCharacter(closestPlayer);
		PlayTrack(TrackType.Ambience_Carrier);
		Timing.KillCoroutines(_enterHangarSequence);
	}

	private void SpawnGyroRewardPieces()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			uint num = Prefabs.ConvertLegacyPrefabId(_gyroPartID);
			if (num == 0)
			{
				Debug.LogError((object)"AircraftVFX :: SpawnGyroRewardPieces : Missing Gyrocopter Reward Piece!");
			}
			for (int i = 0; i < 3; i++)
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(num, _gyroRewardPiecePositions[i].position, _gyroRewardPiecePositions[i].rotation, MiniGuid.New());
			}
		}
	}

	public void EndGameObjectives_ObjectivesCompleted(AircraftObjectivesCompletedArgs args)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_completedObjectivesSequence = Timing.RunCoroutine(ObjectivesCompleted_Sequence(args));
		_completed = true;
	}

	private IEnumerator<float> ObjectivesCompleted_Sequence(AircraftObjectivesCompletedArgs args)
	{
		ActivateParts();
		CloseFuelHatches();
		if (args.IsFirstTime)
		{
			yield return Timing.WaitForSeconds(0.5f);
			AudioManager.GetAudioPlayer().Play2D(_objectivesCompletedClip, AudioMixerChannels.Music);
			yield return Timing.WaitForSeconds(_objectivesCompletedClip.length * 0.7f);
			if (_player1Speech != null)
			{
				_player1Speech.Play("STATUS_END_OBJECTIVES_COMPLETED", _player1.Gender, underwater: false);
			}
			yield return Timing.WaitForSeconds(4f);
		}
		_aircraftStation.SetActive(true);
		Timing.KillCoroutines(_completedObjectivesSequence);
		UnityEvent objectivesComplete = ObjectivesComplete;
		if (objectivesComplete != null)
		{
			objectivesComplete.Invoke();
		}
	}

	public void EndGameObjectives_ObjectiveCompleted()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!_completedObjectiveSequence.IsRunning)
		{
			_completedObjectiveSequence = Timing.RunCoroutine(ObjectiveCompleted_Sequence());
		}
	}

	private IEnumerator<float> ObjectiveCompleted_Sequence()
	{
		yield return Timing.WaitForSeconds(0.5f);
		AudioManager.GetAudioPlayer().Play2D(_objectiveCompletedClip, AudioMixerChannels.FX);
	}

	private void EnterCockpit()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		}
		if (_player1.IsOwner)
		{
			_player1.PlayerCamera.SetCameraMode(CameraViewMode.FIRST_PERSON);
		}
		if (_player2 != null && _player2.IsOwner)
		{
			_player2.PlayerCamera.SetCameraMode(CameraViewMode.FIRST_PERSON);
		}
		_enterCockpitSequence = Timing.RunCoroutine(EnterCockpit_Sequence());
	}

	public void ReplicatedEnterCockpit()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateEnterCockpit replicateEnterCockpit = new ReplicateEnterCockpit();
			replicateEnterCockpit.aircraftVFX = this;
			replicateEnterCockpit.Post();
		}
		EnterCockpit();
	}

	private IEnumerator<float> EnterCockpit_Sequence()
	{
		EventManager.RaiseEvent(new EndGameSequenceEvent());
		EventManager.RaiseEvent(new FreezeNotificationsEvent(freeze: true));
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsOwner)
			{
				RewiredUtilities.SetAndCacheMapsEnabled(allPlayer.Input, state: false, 0);
				RewiredUtilities.SetAndCacheMapsEnabled(allPlayer.Input, state: true, 10);
			}
			allPlayer.Character.HideCurrentItem();
			FreezePlayer(allPlayer);
			allPlayer.PlayerCamera.CameraMovement.SetRotation(allPlayer.Character.CharacterFirstPerson.CameraRigPosition);
			allPlayer.PlayerCamera.CameraMovement.SetCameraTarget(allPlayer.Character.CharacterFirstPerson.CameraRigPosition);
			((Behaviour)allPlayer.PlayerCamera.MouseLook).enabled = false;
		}
		PlayTrack(TrackType.Ambience_Cockpit);
		_aircraftStation.SetActive(false);
		float num = Mathf.Abs(Singleton<GameTime>.Instance.MilitaryTime - 16f);
		if (Singleton<GameTime>.Instance.MilitaryTime > 16f)
		{
			num = 24f - num;
		}
		float num2 = num * 60f * 60f;
		float rate = num2 / 3f / (float)GameTime.TIME_SCALE;
		while (Singleton<GameTime>.Instance.MilitaryTime > 16f)
		{
			Singleton<GameTime>.Instance.Tick += Time.deltaTime * rate;
			yield return 0f;
		}
		while (Singleton<GameTime>.Instance.MilitaryTime < 16f)
		{
			Singleton<GameTime>.Instance.Tick += Time.deltaTime * rate;
			yield return 0f;
		}
		while (_ambientLight.intensity > 0f)
		{
			Light ambientLight = _ambientLight;
			ambientLight.intensity -= Time.deltaTime;
			if (_ambientLight.intensity <= 0f)
			{
				((Behaviour)_ambientLight).enabled = false;
			}
			yield return 0f;
		}
		Singleton<GameTime>.Instance.MilitaryTime_NoEvents = 16f;
		Singleton<GameTime>.Instance.Paused = true;
		yield return Timing.WaitForSeconds(0.5f);
		_player1.PlayerCamera.CameraMovement.ResetCameraTarget();
		((Behaviour)_player1.PlayerCamera.MouseLook).enabled = _player1.IsOwner;
		if (_player2 != null)
		{
			_player2.PlayerCamera.CameraMovement.ResetCameraTarget();
			((Behaviour)_player2.PlayerCamera.MouseLook).enabled = _player2.IsOwner;
		}
		_tightCameraClamp = true;
		ClampCameras();
		_player1.Character.PlayingEventAnimation = true;
		SetCharacterPosition(_player1, _cockpitOperatingPosition);
		PlayCharacterAnimation(_player1, "EndGame_EnterCockpit");
		PlayAircraftAnimation("Cockpit_Enter");
		if (_player2 != null)
		{
			_player2.Character.PlayingEventAnimation = true;
			SetCharacterPosition(_player2, _backseatOperatingPosition);
			PlayCharacterAnimation(_player2, "EndGame_EnterCockpit_2");
		}
		AudioManager.GetAudioPlayer().Play2D(_enterCockpitClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(1f);
		_aircraftManager.Objectives.CompleteFinalObjective();
		yield return Timing.WaitForSeconds(8f);
		SetCharacterPosition(_player1, _cockpitOperatingPosition);
		((Component)_player1.Character).transform.parent = _cockpitOperatingPosition;
		((Component)_player1.Character).transform.localPosition = Vector3.zero;
		if (_player2 != null)
		{
			SetCharacterPosition(_player2, _backseatOperatingPosition);
			((Component)_player2.Character).transform.parent = _backseatOperatingPosition;
			((Component)_player2.Character).transform.localPosition = Vector3.zero;
		}
		yield return Timing.WaitForSeconds(1f);
		_tightCameraClamp = false;
		_player1.Statistics.Invincible = true;
		Sunburn playerEffect = _player1.Statistics.GetPlayerEffect<Sunburn>();
		if ((Object)(object)playerEffect != (Object)null)
		{
			playerEffect.ResetExposure();
		}
		if (_player2 != null)
		{
			_player2.Statistics.Invincible = true;
			Sunburn playerEffect2 = _player2.Statistics.GetPlayerEffect<Sunburn>();
			if ((Object)(object)playerEffect2 != (Object)null)
			{
				playerEffect2.ResetExposure();
			}
		}
		InteractionCheck(InteractionSequence.CheckSystems);
	}

	public void InteractionCheck(InteractionSequence sequence)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_interactionSequence);
		switch (sequence)
		{
		case InteractionSequence.CheckSystems:
			_interactionSequence = Timing.RunCoroutine(InteractionCheck_Sequence());
			break;
		case InteractionSequence.StartEngine:
			_interactionSequence = Timing.RunCoroutine(InteractionStartEngine_Sequence());
			break;
		case InteractionSequence.CloseCockpit:
			_interactionSequence = Timing.RunCoroutine(InteractionCloseCockpit_Sequence());
			break;
		case InteractionSequence.EngageRail:
			_interactionSequence = Timing.RunCoroutine(InteractionRail_Sequence());
			break;
		case InteractionSequence.EngageEngine:
			_interactionSequence = Timing.RunCoroutine(InteractionEngageEngine_Sequence());
			break;
		case InteractionSequence.TakeOff:
			_interactionSequence = Timing.RunCoroutine(InteractionLaunch_Sequence());
			break;
		}
	}

	private IEnumerator<float> InteractionCheck_Sequence()
	{
		PlayCharacterAnimation(_player1, "Cockpit_Check");
		PlayAircraftAnimation("Cockpit_Check");
		yield return Timing.WaitForSeconds(0.8f);
		AudioManager.GetAudioPlayer().Play2D(_cockpitCheckClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(10f);
		_cockpitInteraction.SetActive(true);
	}

	private IEnumerator<float> InteractionStartEngine_Sequence()
	{
		PlayCharacterAnimation(_player1, "Cockpit_StartEngine");
		PlayAircraftAnimation("Cockpit_StartEngine");
		yield return Timing.WaitForSeconds(1f);
		AudioManager.GetAudioPlayer().Play2D(_cockpitStartEngineClip, AudioMixerChannels.FX);
		_cockpitLights.SetActive(true);
		yield return Timing.WaitForSeconds(3.2f);
		_maxPropellerSpeed = 0.3f;
		RotatePropellers();
		CameraShakeEffect(0.001f, -1f);
		yield return Timing.WaitForSeconds(11.1f);
		_idleEngineAudio.Play();
		_cockpitInteraction.SetActive(true);
	}

	private IEnumerator<float> InteractionCloseCockpit_Sequence()
	{
		PlayCharacterAnimation(_player1, "Cockpit_Close");
		PlayAircraftAnimation("Cockpit_Close");
		AudioManager.GetAudioPlayer().Play2D(_cockpitCloseClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(3.5f);
		((Behaviour)_idleEngineAudioFilter).enabled = true;
		yield return Timing.WaitForSeconds(6.1f);
		CloseEngineHatch();
		yield return Timing.WaitForSeconds(2.5f);
		_cockpitInteraction.SetActive(true);
	}

	private IEnumerator<float> InteractionRail_Sequence()
	{
		PlayCharacterAnimation(_player1, "Cockpit_Rail");
		yield return Timing.WaitForSeconds(0.5f);
		PlayAircraftAnimation("Cockpit_Rail");
		AudioManager.GetAudioPlayer().Play2D(_cockpitRailClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(4.5f);
		Quaternion startingRotation = _aircraftRail.localRotation;
		Quaternion endRotation = Quaternion.identity;
		float timer = 0f;
		while (timer < 9f)
		{
			float num = timer / 9f;
			_aircraftRail.localRotation = Quaternion.Lerp(startingRotation, endRotation, num);
			timer += Time.deltaTime;
			yield return 0f;
		}
		_aircraftRail.localRotation = endRotation;
		yield return Timing.WaitForSeconds(1f);
		_cockpitInteraction.SetActive(true);
	}

	private IEnumerator<float> InteractionEngageEngine_Sequence()
	{
		PlayCharacterAnimation(_player1, "Cockpit_EngageEngine");
		PlayAircraftAnimation("Cockpit_Throttle");
		RaiseRail();
		yield return Timing.WaitForSeconds(0.5f);
		yield return Timing.WaitForSeconds(3f);
		CameraShakeEffect(0.005f, 60f);
		_maxPropellerSpeed = 1f;
		yield return Timing.WaitForSeconds(2f);
		_idleEngineAudio.Stop();
		AudioManager.GetAudioPlayer().Play2D(_cockpitThrottleClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(2f);
		_throttledEngineAudio.Play();
		Quaternion startingRotation = _aircraft.localRotation;
		Quaternion endRotation = Quaternion.identity;
		float timer = 0f;
		while (timer < 3f)
		{
			float num = timer / 3f;
			_aircraft.localRotation = Quaternion.Lerp(startingRotation, endRotation, num);
			timer += Time.deltaTime;
			yield return 0f;
		}
		_aircraft.localRotation = endRotation;
		_cockpitInteraction.SetActive(true);
	}

	private IEnumerator<float> InteractionLaunch_Sequence()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsOwner)
			{
				RewiredUtilities.DisableMaps(allPlayer.Input);
				RewiredUtilities.SetAndCacheMapsEnabled(allPlayer.Input, state: true, 9);
			}
		}
		PlayCharacterAnimation(_player1, "Cockpit_Launch");
		PlayAircraftAnimation("Cockpit_Launch");
		PlayTrackInstantly(TrackType.Cockpit_TakeOff);
		AudioManager.GetAudioPlayer().Play2D(_cockpitLaunchClip, AudioMixerChannels.FX);
		yield return Timing.WaitForSeconds(1.5f);
		((Component)_aircraft).transform.parent = ((Component)_aircraftTakeOffAnimation).transform;
		float num = 1f;
		float animationTime = _aircraftTakeOffAnimation["Aircraft_TakeOff"].length * (1f / num);
		_aircraftTakeOffAnimation["Aircraft_TakeOff"].speed = num;
		_aircraftTakeOffAnimation.Play();
		CameraSpeedEffect(90f, 1f);
		CameraShakeEffect(0.01f, 60f);
		float timer = 0f;
		bool launched = false;
		while (timer < animationTime)
		{
			if (timer > 7f && !launched)
			{
				launched = true;
				FlyAircraft();
			}
			timer += Time.deltaTime;
			yield return 0f;
		}
		Timing.KillCoroutines(_interactionSequence);
	}

	public void FlyAircraft()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_flyingSequence = Timing.RunCoroutine(FlyAircraft_Sequence());
	}

	private IEnumerator<float> FlyAircraft_Sequence()
	{
		CameraSpeedEffect(60f, 3f);
		float flightTime = 25f;
		float roll = 0f;
		CameraSpeedEffect(60f, 3f);
		CameraShakeEffect(0.01f, flightTime + 3f);
		float timer = 0f;
		while (timer < flightTime)
		{
			if (_player1.IsOwner)
			{
				float axis = _player1.Input.GetAxis(0);
				roll += axis;
				roll = Mathf.Clamp(roll, -10f, 10f);
			}
			_aircraft.localRotation = Quaternion.Lerp(_aircraft.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 0f - roll)), Time.deltaTime);
			timer += Time.deltaTime;
			yield return 0f;
		}
		CrashAircraft();
		Timing.KillCoroutines(_flyingSequence);
	}

	public void CrashAircraft()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsOwner)
			{
				allPlayer.Movement.IsInEndGameScene = true;
			}
		}
		_crashSequence = Timing.RunCoroutine(CrashAircraft_Sequence());
	}

	private IEnumerator<float> CrashAircraft_Sequence()
	{
		Singleton<Fader>.Instance?.SetFadeColor(Color.black);
		Singleton<Fader>.Instance?.FadeOut(3f);
		while (_throttledEngineAudio.volume > 0f)
		{
			AudioSource throttledEngineAudio = _throttledEngineAudio;
			throttledEngineAudio.volume -= 0.1f * Time.deltaTime;
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(5f);
		TMPTextScreenViewAdapter textView = Object.FindObjectOfType<TMPTextScreenViewAdapter>();
		textView?.SetHeaderText("AIRCRAFT_TEXT_SCREEN_LABEL_THREE_HOURS_LATER");
		textView?.SetSubheaderText("");
		textView?.SetTooltipText("");
		textView.Show();
		yield return Timing.WaitForSeconds(4f);
		yield return _introAssetLoader.Load_Async();
		_introAssetLoader.Jet.SetupEndingScene();
		textView.Hide();
		SetupCrashScene();
		Singleton<Fader>.Instance?.FadeIn(0.1f);
		_throttledEngineAudio.volume = 1f;
		CameraShakeEffect(0.001f, 60f);
		yield return Timing.WaitForSeconds(1f);
		if (MPExtensions.IsMaster(Game.Mode) && _player1.IsOwner)
		{
			EventManager.RaiseEvent(new CompletedEndGameEvent());
		}
		yield return Timing.WaitForSeconds(15f);
		_tightCameraClamp = true;
		yield return Timing.WaitForSeconds(3f);
		((Component)_crashPrivateJetTrans).gameObject.SetActive(true);
		float num = 1f;
		float animationTime = _crashAnimation["Aircraft_Crash"].length * (1f / num);
		_crashAnimation["Aircraft_Crash"].speed = num;
		_crashAnimation.Play();
		PlayAircraftAnimation("Cockpit_Crash");
		yield return Timing.WaitForSeconds(2f);
		PlayCharacterAnimation(_player1, "Cockpit_Crash");
		yield return Timing.WaitForSeconds(1f);
		AudioManager.GetAudioPlayer().Play2D(_cockpitCrashClip, AudioMixerChannels.FX);
		PlayTrackInstantly(TrackType.Cockpit_Crash);
		yield return Timing.WaitForSeconds(animationTime - 3f);
		CameraSpeedEffect(90f, 0.1f);
		_crashImpact.SetActive(true);
		_throttledEngineAudio.Stop();
		yield return Timing.WaitForSeconds(0.1f);
		_tightCameraClamp = false;
		_crashImpact.SetActive(false);
		yield return _introAssetLoader.Jet.EndingSequence();
		Crashed();
		_introAssetLoader.IntroAssets.SetActive(false);
		Timing.KillCoroutines(_rotatePropeller);
		Timing.KillCoroutines(_clampCamera);
		Timing.KillCoroutines(_crashSequence);
	}

	private void Crashed()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Crashed_Sequence());
	}

	private IEnumerator<float> Crashed_Sequence()
	{
		UVideoScreenViewAdapter video = Object.FindObjectOfType<UVideoScreenViewAdapter>();
		video?.Show();
		yield return Timing.WaitForSeconds(12f);
		yield return Singleton<Fader>.Instance.FadeOut_Async(7f);
		video?.Hide();
		Object.FindObjectOfType<EndGameCreditsController>().Show();
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsOwner)
			{
				allPlayer.Movement.IsInEndGameScene = false;
			}
		}
	}

	private void SetupCrashScene()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		((Component)((Component)this).transform.parent).transform.parent = null;
		StrandedWorld instance = Singleton<StrandedWorld>.Instance;
		if (instance != null)
		{
			((Component)instance).gameObject.SetActive(false);
		}
		((Component)Ceto.Ocean.Instance).gameObject.SetActive(false);
		GameObject.Find("Manager - Terrain Impostors").SetActive(false);
		Singleton<GameTime>.Instance.MilitaryTime = 21f;
		EventManager.RaiseEvent(new EndGameSequenceEvent());
		((Component)_crashAnimation).gameObject.SetActive(true);
		((Component)_crashAircraftTrans).gameObject.SetActive(true);
		((Component)_aircraft).transform.parent = _crashAircraftTrans;
		_aircraft.localPosition = Vector3.zero;
		_aircraft.localRotation = Quaternion.identity;
	}

	private void RotatePropellers()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_rotatePropeller = Timing.RunCoroutine(RotatePropeller());
	}

	private IEnumerator<float> RotatePropeller()
	{
		while (true)
		{
			if (_curPropellerSpeed < _maxPropellerSpeed)
			{
				_curPropellerSpeed += 0.1f * Time.deltaTime;
			}
			float num = 500f * _curPropellerSpeed * Time.deltaTime;
			((Component)_propeller).transform.Rotate(0f, 0f, 0f - num);
			yield return 0f;
		}
	}

	private void ClampCameras()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		_player1.PlayerCamera.CameraMovement.ResetCameraTarget();
		if (_player2 != null)
		{
			_player2.PlayerCamera.CameraMovement.ResetCameraTarget();
		}
		_clampCamera = Timing.RunCoroutine(ClampCameraUpdate());
	}

	private IEnumerator<float> ClampCameraUpdate()
	{
		while (true)
		{
			ClampCamera(_player1, _player1.Character.CharacterFirstPerson.CameraRigPosition);
			if (_player2 != null)
			{
				ClampCamera(_player2, _player2.Character.CharacterFirstPerson.CameraRigPosition);
			}
			yield return 0f;
		}
	}

	private void ClampCamera(IPlayer player, Transform target)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (_tightCameraClamp)
		{
			player.PlayerCamera.MouseLook.ClampViewToPosUpdate(target.forward, 10f, 10f, lerpAngles: true);
		}
		else
		{
			player.PlayerCamera.MouseLook.ClampViewToPosUpdate(target.forward, 60f, 45f);
		}
	}

	private void ActivateParts()
	{
		_enginePart.SetActive(true);
		_propellerPart.SetActive(true);
	}

	private void CloseEngineHatch()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_trolley.SetActive(false);
		float z = _engingeHatch.localRotation.z;
		LeanTween.value(((Component)this).gameObject, (Action<float>)RotateEngineHatch, z, 0f, 3f).setEase((LeanTweenType)7);
	}

	private void RotateEngineHatch(float rotation)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_engingeHatch.localRotation = Quaternion.Euler(_engingeHatch.localRotation.x, _engingeHatch.localRotation.y, rotation);
	}

	private void CloseFuelHatches()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float x = _fuelHatch1.localRotation.x;
		LeanTween.value(((Component)this).gameObject, (Action<float>)RotateFuelHatch, x, 0f, 1f).setEase((LeanTweenType)7);
	}

	private void RotateFuelHatch(float rotation)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		_fuelHatch1.localRotation = Quaternion.Euler(rotation, _fuelHatch1.localRotation.y, _fuelHatch1.localRotation.z);
		_fuelHatch2.localRotation = Quaternion.Euler(rotation, _fuelHatch2.localRotation.y, _fuelHatch2.localRotation.z);
	}

	private void RaiseRail()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_ = _rail.localRotation;
		LeanTween.value(((Component)this).gameObject, (Action<float>)RotateRail, 0f, 5f, 2f).setEase((LeanTweenType)7);
	}

	private void RotateRail(float rotation)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_rail.localRotation = Quaternion.Euler(rotation, 0f, 0f);
	}

	private void SkipToLaunch()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		_player1.Character.PlayingEventAnimation = true;
		SetPlayer2();
		_throttledEngineAudio.Play();
		_cockpitLights.SetActive(true);
		SetCharacterPosition(_player1, _cockpitOperatingPosition);
		((Component)_player1.Character).transform.parent = _cockpitOperatingPosition;
		((Component)_player1.Character).transform.localPosition = Vector3.zero;
		_maxPropellerSpeed = 2f;
		RotatePropellers();
		CloseEngineHatch();
		ActivateParts();
		_aircraftRail.localRotation = Quaternion.identity;
		_aircraft.localRotation = Quaternion.identity;
		RaiseRail();
		InteractionCheck(InteractionSequence.TakeOff);
		Timing.KillCoroutines(_enterCockpitSequence);
	}

	private void SkipToCrash()
	{
		CrashAircraft();
	}

	public void PlayTrackInstantly(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Play_Instantly));
	}

	public void PlayTrack(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Play));
	}

	public void StopTrack(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Stop));
	}

	public new JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Entered", _enteredHangar);
		jObject.AddField("EnteredHangar", _enteredHangar);
		jObject.AddField("Completed", _completed);
		return jObject;
	}

	public new void Load(JObject data)
	{
		if (data.GetField("EnteredHangar") != null)
		{
			_enteredHangar = data.GetField("EnteredHangar").GetValue<bool>();
		}
		_completed = data.GetField("Completed").GetValue<bool>();
	}
}
