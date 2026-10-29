using System;
using System.Runtime.CompilerServices;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Rendering;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using RootMotion.FinalIK;
using UnityEngine;

public class Interacting_FISHINGROD : ConstructionObject, IChargeable, IUseable, IAnimatablePrimary, IBase, IHasCraftingType, IReloadable, IAnimatableSecondary, IGameInputHandler
{
	public class ReplicateCatchFish : MultiplayerMessage
	{
		[Replicate]
		public Interacting_FISHINGROD FishingRod;

		[Replicate]
		public short FishPrefabId;

		public override void OnPeer()
		{
			FishingRod.CatchFish(GetFishPrefab((uint)FishPrefabId));
		}
	}

	public class ReplicateFishEscape : MultiplayerMessage
	{
		[Replicate]
		public Interacting_FISHINGROD FishingRod;

		public override void OnPeer()
		{
			FishingRod.FishEscape();
		}
	}

	public class ReplicateLineSnap : MultiplayerMessage
	{
		[Replicate]
		public Interacting_FISHINGROD FishingRod;

		public override void OnPeer()
		{
			if (!FishingRod.Owner.IsOwner)
			{
				FishingRod.LineSnap();
			}
		}
	}

	public class ReplicateCollectBobber : MultiplayerMessage
	{
		[Replicate]
		public Interacting_FISHINGROD FishingRod;

		public override void OnPeer()
		{
			FishingRod.CollectBobber();
		}
	}

	private static readonly Vector3 FISH_PROXY_BOBBER_Y_OFFSET = Vector3.down * 0.5f;

	[SerializeField]
	private bool _canUseSecondary;

	private bool _reloaded;

	[Header("SECONDARY ANIMATIONS")]
	[SerializeField]
	private AnimationType _secondaryIdle;

	[SerializeField]
	private AnimationType _secondary;

	[SerializeField]
	private AnimationType _reloadAnimation;

	[SerializeField]
	private RenderableProxy _bobberProxy;

	[SerializeField]
	private RenderableProxy _fishProxyPrefab;

	[SerializeField]
	private CCDIK _fishingRodIK;

	[SerializeField]
	private Transform _fishingRodIKTarget;

	[SerializeField]
	private VerletLine _fishingLine;

	[SerializeField]
	private Transform _fishingReel;

	[SerializeField]
	private Transform _fishingReelRope;

	[SerializeField]
	private float _bobberCastSpeed = 1.5f;

	[SerializeField]
	private float MaxLineLength = 15f;

	[SerializeField]
	private GameObject _fishParticle;

	private Interacting_FISHING_BOBBER _activeBobber;

	private RenderableProxy _fishProxy;

	private float _fishEndurance = 1f;

	private float _fishResistTimer;

	private float _fishRecoverTimer;

	private float _castingTimer;

	private float _bobberStuckTimer;

	private float _lastCaughtTimer;

	private bool _isHolding;

	private float _animLerp;

	private float _fishAvoidTimer = 10f;

	private Vector3 _previousBobberPos;

	private Vector3 _fishRandomDir;

	private Vector3 _springPos;

	private IFishable _currentFishable;

	private bool _casting;

	private bool _reelingIn;

	private bool _reelingOut;

	private float _lineTension;

	private float _bobberMovingSpeed;

	private bool _fishPulling;

	private bool _subscribed;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _charged;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _usedSecondary;

	public bool CanUseSecondary
	{
		get
		{
			return _canUseSecondary;
		}
		set
		{
			_canUseSecondary = value;
		}
	}

	public bool Reloaded => _reloaded;

	public AnimationType SecondaryIdle => _secondaryIdle;

	public AnimationType Secondary => _secondary;

	public AnimationType ReloadAnimation => _reloadAnimation;

	public Interacting_FISHING_BOBBER ActiveBobber => _activeBobber;

	public bool Casting => _casting;

