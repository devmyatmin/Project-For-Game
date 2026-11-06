using System;
using System.Collections.Generic;
using Beam.Language;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam.Crafting;

public class FireLighter : MonoBehaviour, IGameInputHandler, INeedPlayerComponent
{
	private class ReplicateStartLighting : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Construction_CAMPFIRE Fire;

		public override void OnPeer()
		{
			Player.Lighter.StartLighting(Fire);
		}
	}

	private abstract class FireLighterMessage : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public float Energy;

		public override void OnPeer()
		{
			Player.Lighter._replicatedMessages.Enqueue(this);
		}

		public abstract IEnumerator<float> Execute();
	}

	private class ReplicateSequence : FireLighterMessage
	{
		[Replicate]
		public bool Success;

		public override IEnumerator<float> Execute()
		{
			Player.Lighter._energy = Energy;
			if (Success)
			{
				return Player.Lighter.SuccessMove_Sequence();
			}
			return Player.Lighter.FailMove_Sequence();
		}
	}

	private class ReplicateInputForceStop : FireLighterMessage
	{
		public override IEnumerator<float> Execute()
		{
			Player.Lighter._inputForceStopButton = true;
			yield return 0f;
		}
	}

	private const int ATTEMPTS_REQUIRED = 6;

	private const float TOTAL_ENERGY_REQUIRED = 100f;

	private const float ENERGY_PER_PRESS = 3f;

	private const float DECAY_PER_SEC = 7.5f;

	private const string FIRE_IDLE = "LIGHT_FIRE_IDLE";

	private const string FIRE_LEFT = "LIGHT_FIRE_LEFT";

	private const string FIRE_RIGHT = "LIGHT_FIRE_RIGHT";

	private const string FIRE_FAIL = "LIGHT_FIRE_FAIL";

	private const string STICK_LIGHT_LEFT = "STICK_LIGHT_LEFT";

	private const string STICK_LIGHT_RIGHT = "STICK_LIGHT_RIGHT";

	[SerializeField]
	private Light _pointLight;

	[SerializeField]
	private ParticleSystem _emberParticles;

	[SerializeField]
	private ParticleSystem _smokeParticles;

	[SerializeField]
	private AudioClip[] _rubbingAudioClips;

	[SerializeField]
	private AudioClip _failedAudioClip;

	[SerializeField]
	private AudioClip _failedFemaleAudioClip;

	[SerializeField]
	private SceneGameAction _action;

	private IPlayer _player;

	private IGameInputController _controller;

	private bool _isLightingFire;

	private CoroutineHandle _fireSequence;

	private bool _inputLeftButton;

	private bool _inputRightButton;

	private bool _inputForceStopButton;

	private Construction_CAMPFIRE _fire;

	private CoroutineHandle _animHandle;

	private AudioActorHandle _audioHandle;

	private bool _inputLeft;

	private Animation _proxyAnim;

	private float _energy;

	private Queue<FireLighterMessage> _replicatedMessages = new Queue<FireLighterMessage>();

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsLightingFire => _isLightingFire;

	public event Action<bool> ToggleFiring;

	public event Action<float> EnergyPercentUpdated;

	protected virtual void OnToggleFiring(bool firing)
	{
		ToggleFiring?.Invoke(firing);
	}

	protected virtual void OnEnergyPercentUpdated(float energy)
	{
		EnergyPercentUpdated?.Invoke(energy);
	}

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
		_controller.Interacter.RequestGameActions += Interacter_RequestGameActions;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_controller))
		{
			_controller.Interacter.RequestGameActions -= Interacter_RequestGameActions;
		}
	}

	private void Interacter_RequestGameActions(Interacter sender, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return;
		}
		Construction_CAMPFIRE component = obj.gameObject.GetComponent<Construction_CAMPFIRE>();
		if ((Object)(object)component != (Object)null && !component.IsBurning)
		{
			IPickupable currentObject = _player.Holder.CurrentObject;
			if (!BeamTeam.IsNullOrDestroyed(currentObject) && currentObject.CraftingType.InteractiveType == InteractiveType.TOOLS_KINDLING)
			{
				sender.AddGameAction(_action);
			}
		}
	}

	public void CheckInput()
	{
	}

	public void FireLighting_GameAction(IBase obj)
	{
		if (!_player.Movement.grounded)
		{
			return;
		}
		Construction_CAMPFIRE component = obj.gameObject.GetComponent<Construction_CAMPFIRE>();
		if ((Object)(object)component != (Object)null)
		{
			if (component.FuelHours > 0f)
			{
				ReplicatedStartLighting(component);
			}
			else
			{
				LocalizedNotification.Post(_player, NotificationPriority.Immediate, 4f, "FIRE_LIGHTER_NTF_OUT_OF_FUEL_DESC");
			}
		}
	}

	private void StartLighting(Construction_CAMPFIRE fire)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_fire = fire;
		_fire.IsOnUsing = true;
		Timing.RunCoroutine(Lighting_Sequence());
	}

	private void GetInput()
	{
		_inputLeftButton = _player.Input.GetButtonDown(79);
		_inputRightButton = _player.Input.GetButtonDown(80);
		_inputForceStopButton = _player.Input.GetButtonDown(4) || _player.Input.GetButtonDown(39) || _player.Input.GetButtonDown(31);
	}

	private IEnumerator<float> FailMove_Sequence()
	{
		AudioClip clip = ((_player.Gender == 0) ? _failedAudioClip : _failedFemaleAudioClip);
		_audioHandle = AudioManager.GetAudioPlayer().Play3D(clip, ((Component)this).transform.position, AudioMixerChannels.FX);
		_animHandle = _player.Character.PlayAnimation("LIGHT_FIRE_FAIL");
		_player.PlayerSkills.ReplicatedAddPhysical(PlayerSkills.ExpSource.CAMPFIRE_FAIL);
		yield return Timing.WaitForSeconds(1f);
	}

	private IEnumerator<float> SuccessMove_Sequence()
	{
		_audioHandle = AudioManager.GetAudioPlayer().Play3D(_rubbingAudioClips, ((Component)this).transform.position, AudioMixerChannels.FX);
		_animHandle = _player.Character.PlayAnimation(_inputLeft ? "LIGHT_FIRE_LEFT" : "LIGHT_FIRE_RIGHT");
		_proxyAnim.Play(_inputLeft ? "STICK_LIGHT_LEFT" : "STICK_LIGHT_RIGHT");
		_inputLeft = !_inputLeft;
		yield return 0f;
	}

	private IEnumerator<float> Lighting_Sequence()
	{
		_replicatedMessages.Clear();
		_isLightingFire = true;
		if (_player.IsOwner)
		{
			_controller.CurrentHandler = this;
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 13);
			RewiredUtilities.SetCursorEnabled(_player.Input, enabled: false);
			_player.PlayerCamera.CameraMovement.SetRotation(_player.Character.CameraRigTransform);
			_player.PlayerUI.Suspend(suspend: true);
		}
		_player.Character.HideCurrentItem();
		GameObject proxy = _player.Holder.Proxies.GetProxy(new CraftingType(AttributeType.None, InteractiveType.TOOLS_KINDLING));
		proxy.SetActive(true);
		_proxyAnim = proxy.GetComponent<Animation>();
		_player.Character.SetBool("starting_fire", val: true);
		yield return 0f;
		_player.Character.PlayAnimation("LIGHT_FIRE_IDLE");
		PlatformLocalizedText platformLocalizedText = new PlatformLocalizedText();
		platformLocalizedText.SetPlayer(_player.Id);
		platformLocalizedText.SetTerm("FIRE_LIGHTER_NTF_PRESS_TO_LIGHT");
		platformLocalizedText.SetActions(79, 80);
		LocalizedNotification.Post(_player, NotificationPriority.Immediate, 8f, platformLocalizedText);
		bool completed = false;
		bool forceStop = false;
		_inputForceStopButton = false;
		_inputLeft = true;
		_energy = 0f;
		if (_player.IsOwner)
		{
			OnToggleFiring(firing: true);
		}
		_animHandle = default;
		_audioHandle = default;
		EmissionModule ep = _emberParticles.emission;
		EmissionModule sp = _smokeParticles.emission;
		_emberParticles.Clear();
		if (!_emberParticles.isPlaying)
		{
			_emberParticles.Play();
		}
		_smokeParticles.Clear();
		if (!_smokeParticles.isPlaying)
		{
			_smokeParticles.Play();
		}
		if (!BeamTeam.IsNullOrDestroyed(_fire) && _player.IsOwner)
		{
			Vector3 val = _fire.transform.position - _player.transform.position;
			val.y = 0f;
			float magnitude = val.magnitude;
			Vector3 position = _player.transform.position + val.normalized * (magnitude - 1f);
			position.y = _fire.transform.position.y + 0.9f;
			if (magnitude > 1.5f)
			{
				_player.transform.position = position;
			}
		}
		float physicalLevelValue = _player.PlayerSkills.PhysicalPercentage;
		int numberOfFails = 0;
		float value = Random.value;
		float failAtEnergy = Random.Range(30, 70);
		bool willFail = value > physicalLevelValue;
		while (!completed && !forceStop)
		{
			_energy -= Time.deltaTime * 7.5f;
			if (_energy < 0f)
			{
				_energy = 0f;
			}
			if (_player.IsOwner)
			{
				GetInput();
				if ((_inputLeftButton && _inputLeft) || (_inputRightButton && !_inputLeft))
				{
					if (((PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard) & willFail) && _energy >= failAtEnergy)
					{
						_energy = 20f;
						numberOfFails++;
						_player.SetVibration(0, 0.5f, 0.15f);
						value = Random.value;
						failAtEnergy = Random.Range(30, 70);
						physicalLevelValue = Mathf.Clamp(_player.PlayerSkills.PhysicalPercentage, 0.2f, 1f);
						willFail = value > physicalLevelValue && numberOfFails < 5;
						yield return Timing.WaitUntilDone(ReplicatedFailMove_Sequence());
					}
					else
					{
						_energy += 3f + physicalLevelValue;
						yield return Timing.WaitUntilDone(ReplicatedSuccessMove_Sequence());
					}
				}
				if (_inputForceStopButton)
				{
					ReplicateInputForceStop replicateInputForceStop = new ReplicateInputForceStop();
					replicateInputForceStop.Player = _player;
					replicateInputForceStop.Energy = _energy;
					replicateInputForceStop.Post();
				}
				OnEnergyPercentUpdated(_energy / 100f);
			}
			else
			{
				while (_replicatedMessages.Count != 0)
				{
					yield return Timing.WaitUntilDone(_replicatedMessages.Dequeue().Execute());
				}
			}
			ep.rateOverTime = MinMaxCurve.op_Implicit(Mathf.Clamp(_energy, 0f, 50f));
			sp.rateOverTime = MinMaxCurve.op_Implicit(Mathf.Clamp(_energy, 0f, 50f));
			float num = Mathf.PerlinNoise(5f, Time.time * 5f);
			float num2 = _energy * 0.03f;
			_pointLight.intensity = Mathf.Lerp(0f, num2, num);
			if (_inputForceStopButton || _player.Statistics.IsKnockedOut)
			{
				_fire.IsOnUsing = false;
				forceStop = true;
				_isLightingFire = false;
				AudioManager.GetAudioPlayer().Stop(_audioHandle);
				_proxyAnim["STICK_LIGHT_LEFT"].normalizedTime = 0f;
				_proxyAnim["STICK_LIGHT_RIGHT"].normalizedTime = 0f;
				_player.Character.SetBool("starting_fire", val: false);
			}
			else if (_energy >= 100f)
			{
				_player.SetVibration(0, 0.3f, 0.1f);
				_fire.Ignite(_player);
				_player.PlayerSkills.ReplicatedAddPhysical(PlayerSkills.ExpSource.CAMPFIRE_LIGHT);
				completed = true;
				_fire.IsOnUsing = false;
				_fire = null;
			}
			yield return 0f;
		}
		if (completed)
		{
			while (_animHandle.IsRunning)
			{
				yield return 0f;
			}
		}
		_animHandle.IsRunning = false;
		AudioManager.GetAudioPlayer().Stop(_audioHandle);
		proxy.SetActive(false);
		if (_player.IsOwner)
		{
			OnToggleFiring(firing: false);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 13);
			_player.PlayerUI.Suspend(suspend: false);
			_controller.CurrentHandler = null;
			_controller.Raycaster.Reset();
		}
		_player.Character.ShowCurrentItem();
		_isLightingFire = false;
		_player.Character.SetBool("starting_fire", val: false);
	}

	private void ReplicatedStartLighting(Construction_CAMPFIRE fire)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateStartLighting replicateStartLighting = new ReplicateStartLighting();
			replicateStartLighting.Player = _player;
			replicateStartLighting.Fire = fire;
			replicateStartLighting.Post();
		}
		StartLighting(fire);
	}

	private IEnumerator<float> ReplicatedSuccessMove_Sequence()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateSequence replicateSequence = new ReplicateSequence();
			replicateSequence.Player = _player;
			replicateSequence.Energy = _energy;
			replicateSequence.Success = true;
			replicateSequence.Post();
		}
		return SuccessMove_Sequence();
	}

	private IEnumerator<float> ReplicatedFailMove_Sequence()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateSequence replicateSequence = new ReplicateSequence();
			replicateSequence.Player = _player;
			replicateSequence.Energy = _energy;
			replicateSequence.Success = false;
			replicateSequence.Post();
		}
		return FailMove_Sequence();
	}
}
