using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class Character : MonoBehaviour, INeedPlayerComponent
{
	private bool _viewingThirdPerson;

	[FormerlySerializedAs("PlayerCharacters")]
	[SerializeField]
	private CharacterRenderer[] _playerCharacters;

	[SerializeField]
	private CharacterRenderer _characterFirstPerson;

	[SerializeField]
	private CharacterRenderer _characterThirdPerson;

	[FormerlySerializedAs("_cameraTarget")]
	[SerializeField]
	private Transform _cameraRigTransform;

	[FormerlySerializedAs("WatchPosition")]
	[SerializeField]
	private Transform _watchPosition;

	[FormerlySerializedAs("HoldingPosition")]
	[SerializeField]
	private Transform _holdingPosition;

	[FormerlySerializedAs("ItemProxiesHand")]
	[SerializeField]
	private Transform _itemProxiesHand;

	[FormerlySerializedAs("ItemProxiesBody")]
	[SerializeField]
	private Transform _itemProxiesBody;

	[FormerlySerializedAs("Animater")]
	[SerializeField]
	private Animater _animater;

	[FormerlySerializedAs("AnimatorOverrideController")]
	[SerializeField]
	private AnimatorOverrideController _animatorOverrideController;

	[SerializeField]
	private Transform _playerCharacterPosition;

	[SerializeField]
	private Transform _currentCharacterPosition;

	[FormerlySerializedAs("AimTarget")]
	[SerializeField]
	private Transform _aimTarget;

	private Vector3 _aimIKTargetPos;

	private Quaternion _aimIKTargetRot;

	private Vector3 _playerVelocity;

	private Vector3 _playerLocalVelocity;

	private float _mouseTurning;

	private bool _playingEventAnimation;

	private bool _playingAnimation;

	private Transform _ladderHelperPosition;

	private IPlayer _player;

	private const string NULL_STATE_NAME = "NULL";

	[SerializeField]
	private string _itemUseLayerName = "Item_Use";

	private Coroutine _sequence;

	private bool _showingLeftArm;

	private float inputX;

	private float inputY;

	private Vector3 _CompassCrouchPos = new Vector3(-0.133f, -0.12f, 0.04f);

	public CharacterRenderer CharacterFirstPerson => _characterFirstPerson;

	public CharacterRenderer CharacterThirdPerson => _characterThirdPerson;

	public Transform CameraRigTransform => _cameraRigTransform;

	public Transform WatchPosition => _watchPosition;

	public Transform HoldingPosition => _holdingPosition;

	public Transform ItemProxiesHand => _itemProxiesHand;

	public Transform ItemProxiesBody => _itemProxiesBody;

	public Animater Animater => _animater;

	public AnimatorOverrideController AnimatorOverrideController => _animatorOverrideController;

	public Transform PlayerCharacterPosition => _playerCharacterPosition;

	public Transform CurrentCharacterPosition
	{
		get
		{
			return _currentCharacterPosition;
		}
		set
		{
			_currentCharacterPosition = value;
		}
	}

	public Transform AimTarget => _aimTarget;

	public bool PlayingEventAnimation
	{
		get
		{
			return _playingEventAnimation;
		}
		set
		{
			_playingEventAnimation = value;
		}
	}

	public Transform LadderHelperPosition => _ladderHelperPosition;

	IPlayer INeedPlayerComponent.Player => _player;

	public event Action ShowLeftArm;

	public event Action HideLeftArm;

	protected virtual void OnShowLeftArm()
	{
		ShowLeftArm?.Invoke();
	}

	protected virtual void OnHideLeftArm()
	{
		HideLeftArm?.Invoke();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.PlayerCamera.RenderingCameraChanged += PlayerCamera_RenderingCameraChanged;
		_player.Statistics.KnockedOut += PlayerStatistics_KnockedOut;
		CreateCharacters(player);
		Initialize();
	}

	private void CreateCharacters(IPlayer player)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (player.Id > _playerCharacters.Length)
		{
			Debug.LogError((object)"Character :: Not Enough Assigned 'PlayerCharacters' for number of players");
		}
		int genderById = (int)Options.CustomSettings.GetGenderById(player.Id);
		CharacterRenderer characterRenderer = _playerCharacters[genderById];
		_player.Gender = genderById;
		_characterFirstPerson = Object.Instantiate<CharacterRenderer>(characterRenderer, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		((Object)((Component)_characterFirstPerson).gameObject).name = "FirstPerson";
		_characterFirstPerson.THIRD_PERSON_CHARACTER = false;
		_characterFirstPerson.SetPlayer(player);
		_characterThirdPerson = Object.Instantiate<CharacterRenderer>(characterRenderer, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		((Object)((Component)_characterThirdPerson).gameObject).name = "ThirdPerson";
		_characterThirdPerson.THIRD_PERSON_CHARACTER = true;
		_characterThirdPerson.SetPlayer(player);
		Player player2 = (Player)player;
		if (Object.op_Implicit((Object)(object)player2))
		{
			player2.SetStateAnimator();
		}
	}

	private void Initialize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		_animatorOverrideController = new AnimatorOverrideController();
		AnimatorOverrideController.runtimeAnimatorController = _characterFirstPerson.AnimController.runtimeAnimatorController;
		_ladderHelperPosition = new GameObject("LadderHelperPosition").transform;
		_ladderHelperPosition.parent = ((Component)this).transform.parent;
		_ladderHelperPosition.localPosition = Vector3.zero;
		((Component)_watchPosition).transform.parent = _characterFirstPerson.WatchPosition;
		_watchPosition.localPosition = Vector3.zero;
		_watchPosition.localRotation = Quaternion.identity;
	}

	public void RefreshPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_aimIKTargetPos = _currentCharacterPosition.position;
		((Component)this).transform.position = _aimIKTargetPos;
	}

	public void RefreshRotation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_aimIKTargetRot = _currentCharacterPosition.rotation;
		((Component)this).transform.rotation = _aimIKTargetRot;
	}

	private void OnDestroy()
	{
		if (_player != null && (Object)(object)_player.PlayerCamera != (Object)null)
		{
			_player.PlayerCamera.RenderingCameraChanged -= PlayerCamera_RenderingCameraChanged;
			_player.Statistics.KnockedOut -= PlayerStatistics_KnockedOut;
		}
	}

	public void SpyGlassRendering(bool value)
	{
		_characterFirstPerson.SetSpyglassRenderers(value);
	}

	private void PlayerCamera_RenderingCameraChanged(bool ourCamera)
	{
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		if (ourCamera)
		{
			bool flag = _player.PlayerCamera.CameraMode == PlayerCameraMode.First;
			_characterFirstPerson.SetRenderers(flag);
			_characterFirstPerson.SetArmEffectActive(flag);
			_characterFirstPerson.SetHeadRenderers(flag, shadowsOnly: true);
			bool flag2 = _player.PlayerCamera.CameraMode == PlayerCameraMode.Third;
			_characterThirdPerson.SetRenderers(flag2);
			_characterThirdPerson.SetArmEffectActive(flag2);
			_characterThirdPerson.SetHeadRenderers(flag2);
			_holdingPosition.parent = (flag ? _characterFirstPerson : _characterThirdPerson).HoldingPosition;
			_holdingPosition.localPosition = Vector3.zero;
			_holdingPosition.localRotation = Quaternion.identity;
			_holdingPosition.localScale = Vector3.one;
			_itemProxiesHand.parent = (flag ? _characterFirstPerson : _characterThirdPerson).HoldingPosition;
			_itemProxiesHand.localPosition = Vector3.zero;
			_itemProxiesHand.localRotation = Quaternion.identity;
			_itemProxiesHand.localScale = Vector3.one;
			_itemProxiesBody.parent = (flag ? _characterFirstPerson : _characterThirdPerson).ProxyBodyPosition;
			_itemProxiesBody.localPosition = Vector3.zero;
			_itemProxiesBody.localRotation = Quaternion.identity;
			_itemProxiesBody.localScale = Vector3.one;
			((Component)_watchPosition).transform.parent = (flag ? _characterFirstPerson : _characterThirdPerson).WatchPosition;
			_watchPosition.localPosition = Vector3.zero;
			_watchPosition.localRotation = Quaternion.identity;
			_watchPosition.localScale = Vector3.one;
		}
		else
		{
			_characterFirstPerson.SetRenderers(enabled: false);
			_characterFirstPerson.SetArmEffectActive(enabled: false);
			_characterFirstPerson.SetHeadRenderers(enabled: false);
			_characterThirdPerson.SetRenderers(enabled: true);
			_characterThirdPerson.SetArmEffectActive(enabled: true);
			_characterThirdPerson.SetHeadRenderers(enabled: true);
			_holdingPosition.parent = _characterThirdPerson.HoldingPosition;
			_holdingPosition.localPosition = Vector3.zero;
			_holdingPosition.localRotation = Quaternion.identity;
			_holdingPosition.localScale = Vector3.one;
			_itemProxiesHand.parent = _characterThirdPerson.HoldingPosition;
			_itemProxiesHand.localPosition = Vector3.zero;
			_itemProxiesHand.localRotation = Quaternion.identity;
			_itemProxiesHand.localScale = Vector3.one;
			_itemProxiesBody.parent = _characterThirdPerson.ProxyBodyPosition;
			_itemProxiesBody.localPosition = Vector3.zero;
			_itemProxiesBody.localRotation = Quaternion.identity;
			_itemProxiesBody.localScale = Vector3.one;
			((Component)_watchPosition).transform.parent = _characterThirdPerson.WatchPosition;
			_watchPosition.localPosition = Vector3.zero;
			_watchPosition.localRotation = Quaternion.identity;
			_watchPosition.localScale = Vector3.one;
		}
	}

	private void PlayerStatistics_KnockedOut(bool obj)
	{
		ResetValuesOnKnockout();
	}

	public void ResetValuesOnKnockout()
	{
		OnHideLeftArm();
		_showingLeftArm = false;
		SetFloat("watch", 0f);
	}

	public void SetCameraMode(PlayerCameraMode mode)
	{
		_viewingThirdPerson = mode == PlayerCameraMode.Third;
		SetPositions();
		PlayerCamera_RenderingCameraChanged(_player.IsOwner);
	}

	private void SetPositions()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (_viewingThirdPerson)
		{
			((Component)_cameraRigTransform).transform.parent = _characterThirdPerson.CameraPosition;
			_cameraRigTransform.localPosition = Vector3.zero;
			_cameraRigTransform.localRotation = Quaternion.identity;
			_cameraRigTransform.localScale = Vector3.one;
			((Behaviour)_characterFirstPerson.AnimationEventPlayer).enabled = false;
			((Behaviour)_characterThirdPerson.AnimationEventPlayer).enabled = true;
		}
		else
		{
			((Component)_cameraRigTransform).transform.parent = _characterFirstPerson.CameraPosition;
			_cameraRigTransform.localPosition = Vector3.zero;
			_cameraRigTransform.localRotation = Quaternion.identity;
			_cameraRigTransform.localScale = Vector3.one;
			((Behaviour)_characterThirdPerson.AnimationEventPlayer).enabled = false;
			((Behaviour)_characterFirstPerson.AnimationEventPlayer).enabled = true;
		}
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_aimIKTargetPos = _currentCharacterPosition.position;
		((Component)this).transform.position = _aimIKTargetPos;
	}

	public void SetRotation(Quaternion rotation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_aimIKTargetRot = _currentCharacterPosition.rotation;
		((Component)this).transform.rotation = _aimIKTargetRot;
	}

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (_player != null && _player.Input != null)
		{
			CheckWatch();
			if (!_playingEventAnimation)
			{
				float num = 100f;
				float num2 = 100f;
				_aimIKTargetPos = Vector3.Lerp(_aimIKTargetPos, _currentCharacterPosition.position, Time.deltaTime * num);
				_aimIKTargetRot = Quaternion.Lerp(_aimIKTargetRot, _currentCharacterPosition.rotation, Time.deltaTime * num2);
				((Component)this).transform.position = _aimIKTargetPos;
				((Component)this).transform.rotation = _aimIKTargetRot;
				_playerVelocity = _player.Movement.GetVelocity();
				_playerLocalVelocity = ((_player.Movement.Vehicle != null) ? Vector3.zero : ((Component)this).transform.InverseTransformDirection(_playerVelocity));
				_mouseTurning = Mathf.Lerp(_mouseTurning, _player.Movement.IsBusy ? 0f : _player.Input.GetAxis(2), Time.deltaTime * 8f);
			}
		}
	}

	public void CrossFade(string name, float transitionTime = 0.3f, int layer = -1, float offset = 0f)
	{
		_characterFirstPerson.AnimController.CrossFade(name, transitionTime, layer, offset);
		_characterThirdPerson.AnimController.CrossFade(name, transitionTime, layer, offset);
	}

	public void Play(string name, int layer = -1, float offset = 0f)
	{
		_characterFirstPerson.AnimController.Play(name, layer, offset);
		_characterThirdPerson.AnimController.Play(name, layer, offset);
	}

	public void SetTrigger(string name)
	{
		_characterFirstPerson.AnimController.SetTrigger(name);
		_characterThirdPerson.AnimController.SetTrigger(name);
	}

	public void SetFloat(string name, float val)
	{
		_characterFirstPerson.AnimController.SetFloat(name, val);
		_characterThirdPerson.AnimController.SetFloat(name, val);
	}

	public void SetInteger(string name, int val)
	{
		_characterFirstPerson.AnimController.SetInteger(name, val);
		_characterThirdPerson.AnimController.SetInteger(name, val);
	}

	public void SetBool(string name, bool val)
	{
		_characterFirstPerson.AnimController.SetBool(name, val);
		_characterThirdPerson.AnimController.SetBool(name, val);
	}

	public void Hold(IPickupable pickupable)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode) && !_player.IsOwner)
		{
			PlayerRegistry.LocalPlayer?.PlayerCamera?.PickupableLocalCorrection?.Set(pickupable);
		}
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			PlayIdleAnimation(AnimationType.NONE);
			return;
		}
		pickupable.transform.localPosition = pickupable.LocalHoldingPosition;
		pickupable.transform.localEulerAngles = pickupable.LocalHoldingRotation;
		PlayIdleAnimation(pickupable.Idle);
	}

	public void PlayPickupAnimation()
	{
		CrossFade("Pickup Item", 0.2f, 8);
	}

	public void PlayWarmUpCharacterAnimation(int val = 0)
	{
		SetInteger("Warm_Up_Pose", val);
		PlayEventAnimation("Warm_Up");
	}

	public void PlayDeathAnimation()
	{
		PlayEventAnimation("Death");
	}

	public void PlayEventAnimation(string animName)
	{
		SetTrigger(animName);
	}

	public Coroutine PlayUseAnimation(AnimationType type, float animationSpeed, float normalizedTime, float delay, bool useAnimation = true, Action Callback = null, AnimationBlendMode blend = AnimationBlendMode.Crossfade)
	{
		BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, _sequence);
		_sequence = ((MonoBehaviour)this).StartCoroutine(PlayUseAnimation_Sequence(type, animationSpeed, normalizedTime, delay, useAnimation, Callback, blend));
		return _sequence;
	}

	private IEnumerator PlayUseAnimation_Sequence(AnimationType type, float animationSpeed, float normalizedTime, float delay, bool useAnimation, Action Callback, AnimationBlendMode blend)
	{
		_playingAnimation = true;
		string text = type.ToString();
		AnimationClip val = _player.Character.AnimatorOverrideController[text];
		if ((Object)(object)val == (Object)null)
		{
			yield return (object)new WaitForSeconds(Math.Max(delay, 0.3f));
			Callback?.Invoke();
			_playingAnimation = false;
			yield break;
		}
		float num = val.length / Mathf.Abs(animationSpeed);
		float num2 = num * delay;
		float leftOver = num - num2;
		_playingAnimation = true;
		if (animationSpeed == 0f)
		{
			animationSpeed = 1f;
		}
		SetFloat("ItemReverseSpeed", animationSpeed);
		SetFloat("ItemUseSpeed", animationSpeed);
		if (blend == AnimationBlendMode.Crossfade)
		{
			if (useAnimation)
			{
				CrossFade(text, 0.1f, 3, normalizedTime);
				CrossFade(text, 0.33f, 6, normalizedTime);
				CrossFade(text, 0.33f, 7, normalizedTime);
			}
			else
			{
				CrossFade(text, 0.1f, 2, normalizedTime);
				CrossFade(text, 0.33f, 5, normalizedTime);
			}
		}
		else if (useAnimation)
		{
			Play(text, 3, normalizedTime);
			Play(text, 6, normalizedTime);
			Play(text, 7, normalizedTime);
		}
		else
		{
			Play(text, 2, normalizedTime);
			Play(text, 5, normalizedTime);
		}
		yield return (object)new WaitForSeconds(delay);
		Callback?.Invoke();
		if (leftOver > 0f)
		{
			yield return (object)new WaitForSeconds(leftOver);
		}
		_playingAnimation = false;
	}

	public void PlayUseAnimation(string typeName, float animationSpeed = 1f)
	{
		if (animationSpeed == 0f)
		{
			animationSpeed = 1f;
		}
		SetFloat("ItemReverseSpeed", animationSpeed);
		SetFloat("ItemUseSpeed", animationSpeed);
		CrossFade(typeName, 0.1f, 3);
		CrossFade(typeName, 0.33f, 6);
		CrossFade(typeName, 0.33f, 7);
	}

	public float PlayAnimationAsync(string name)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(PlayAnimation(name));
	}

	public CoroutineHandle PlayAnimation(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(PlayAnimation_Sequence(name));
	}

	public IEnumerator<float> PlayAnimation_Sequence(string clipName)
	{
		_playingAnimation = true;
		AnimationClip val = _player.Character.AnimatorOverrideController[clipName];
		if ((Object)(object)val != (Object)null)
		{
			CrossFade(clipName, 0.1f);
			CrossFade(clipName, 0.1f, 7);
			yield return Timing.WaitForSeconds(val.length);
		}
		_playingAnimation = false;
	}

	public IEnumerator PlayEventAnimation(string animationClip, float WaitTime = 0f, float AnimationSpeed = 1f)
	{
		_playingAnimation = true;
		yield return (object)new WaitForSeconds(WaitTime);
		_playingAnimation = false;
	}

	public void PlayIdleAnimation(AnimationType type)
	{
		PlayIdleAnimation((type == AnimationType.NONE) ? "NULL" : type.ToString());
	}

	public void PlayIdleAnimation(string clipName)
	{
		_playingAnimation = false;
		CrossFade("NULL", 0.5f, 2);
		CrossFade("NULL", 0.5f, 5);
		_characterFirstPerson.AnimController.Update(0.1f);
		_characterThirdPerson.AnimController.Update(0.1f);
		CrossFade(clipName, 0.2f, 1);
		CrossFade(clipName, 0.2f, 2);
		CrossFade(clipName, 0.5f, 4);
		CrossFade(clipName, 0.5f, 5);
	}

	public void ShowCurrentItem()
	{
		_player.Holder.ShowCurrent();
		PlayIdleAnimation((!BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject)) ? _player.Holder.CurrentObject.Idle : AnimationType.NONE);
	}

	public void HideCurrentItem()
	{
		_player.Holder.HideCurrent();
		PlayIdleAnimation("NULL");
	}

	public void NoNull(bool value)
	{
		SetBool("nonull", value);
	}

	public bool CheckIfUseStateNull()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int layerIndex = CharacterThirdPerson.AnimController.GetLayerIndex(_itemUseLayerName);
		AnimatorStateInfo currentAnimatorStateInfo = CharacterThirdPerson.AnimController.GetCurrentAnimatorStateInfo(layerIndex);
		return currentAnimatorStateInfo.IsName("NULL");
	}

	private void CheckWatch()
	{
		if (_playingAnimation)
		{
			return;
		}
		if (_player.Input.GetButton(11) && !_player.Movement.ClimbingLadder && !_player.Movement.ClimbingLedge && !_playingEventAnimation)
		{
			if (!_showingLeftArm)
			{
				ScaleWatch();
			}
			OnShowLeftArm();
			_showingLeftArm = true;
			SetFloat("watch", 1f);
			_player.PlayerCamera.ForceFirstPersonView();
		}
		else
		{
			if (_showingLeftArm)
			{
				OnHideLeftArm();
				_showingLeftArm = false;
			}
			SetFloat("watch", 0f);
		}
	}

	private void ScaleWatch()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		float fieldOfView = _player.PlayerCamera.Camera.fieldOfView;
		float num = 90f;
		float num2 = 60f;
		float num3 = (num - fieldOfView) / (num - num2);
		float num4 = -0.01f;
		float num5 = 0.025f;
		if (_player.Movement.InWater && !_player.Movement.isFloating)
		{
			num4 = 0.01f;
		}
		float num6 = num4 + num5 * num3;
		((Component)_characterFirstPerson.watchTargetParent).transform.localPosition = new Vector3(0f, 0f, num6);
	}

	public void Moving(float inputXRaw, float inputYRaw)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		inputX = Mathf.Lerp(inputX, inputXRaw, Time.deltaTime * 3f);
		inputY = Mathf.Lerp(inputY, inputYRaw, Time.deltaTime * 3f);
		SetFloat("Heading", inputX);
		SetFloat("Direction", inputY);
		SetFloat("crouch", 0f);
		SetFloat("swim", 0f);
		SetFloat("Climb", 0f);
		SetFloat("fall", 0f);
		float val = 0f;
		Vector3 val2 = new Vector3(_playerLocalVelocity.z, 0f, _playerLocalVelocity.x);
		if (_player.Movement.ClimbingLadder && Object.op_Implicit((Object)(object)_player.Movement.activeLadder))
		{
			if (((Component)_player.Movement.activeLadder).gameObject.GetComponent<Ladders>().isTree)
			{
				SetFloat("Climb", 2f);
			}
			else
			{
				SetFloat("Climb", 1f);
			}
			if (_playerLocalVelocity.y > 0.005f)
			{
				val = 1f;
			}
			else
			{
				val = ((!(_playerLocalVelocity.y < -0.005f)) ? 0f : (-1f));
			}
		}
		else if (_player.Movement.InWater)
		{
			if (_player.Movement.isFloating)
			{
				if (val2.magnitude > 0.01f)
				{
					SetFloat("swim", 2f);
				}
				else
				{
					SetFloat("swim", 1f);
				}
			}
			else if (val2.magnitude > 0.01f)
			{
				SetFloat("swim", 2f);
			}
			else
			{
				SetFloat("swim", 1f);
			}
		}
		else if (_player.Movement.grounded)
		{
			CheckBrokenBones();
			_holdingPosition.localPosition = FixCompassPosition();
			if (_player.Movement.crouched)
			{
				SetFloat("crouch", 1f);
				if (val2.magnitude > 0.01f)
				{
					val = ((!(val2.z < -0.01f)) ? 1f : (-1f));
				}
				else
				{
					val = 0f;
				}
			}
			else if (_player.Movement.sprinting)
			{
				if (val2.magnitude > 0.01f)
				{
					val = 2f;
				}
			}
			else if (val2.magnitude > 0.01f)
			{
				val = ((!(val2.z < -0.01f)) ? 1f : (-1f));
			}
			else
			{
				SetFloat("Heading", _mouseTurning * 0.1f);
			}
		}
		else if (_playerVelocity.y < 0f && _player.Movement.falling)
		{
			SetFloat("fall", 1f);
		}
		if ((Object)(object)_player.Movement.ActivePlatform != (Object)null || _player.Movement.Vehicle != null)
		{
			SetFloat("speed", 0f);
		}
		SetFloat("speed", val);
	}

	public IEnumerator ClimbAnimation()
	{
		SetFloat("LedgeClimb", 1f);
		yield return (object)new WaitForSeconds(1.688f);
		SetFloat("LedgeClimb", 0f);
		_player.Movement.FinishLedgeClimb();
	}

	public void Jump()
	{
		SetFloat("jump", 1f);
		((MonoBehaviour)this).StartCoroutine(jumptimer(0.617f));
	}

	private IEnumerator jumptimer(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
		SetFloat("jump", 0f);
	}

	public void Sliding()
	{
	}

	public void Land()
	{
		SetFloat("fall", 0f);
	}

	public void Fall_Land()
	{
		SetFloat("fall", 0f);
	}

	private void CheckBrokenBones()
	{
		bool val = _player.Statistics.HasStatusEffect(StatusEffect.BROKEN_BONES);
		SetBool("Limping", val);
	}

	public void DoSunburnEffect(float value)
	{
		_characterFirstPerson.SunburnEffect.DoEffect(value);
	}

	private Vector3 FixCompassPosition()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (_player.Movement.crouched && _player.PlayerCamera.CameraMode == PlayerCameraMode.First && _player.IsOwner && !BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject) && _player.Holder.CurrentObject.CraftingType.InteractiveType == InteractiveType.TOOLS_COMPASS)
		{
			return Vector3.Lerp(_holdingPosition.localPosition, _CompassCrouchPos, Time.deltaTime * 5f);
		}
		return Vector3.Lerp(_holdingPosition.localPosition, Vector3.zero, Time.deltaTime * 15f);
	}

	public Character()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
	}
}