	public bool ReelingIn => _reelingIn;

	public bool ReelingOut => _reelingOut;

	public float LineTension => _lineTension;

	public float BobberMovingSpeed => _bobberMovingSpeed;

	public bool FishPulling => _fishPulling;

	public bool HasFish => _currentFishable != null;

	public BaseActionUnityEvent Charged => _charged;

	public BaseActionUnityEvent UsedSecondary => _usedSecondary;

	public event Action LineSnapped;

	public event Action FishCaught;

	public event Action FishEscaped;

	public event Action BobberReset;

	public override void OnDestroy()
	{
		this.LineSnapped = null;
		this.FishCaught = null;
		this.FishEscaped = null;
		this.BobberReset = null;
		base.OnDestroy();
	}

	void IGameInputHandler.Initialize(IGameInputController controller)
	{
	}

	void IGameInputHandler.CheckInput()
	{
	}

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		_isHolding = holding;
		if (_isHolding)
		{
			_fishingRodIK.solver.target = _fishingRodIKTarget;
			if (base.PreviousOwnerId != -1 && !_subscribed)
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(base.PreviousOwnerId);
				playerFromId.Holder.Storage.Popped += Storage_Popped;
				playerFromId.Holder.Storage.Pushed += Storage_Pushed;
				_subscribed = true;
			}
			return;
		}
		ResetBobber();
		_fishingRodIK.gameObject.SetActive(value: false);
		_fishingLine.gameObject.SetActive(value: false);
		_bobberProxy.gameObject.SetActive(value: false);
		_fishParticle.gameObject.SetActive(value: false);
		_reloaded = false;
		if (base.PreviousOwnerId != -1 && _subscribed)
		{
			IPlayer playerFromId2 = PlayerUtilities.GetPlayerFromId(base.PreviousOwnerId);
			playerFromId2.Holder.Storage.Popped -= Storage_Popped;
			playerFromId2.Holder.Storage.Pushed -= Storage_Pushed;
			_subscribed = false;
		}
	}

	private void Storage_Pushed(IPickupable obj, bool success)
	{
		if (success)
		{
			OnStorageChanged(obj);
		}
	}

	private void Storage_Popped(IPickupable obj)
	{
		OnStorageChanged(obj);
	}

	private void OnStorageChanged(IPickupable obj)
	{
		if (CanReloadWith(obj))
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = base.Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if (interacting_FISHING_BOBBER != null)
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				return;
			}
			ResetBobber();
			_reloaded = false;
			_bobberProxy.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		if (LevelLoader.IsLoading() || base.Owner == null)
		{
			return;
		}
		base.Owner.Character.SetFloat("FishingTension", -1f);
		_fishParticle.gameObject.SetActive(_fishProxy != null);
		if (!_isHolding)
		{
			return;
		}
		if (_activeBobber == null)
		{
			Vector3 to = base.transform.position + base.transform.up * 1.5f;
			_springPos = SpringVector(_springPos, to, 0.1f);
			if (!_fishingRodIK.gameObject.activeSelf)
			{
				_fishingRodIK.gameObject.SetActive(value: true);
			}
			_fishingRodIKTarget.transform.position = _springPos;
			_fishingRodIK.solver.IKPositionWeight = 0.2f;
			if (_fishingLine.gameObject.activeSelf)
			{
				_fishingLine.gameObject.SetActive(value: false);
			}
			return;
		}
		if (base.Owner.IsOwner && _activeBobber != null)
		{
			base.Owner.GameInput.CurrentHandler = this;
		}
		if (_lastCaughtTimer > 5f)
		{
			if (_currentFishable == null && !_casting && !base.Owner.Movement.InWater && (_activeBobber.transform.position - base.Owner.transform.position).sqrMagnitude > BeamMath.Sqr(5f) && base.Owner.IsOwner)
			{
				IFishable obj = FishingManager.Instance?.GetInterestedFishableAt(_activeBobber.transform.position);
				FishBase fish = obj as FishBase;
				if (obj != null)
				{
					ReplicatedCatchFish(fish);
				}
			}
		}
		else if (_currentFishable == null)
		{
			_lastCaughtTimer += Time.deltaTime;
		}
		if (_fishingLine.gameObject.activeSelf)
		{
			_fishingLine.gameObject.SetActive(value: false);
		}
		if (!_fishingLine.gameObject.activeSelf)
		{
			_fishingLine.gameObject.SetActive(value: true);
		}
		DoReel();
	}

	private void ReplicatedCatchFish(FishBase fish)
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateCatchFish replicateCatchFish = new ReplicateCatchFish();
			replicateCatchFish.FishingRod = this;
			replicateCatchFish.FishPrefabId = (short)fish.FishingData.FishPrefab.PrefabId;
			replicateCatchFish.Post();
		}
		CatchFish(fish);
	}

	private void CatchFish(FishBase fish)
	{
		if (!(fish == null))
		{
			_currentFishable = fish;
			_currentFishable.Hook(hooked: true);
			_fishProxy = CreateFishProxy(((IFishRenderer)fish).FishTemplate.Renderable);
			_fishProxy.transform.position = ((IFishRenderer)fish).FishRenderer.transform.position;
			_fishProxy.transform.rotation = ((IFishRenderer)fish).FishRenderer.transform.rotation;
			_lineTension = 0.5f;
			_lastCaughtTimer = 0f;
			_fishRecoverTimer = _currentFishable.FishingData.RecoverTime;
			_fishEndurance = _currentFishable.FishingData.Strength;
			_activeBobber?.rigidbody.AddForce(new Vector3(0f, -5f, 0f), ForceMode.VelocityChange);
			this.FishCaught?.Invoke();
		}
	}

	private static FishBase GetFishPrefab(uint prefabID)
	{
		FishSpawner fishSpawner = UnityEngine.Object.FindObjectOfType<PlayerDetailSpawnerManager>().Fish_Spawner as FishSpawner;
		if (fishSpawner == null)
		{
			Debug.LogError("Interacting_FISHINGROD :: Load : Error Finding FishSpawners");
			return null;
		}
		foreach (CellObjectSettings<FishBase> cellObjectsSetting in fishSpawner.CellObjectsSettings)
		{
			Interactive_FISH fishPrefab = cellObjectsSetting.Prefab.FishingData.FishPrefab;
			if (fishPrefab != null && fishPrefab.PrefabId == prefabID)
			{
				return cellObjectsSetting.Prefab;
			}
		}
		return null;
	}

	private RenderableProxy CreateFishProxy(Renderable template)
	{
		RenderableProxy renderableProxy = UnityEngine.Object.Instantiate(_fishProxyPrefab);
		renderableProxy.MeshFilter.sharedMesh = template.Mesh;
		renderableProxy.Renderer.sharedMaterial = template.Material;
		return renderableProxy;
	}

	private void DoReel()
	{
		Vector3 vector = new Vector3(_fishingLine.CableStart.position.x, base.Owner.transform.position.y, _fishingLine.CableStart.position.z);
		float magnitude = (_activeBobber.transform.position - vector).magnitude;
		float num = 10f;
		_bobberMovingSpeed = (_activeBobber.transform.position - _previousBobberPos).magnitude / Time.deltaTime;
		_previousBobberPos = _activeBobber.transform.position;
		float x = Mathf.Clamp01(1f - magnitude / MaxLineLength * 0.9f);
		_fishingReelRope.localScale = new Vector3(x, 1f, 1f);
		Vector3 rotationAngle = new Vector3(_activeBobber.transform.position.x, _activeBobber.transform.position.y, _activeBobber.transform.position.z) - base.Owner.PlayerCamera.transform.position;
		rotationAngle.Normalize();
		if ((_activeBobber.transform.position - base.Owner.transform.position).sqrMagnitude > BeamMath.Sqr(0.5f))
		{
			base.Owner.PlayerCamera.MouseLook.ClampViewToPosUpdate(rotationAngle, 90f, 0f);
		}
		if (_casting)
		{
			_fishingReel.transform.Rotate(Time.deltaTime * -3600f, 0f, 0f);
			_fishingLine.CableEnd = _activeBobber.transform;
			_fishingRodIK.solver.IKPositionWeight = 0f;
			_fishingLine.CableLength = magnitude;
			if (magnitude >= MaxLineLength)
			{
				_fishingLine.CableLength = 1f;
				_activeBobber.rigidbody.velocity = Vector3.zero;
				_casting = false;
			}
			else if (_bobberMovingSpeed < 3f)
			{
				_casting = false;
			}
			_castingTimer += Time.deltaTime;
			if (_castingTimer > 5f)
			{
				_castingTimer = 0f;
				_casting = false;
			}
			return;
		}
		_reelingIn = (base.Owner.IsOwner ? base.Owner.Input.GetButton(7) : base.Owner.MultiplayerState.ReelingIn);
		if (base.Owner.IsOwner)
		{
			base.Owner.MultiplayerState.ReelingIn = _reelingIn;
		}
		_reelingOut = false;
		if (magnitude > MaxLineLength * 1.5f || (!_fishPulling && _reelingIn && _bobberMovingSpeed < 0.2f))
		{
			_bobberStuckTimer += Time.deltaTime;
		}
		else
		{
			_bobberStuckTimer = 0f;
		}
		if (_bobberStuckTimer > 2f)
		{
			_lineTension += 10f * Time.deltaTime;
			if (_lineTension >= 1f)
			{
				if (base.Owner.IsOwner)
				{
					ReplicatedLineSnap();
				}
				return;
			}
		}
		if (base.Owner.IsOwner && _reelingIn && !_casting && (Vector3Tools.SqrDistanceOnAxis(_activeBobber.transform.position, base.Owner.transform.position, Vector3Tools.xz) < BeamMath.Sqr(2f) || magnitude < 1f))
		{
			ReplicatedCollectBobber();
			return;
		}
		if (base.Owner.IsOwner && base.Owner.Movement.InWater)
		{
			ReplicatedFishEscape();
			ReplicatedCollectBobber();
			return;
		}
		base.Owner.Character.SetFloat("FishingTension", _animLerp);
		if (_currentFishable == null)
		{
			_animLerp = Mathf.Lerp(_animLerp, _reelingIn ? 0.5f : 0f, Time.deltaTime);
			_fishingRodIKTarget.transform.position = Vector3.Lerp(_fishingRodIKTarget.transform.position, _activeBobber.transform.position, Time.deltaTime * 10f);
			if (_reelingIn)
			{
				Vector3 normalized = (vector - _activeBobber.transform.position).normalized;
				_activeBobber.rigidbody.AddForce(normalized * num, ForceMode.Acceleration);
				_fishingRodIK.solver.IKPositionWeight = Mathf.Lerp(_fishingRodIK.solver.IKPositionWeight, 0.02f, Time.deltaTime);
				_fishingLine.CableLength = magnitude - 1f;
			}
			else
			{
				_fishingRodIK.solver.IKPositionWeight = Mathf.Lerp(_fishingRodIK.solver.IKPositionWeight, 0.001f, Time.deltaTime * 2f);
			}
		}
		else
		{
			_fishingLine.CableLength = 1f;
			_fishingRodIK.solver.IKPositionWeight = Mathf.Lerp(_fishingRodIK.solver.IKPositionWeight, 0.02f + 0.2f * _lineTension, Time.deltaTime);
			_fishingRodIKTarget.transform.position = Vector3.Lerp(_fishingRodIKTarget.transform.position, _activeBobber.transform.position + (_reelingIn ? (UnityEngine.Random.insideUnitSphere * (_lineTension * 1f)) : Vector3.zero), Time.deltaTime * 10f);
			_animLerp = Mathf.Lerp(_animLerp, _lineTension, Time.deltaTime);
			float num2 = 0.3f;
			if (_fishPulling)
			{
				_fishResistTimer += Time.deltaTime;
				if (_fishEndurance <= 0f || _fishResistTimer > _currentFishable.FishingData.ResistTime)
				{
					_fishResistTimer = 0f;
					_fishPulling = false;
				}
				if (_reelingIn)
				{
					_fishEndurance -= Time.deltaTime * 0.1f;
					_lineTension += Time.deltaTime * num2;
				}
				else
				{
					_reelingOut = true;
				}
			}
			else if (!_fishPulling)
			{
				_fishRecoverTimer += Time.deltaTime;
				if (_fishEndurance > 0f && _fishRecoverTimer > _currentFishable.FishingData.RecoverTime)
				{
					_fishRecoverTimer = 0f;
					_fishPulling = true;
				}
				if (_reelingIn)
				{
					Vector3 normalized2 = (vector - _activeBobber.transform.position).normalized;
					_activeBobber.rigidbody.AddForce(normalized2 * num, ForceMode.Acceleration);
				}
				else
				{
					_reelingOut = true;
				}
			}
			if (_reelingOut)
			{
				float num3 = 0.3f;
				if (_fishPulling)
				{
					num3 = 1f;
					ZigZagWaypoint(_currentFishable.FishingData.DodgeRate);
				}
				_activeBobber.rigidbody.AddForce(num3 * _currentFishable.FishingData.DodgeSpeed * _fishRandomDir, ForceMode.Acceleration);
				_activeBobber.rigidbody.AddForce(num3 * _currentFishable.FishingData.Strength * base.Owner.transform.forward, ForceMode.Acceleration);
				_lineTension -= Time.deltaTime * num2;
				if (magnitude > MaxLineLength)
				{
					_lineTension += Time.deltaTime * (num2 * 2f);
				}
			}
			_fishEndurance = Mathf.Clamp(_fishEndurance, 0f, _currentFishable.FishingData.Strength);
			if (_fishProxy != null)
			{
				Vector3 velocity = _activeBobber.rigidbody.velocity;
				velocity.y = 0f;
				velocity.Normalize();
				Vector3 b = _activeBobber.transform.position + FISH_PROXY_BOBBER_Y_OFFSET;
				Vector3 forward = velocity;
				_fishProxy.transform.position = Vector3.Lerp(_fishProxy.transform.position, b, Time.deltaTime * 7f);
				_fishProxy.transform.rotation = Quaternion.Lerp(_fishProxy.transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * 10f);
				_fishParticle.transform.position = new Vector3(_fishProxy.transform.position.x, _activeBobber.transform.position.y + 0.3f, _fishProxy.transform.position.z);
			}
		}
		_lineTension = Mathf.Clamp01(_lineTension);
		if (_lineTension >= 1f)
		{
			_fishingLine.CableLength = magnitude;
			_fishingRodIK.solver.IKPositionWeight = 0f;
			if (_reelingIn)
			{
				if (base.Owner.IsOwner)
				{
					ReplicatedLineSnap();
				}
			}
			else if (base.Owner.IsOwner)
			{
				ReplicatedFishEscape();
			}
		}
		float num4 = Mathf.Clamp(_bobberMovingSpeed * 0.5f, 0f, 1f);
		if (_reelingOut)
		{
			_fishingReel.transform.Rotate(0f - num4 * 10f, 0f, 0f);
		}
		else if (_reelingIn && !_fishPulling)
		{
			_fishingReel.transform.Rotate(num4 * 5f, 0f, 0f);
		}
	}

	private void ReplicatedFishEscape()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateFishEscape replicateFishEscape = new ReplicateFishEscape();
			replicateFishEscape.FishingRod = this;
			replicateFishEscape.Post();
		}
		FishEscape();
	}

	private void FishEscape()
	{
		this.FishEscaped?.Invoke();
		_activeBobber?.rigidbody.AddForce(new Vector3(0f, -7f, 0f), ForceMode.VelocityChange);
		if (_currentFishable != null)
		{
			_currentFishable.Hook(hooked: false);
			_currentFishable = null;
		}
		ResetBobber(detachBobber: false);
		LocalizedNotification.Post(base.Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_FISH_GOT_AWAY_DESC");
	}

	private void ReplicatedLineSnap()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateLineSnap replicateLineSnap = new ReplicateLineSnap();
			replicateLineSnap.FishingRod = this;
			replicateLineSnap.Post();
		}
		LineSnap();
	}

	private void LineSnap()
	{
		this.LineSnapped?.Invoke();
		_activeBobber?.rigidbody.AddForce(new Vector3(0f, -7f, 0f), ForceMode.VelocityChange);
		if (_currentFishable != null)
		{
			_currentFishable.Hook(hooked: false);
			_currentFishable = null;
		}
		ResetBobber();
		LocalizedNotification.Post(base.Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_LINE_SNAPPED_DESC");
	}

	private void ReplicatedCollectBobber()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateCollectBobber replicateCollectBobber = new ReplicateCollectBobber();
			replicateCollectBobber.FishingRod = this;
			replicateCollectBobber.Post();
		}
		CollectBobber();
	}

	private void CollectBobber()
	{
		if (!(_activeBobber == null))
		{
			_activeBobber.CanCollect = true;
			base.Owner.Holder.Storage.Push(_activeBobber);
			CheckBobber();
			ResetBobber();
			Reload(null);
		}
	}

	private void CheckBobber()
	{
		if (_currentFishable == null || base.Owner.Movement.InWater)
		{
			return;
		}
		if (base.Owner.IsOwner)
		{
			_currentFishable.Catch();
		}
		EventManager.RaiseEvent(new ExperienceEvent(base.Owner.Id, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
		EventManager.RaiseEvent(new AddStatsEvent(base.Owner.Id, new Stats
		{
			FishFished = 1
		}));
		if (Game.Mode.IsMaster())
		{
			Interactive_FISH interactive_FISH = MultiplayerMng.Instantiate<Interactive_FISH>(_currentFishable.FishingData.FishPrefab.PrefabId, MiniGuid.New());
			if (base.Owner.Holder.Storage.CanPush(interactive_FISH))
			{
				base.Owner.Holder.Storage.ReplicatedPush(interactive_FISH);
			}
			else
			{
				interactive_FISH.transform.position = base.Owner.transform.position;
			}
		}
		LocalizedNotification.Post(base.Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_CAUGHT_FISH_DESC", _currentFishable.FishingData.FishPrefab.DisplayName);
	}

	private void ResetBobber(bool detachBobber = true)
	{
		if (detachBobber)
		{
			if (_activeBobber != null)
			{
				_activeBobber.AttachedFishingRod = null;
			}
			_activeBobber = null;
		}
		if (_fishProxy != null)
		{
			BeamTeam.Destroy(_fishProxy.gameObject);
			_fishProxy = null;
		}
		_currentFishable = null;
		_fishingReelRope.localScale = Vector3.one;
		_bobberStuckTimer = 0f;
		_bobberMovingSpeed = 0f;
		_lineTension = 0.5f;
		_fishPulling = false;
		if (base.Owner?.GameInput.CurrentHandler == this)
		{
			base.Owner.GameInput.CurrentHandler = null;
		}
		this.BobberReset?.Invoke();
	}

	public bool CanReloadWith(IPickupable pickupable)
	{
		return pickupable.CraftingType.InteractiveType == InteractiveType.TOOLS_BOBBER;
	}

	public void Reload(IPickupable pickupable)
	{
		ResetBobber();
		if (!pickupable.IsNullOrDestroyed())
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = pickupable as Interacting_FISHING_BOBBER;
			if (interacting_FISHING_BOBBER != null && CanReloadWith(interacting_FISHING_BOBBER))
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				_reloaded = true;
			}
		}
		else
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = base.Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if (interacting_FISHING_BOBBER != null)
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				_reloaded = true;
			}
		}
	}

	public bool ValidateSecondary(IBase obj)
	{
		if (_activeBobber != null)
		{
			return false;
		}
		if (!obj.IsNullOrDestroyed())
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = obj as Interacting_FISHING_BOBBER;
			if (interacting_FISHING_BOBBER != null && CanReloadWith(interacting_FISHING_BOBBER))
			{
				return true;
			}
		}
		else
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = base.Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if (interacting_FISHING_BOBBER != null)
			{
				return true;
			}
		}
		return false;
	}

	public void PreSecondary()
	{
		base.ActionEventData.Populate(this, null);
		_charged.Invoke(this, base.ActionEventData);
	}

	public void UseSecondary()
	{
		Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = base.Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
		if (interacting_FISHING_BOBBER != null)
		{
			base.Owner.Holder.Storage.Pop(interacting_FISHING_BOBBER);
			interacting_FISHING_BOBBER.transform.position = _bobberProxy.transform.position;
			BeamRay ray = base.Owner.PlayerCamera.GetRay(5f);
			float num = Vector3.Distance(interacting_FISHING_BOBBER.transform.position, ray.OriginPoint);
			interacting_FISHING_BOBBER.transform.position = ray.OriginPoint;
			interacting_FISHING_BOBBER.transform.LookAt(ray.EndPoint);
			interacting_FISHING_BOBBER.transform.position += interacting_FISHING_BOBBER.transform.forward * num;
			Physics.IgnoreCollision(interacting_FISHING_BOBBER.GetComponent<Collider>(), GetComponent<Collider>(), ignore: true);
			_activeBobber = interacting_FISHING_BOBBER;
			_activeBobber.AttachedFishingRod = this;
			interacting_FISHING_BOBBER.rigidbody.AddForce(interacting_FISHING_BOBBER.transform.forward * _bobberCastSpeed, ForceMode.Impulse);
			interacting_FISHING_BOBBER.transform.up = Vector3.up;
			interacting_FISHING_BOBBER.rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
			interacting_FISHING_BOBBER.rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
			_casting = true;
			_bobberProxy.gameObject.SetActive(value: false);
			_reloaded = false;
			base.ActionEventData.Populate(this, null);
			_usedSecondary.Invoke(this, base.ActionEventData);
		}
	}

	public void PostSecondary()
	{
	}

	private void ZigZagWaypoint(float dodgeRate)
	{
		_fishAvoidTimer += Time.deltaTime;
		if (_fishAvoidTimer > dodgeRate)
		{
			_fishAvoidTimer = 0f;
			Vector3 lhs = base.Owner.transform.position - _activeBobber.transform.position;
			lhs.y = 0f;
			lhs.Normalize();
			Vector3 vector = Vector3.Cross(lhs, Vector3.up);
			_fishRandomDir = ((UnityEngine.Random.value < 0.5f) ? (-vector) : vector);
		}
	}

	private void CreateBobberProxy(Interacting_FISHING_BOBBER bobber)
	{
		_bobberProxy.MeshFilter.sharedMesh = bobber.Proxy.MeshFilter.sharedMesh;
		_bobberProxy.Renderer.sharedMaterial = bobber.Proxy.MeshRenderer.sharedMaterial;
		_bobberProxy.gameObject.SetActive(value: true);
	}

	private float Spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * MathF.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	private Vector3 SpringVector(Vector3 from, Vector3 to, float time)
	{
		return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
