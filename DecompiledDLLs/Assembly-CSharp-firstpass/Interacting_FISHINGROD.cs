using System;
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
using UnityEngine.Events;

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
		LineSnapped = null;
		FishCaught = null;
		FishEscaped = null;
		BobberReset = null;
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
			((IKSolverHeuristic)_fishingRodIK.solver).target = _fishingRodIKTarget;
			if (PreviousOwnerId != -1 && !_subscribed)
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(PreviousOwnerId);
				playerFromId.Holder.Storage.Popped += Storage_Popped;
				playerFromId.Holder.Storage.Pushed += Storage_Pushed;
				_subscribed = true;
			}
			return;
		}
		ResetBobber();
		((Component)_fishingRodIK).gameObject.SetActive(false);
		((Component)_fishingLine).gameObject.SetActive(false);
		((Component)_bobberProxy).gameObject.SetActive(false);
		_fishParticle.gameObject.SetActive(false);
		_reloaded = false;
		if (PreviousOwnerId != -1 && _subscribed)
		{
			IPlayer playerFromId2 = PlayerUtilities.GetPlayerFromId(PreviousOwnerId);
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
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if ((Object)(object)interacting_FISHING_BOBBER != (Object)null)
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				return;
			}
			ResetBobber();
			_reloaded = false;
			((Component)_bobberProxy).gameObject.SetActive(false);
		}
	}

	private void FixedUpdate()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		if (LevelLoader.IsLoading() || Owner == null)
		{
			return;
		}
		Owner.Character.SetFloat("FishingTension", -1f);
		_fishParticle.gameObject.SetActive((Object)(object)_fishProxy != (Object)null);
		if (!_isHolding)
		{
			return;
		}
		if ((Object)(object)_activeBobber == (Object)null)
		{
			Vector3 to = transform.position + transform.up * 1.5f;
			_springPos = SpringVector(_springPos, to, 0.1f);
			if (!((Component)_fishingRodIK).gameObject.activeSelf)
			{
				((Component)_fishingRodIK).gameObject.SetActive(true);
			}
			((Component)_fishingRodIKTarget).transform.position = _springPos;
			((IKSolver)_fishingRodIK.solver).IKPositionWeight = 0.2f;
			if (((Component)_fishingLine).gameObject.activeSelf)
			{
				((Component)_fishingLine).gameObject.SetActive(false);
			}
			return;
		}
		if (Owner.IsOwner && (Object)(object)_activeBobber != (Object)null)
		{
			Owner.GameInput.CurrentHandler = this;
		}
		if (_lastCaughtTimer > 5f)
		{
			if (_currentFishable == null && !_casting && !Owner.Movement.InWater)
			{
				Vector3 val = _activeBobber.transform.position - Owner.transform.position;
				if (val.sqrMagnitude > BeamMath.Sqr(5f) && Owner.IsOwner)
				{
					IFishable fishable = FishingManager.Instance?.GetInterestedFishableAt(_activeBobber.transform.position);
					FishBase fish = fishable as FishBase;
					if (fishable != null)
					{
						ReplicatedCatchFish(fish);
					}
				}
			}
		}
		else if (_currentFishable == null)
		{
			_lastCaughtTimer += Time.deltaTime;
		}
		if (((Component)_fishingLine).gameObject.activeSelf)
		{
			((Component)_fishingLine).gameObject.SetActive(false);
		}
		if (!((Component)_fishingLine).gameObject.activeSelf)
		{
			((Component)_fishingLine).gameObject.SetActive(true);
		}
		DoReel();
	}

	private void ReplicatedCatchFish(FishBase fish)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
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
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)fish == (Object)null))
		{
			_currentFishable = fish;
			_currentFishable.Hook(hooked: true);
			_fishProxy = CreateFishProxy(((IFishRenderer)fish).FishTemplate.Renderable);
			((Component)_fishProxy).transform.position = ((Component)((IFishRenderer)fish).FishRenderer).transform.position;
			((Component)_fishProxy).transform.rotation = ((Component)((IFishRenderer)fish).FishRenderer).transform.rotation;
			_lineTension = 0.5f;
			_lastCaughtTimer = 0f;
			_fishRecoverTimer = _currentFishable.FishingData.RecoverTime;
			_fishEndurance = _currentFishable.FishingData.Strength;
			Interacting_FISHING_BOBBER activeBobber = _activeBobber;
			if (activeBobber != null)
			{
				activeBobber.rigidbody.AddForce(new Vector3(0f, -5f, 0f), (ForceMode)2);
			}
			FishCaught?.Invoke();
		}
	}

	private static FishBase GetFishPrefab(uint prefabID)
	{
		FishSpawner fishSpawner = Object.FindObjectOfType<PlayerDetailSpawnerManager>().Fish_Spawner as FishSpawner;
		if ((Object)(object)fishSpawner == (Object)null)
		{
			Debug.LogError((object)"Interacting_FISHINGROD :: Load : Error Finding FishSpawners");
			return null;
		}
		foreach (CellObjectSettings<FishBase> cellObjectsSetting in fishSpawner.CellObjectsSettings)
		{
			Interactive_FISH fishPrefab = cellObjectsSetting.Prefab.FishingData.FishPrefab;
			if ((Object)(object)fishPrefab != (Object)null && fishPrefab.PrefabId == prefabID)
			{
				return cellObjectsSetting.Prefab;
			}
		}
		return null;
	}

	private RenderableProxy CreateFishProxy(Renderable template)
	{
		RenderableProxy renderableProxy = Object.Instantiate<RenderableProxy>(_fishProxyPrefab);
		renderableProxy.MeshFilter.sharedMesh = template.Mesh;
		renderableProxy.Renderer.sharedMaterial = template.Material;
		return renderableProxy;
	}

	private void DoReel()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_088e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0910: Unknown result type (might be due to invalid IL or missing references)
		//IL_0915: Unknown result type (might be due to invalid IL or missing references)
		//IL_0917: Unknown result type (might be due to invalid IL or missing references)
		//IL_0927: Unknown result type (might be due to invalid IL or missing references)
		//IL_0947: Unknown result type (might be due to invalid IL or missing references)
		//IL_095c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0977: Unknown result type (might be due to invalid IL or missing references)
		//IL_0981: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0808: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		//IL_073d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0742: Unknown result type (might be due to invalid IL or missing references)
		//IL_0747: Unknown result type (might be due to invalid IL or missing references)
		//IL_074b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0750: Unknown result type (might be due to invalid IL or missing references)
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0760: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(_fishingLine.CableStart.position.x, Owner.transform.position.y, _fishingLine.CableStart.position.z);
		Vector3 val2 = _activeBobber.transform.position - val;
		float magnitude = val2.magnitude;
		float num = 10f;
		val2 = _activeBobber.transform.position - _previousBobberPos;
		_bobberMovingSpeed = val2.magnitude / Time.deltaTime;
		_previousBobberPos = _activeBobber.transform.position;
		float num2 = Mathf.Clamp01(1f - magnitude / MaxLineLength * 0.9f);
		_fishingReelRope.localScale = new Vector3(num2, 1f, 1f);
		Vector3 rotationAngle = new Vector3(_activeBobber.transform.position.x, _activeBobber.transform.position.y, _activeBobber.transform.position.z) - ((Component)Owner.PlayerCamera).transform.position;
		rotationAngle.Normalize();
		val2 = _activeBobber.transform.position - Owner.transform.position;
		if (val2.sqrMagnitude > BeamMath.Sqr(0.5f))
		{
			Owner.PlayerCamera.MouseLook.ClampViewToPosUpdate(rotationAngle, 90f, 0f);
		}
		if (_casting)
		{
			((Component)_fishingReel).transform.Rotate(Time.deltaTime * -3600f, 0f, 0f);
			_fishingLine.CableEnd = _activeBobber.transform;
			((IKSolver)_fishingRodIK.solver).IKPositionWeight = 0f;
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
		_reelingIn = (Owner.IsOwner ? Owner.Input.GetButton(7) : Owner.MultiplayerState.ReelingIn);
		if (Owner.IsOwner)
		{
			Owner.MultiplayerState.ReelingIn = _reelingIn;
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
				if (Owner.IsOwner)
				{
					ReplicatedLineSnap();
				}
				return;
			}
		}
		if (Owner.IsOwner && _reelingIn && !_casting && (Vector3Tools.SqrDistanceOnAxis(_activeBobber.transform.position, Owner.transform.position, Vector3Tools.xz) < BeamMath.Sqr(2f) || magnitude < 1f))
		{
			ReplicatedCollectBobber();
			return;
		}
		if (Owner.IsOwner && Owner.Movement.InWater)
		{
			ReplicatedFishEscape();
			ReplicatedCollectBobber();
			return;
		}
		Owner.Character.SetFloat("FishingTension", _animLerp);
		if (_currentFishable == null)
		{
			_animLerp = Mathf.Lerp(_animLerp, _reelingIn ? 0.5f : 0f, Time.deltaTime);
			((Component)_fishingRodIKTarget).transform.position = Vector3.Lerp(((Component)_fishingRodIKTarget).transform.position, _activeBobber.transform.position, Time.deltaTime * 10f);
			if (_reelingIn)
			{
				val2 = val - _activeBobber.transform.position;
				Vector3 normalized = val2.normalized;
				_activeBobber.rigidbody.AddForce(normalized * num, (ForceMode)5);
				((IKSolver)_fishingRodIK.solver).IKPositionWeight = Mathf.Lerp(((IKSolver)_fishingRodIK.solver).IKPositionWeight, 0.02f, Time.deltaTime);
				_fishingLine.CableLength = magnitude - 1f;
			}
			else
			{
				((IKSolver)_fishingRodIK.solver).IKPositionWeight = Mathf.Lerp(((IKSolver)_fishingRodIK.solver).IKPositionWeight, 0.001f, Time.deltaTime * 2f);
			}
		}
		else
		{
			_fishingLine.CableLength = 1f;
			((IKSolver)_fishingRodIK.solver).IKPositionWeight = Mathf.Lerp(((IKSolver)_fishingRodIK.solver).IKPositionWeight, 0.02f + 0.2f * _lineTension, Time.deltaTime);
			((Component)_fishingRodIKTarget).transform.position = Vector3.Lerp(((Component)_fishingRodIKTarget).transform.position, _activeBobber.transform.position + (_reelingIn ? (Random.insideUnitSphere * (_lineTension * 1f)) : Vector3.zero), Time.deltaTime * 10f);
			_animLerp = Mathf.Lerp(_animLerp, _lineTension, Time.deltaTime);
			float num3 = 0.3f;
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
					_lineTension += Time.deltaTime * num3;
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
					val2 = val - _activeBobber.transform.position;
					Vector3 normalized2 = val2.normalized;
					_activeBobber.rigidbody.AddForce(normalized2 * num, (ForceMode)5);
				}
				else
				{
					_reelingOut = true;
				}
			}
			if (_reelingOut)
			{
				float num4 = 0.3f;
				if (_fishPulling)
				{
					num4 = 1f;
					ZigZagWaypoint(_currentFishable.FishingData.DodgeRate);
				}
				_activeBobber.rigidbody.AddForce(num4 * _currentFishable.FishingData.DodgeSpeed * _fishRandomDir, (ForceMode)5);
				_activeBobber.rigidbody.AddForce(num4 * _currentFishable.FishingData.Strength * Owner.transform.forward, (ForceMode)5);
				_lineTension -= Time.deltaTime * num3;
				if (magnitude > MaxLineLength)
				{
					_lineTension += Time.deltaTime * (num3 * 2f);
				}
			}
			_fishEndurance = Mathf.Clamp(_fishEndurance, 0f, _currentFishable.FishingData.Strength);
			if ((Object)(object)_fishProxy != (Object)null)
			{
				Vector3 velocity = _activeBobber.rigidbody.velocity;
				velocity.y = 0f;
				velocity.Normalize();
				Vector3 val3 = _activeBobber.transform.position + FISH_PROXY_BOBBER_Y_OFFSET;
				Vector3 val4 = velocity;
				((Component)_fishProxy).transform.position = Vector3.Lerp(((Component)_fishProxy).transform.position, val3, Time.deltaTime * 7f);
				((Component)_fishProxy).transform.rotation = Quaternion.Lerp(((Component)_fishProxy).transform.rotation, Quaternion.LookRotation(val4), Time.deltaTime * 10f);
				_fishParticle.transform.position = new Vector3(((Component)_fishProxy).transform.position.x, _activeBobber.transform.position.y + 0.3f, ((Component)_fishProxy).transform.position.z);
			}
		}
		_lineTension = Mathf.Clamp01(_lineTension);
		if (_lineTension >= 1f)
		{
			_fishingLine.CableLength = magnitude;
			((IKSolver)_fishingRodIK.solver).IKPositionWeight = 0f;
			if (_reelingIn)
			{
				if (Owner.IsOwner)
				{
					ReplicatedLineSnap();
				}
			}
			else if (Owner.IsOwner)
			{
				ReplicatedFishEscape();
			}
		}
		float num5 = Mathf.Clamp(_bobberMovingSpeed * 0.5f, 0f, 1f);
		if (_reelingOut)
		{
			((Component)_fishingReel).transform.Rotate(0f - num5 * 10f, 0f, 0f);
		}
		else if (_reelingIn && !_fishPulling)
		{
			((Component)_fishingReel).transform.Rotate(num5 * 5f, 0f, 0f);
		}
	}

	private void ReplicatedFishEscape()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateFishEscape replicateFishEscape = new ReplicateFishEscape();
			replicateFishEscape.FishingRod = this;
			replicateFishEscape.Post();
		}
		FishEscape();
	}

	private void FishEscape()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		FishEscaped?.Invoke();
		Interacting_FISHING_BOBBER activeBobber = _activeBobber;
		if (activeBobber != null)
		{
			activeBobber.rigidbody.AddForce(new Vector3(0f, -7f, 0f), (ForceMode)2);
		}
		if (_currentFishable != null)
		{
			_currentFishable.Hook(hooked: false);
			_currentFishable = null;
		}
		ResetBobber(detachBobber: false);
		LocalizedNotification.Post(Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_FISH_GOT_AWAY_DESC");
	}

	private void ReplicatedLineSnap()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateLineSnap replicateLineSnap = new ReplicateLineSnap();
			replicateLineSnap.FishingRod = this;
			replicateLineSnap.Post();
		}
		LineSnap();
	}

	private void LineSnap()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		LineSnapped?.Invoke();
		Interacting_FISHING_BOBBER activeBobber = _activeBobber;
		if (activeBobber != null)
		{
			activeBobber.rigidbody.AddForce(new Vector3(0f, -7f, 0f), (ForceMode)2);
		}
		if (_currentFishable != null)
		{
			_currentFishable.Hook(hooked: false);
			_currentFishable = null;
		}
		ResetBobber();
		LocalizedNotification.Post(Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_LINE_SNAPPED_DESC");
	}

	private void ReplicatedCollectBobber()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateCollectBobber replicateCollectBobber = new ReplicateCollectBobber();
			replicateCollectBobber.FishingRod = this;
			replicateCollectBobber.Post();
		}
		CollectBobber();
	}

	private void CollectBobber()
	{
		if (!((Object)(object)_activeBobber == (Object)null))
		{
			_activeBobber.CanCollect = true;
			Owner.Holder.Storage.Push(_activeBobber);
			CheckBobber();
			ResetBobber();
			Reload(null);
		}
	}

	private void CheckBobber()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (_currentFishable == null || Owner.Movement.InWater)
		{
			return;
		}
		if (Owner.IsOwner)
		{
			_currentFishable.Catch();
		}
		EventManager.RaiseEvent(new ExperienceEvent(Owner.Id, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
		EventManager.RaiseEvent(new AddStatsEvent(Owner.Id, new Stats
		{
			FishFished = 1
		}));
		if (MPExtensions.IsMaster(Game.Mode))
		{
			Interactive_FISH interactive_FISH = MultiplayerMng.Instantiate<Interactive_FISH>(_currentFishable.FishingData.FishPrefab.PrefabId, MiniGuid.New());
			if (Owner.Holder.Storage.CanPush(interactive_FISH))
			{
				Owner.Holder.Storage.ReplicatedPush(interactive_FISH);
			}
			else
			{
				interactive_FISH.transform.position = Owner.transform.position;
			}
		}
		LocalizedNotification.Post(Owner, NotificationPriority.Immediate, 4f, "FISHING_ROD_NTF_CAUGHT_FISH_DESC", _currentFishable.FishingData.FishPrefab.DisplayName);
	}

	private void ResetBobber(bool detachBobber = true)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (detachBobber)
		{
			if ((Object)(object)_activeBobber != (Object)null)
			{
				_activeBobber.AttachedFishingRod = null;
			}
			_activeBobber = null;
		}
		if ((Object)(object)_fishProxy != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)_fishProxy).gameObject);
			_fishProxy = null;
		}
		_currentFishable = null;
		_fishingReelRope.localScale = Vector3.one;
		_bobberStuckTimer = 0f;
		_bobberMovingSpeed = 0f;
		_lineTension = 0.5f;
		_fishPulling = false;
		if (Owner?.GameInput.CurrentHandler == this)
		{
			Owner.GameInput.CurrentHandler = null;
		}
		BobberReset?.Invoke();
	}

	public bool CanReloadWith(IPickupable pickupable)
	{
		return pickupable.CraftingType.InteractiveType == InteractiveType.TOOLS_BOBBER;
	}

	public void Reload(IPickupable pickupable)
	{
		ResetBobber();
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = pickupable as Interacting_FISHING_BOBBER;
			if ((Object)(object)interacting_FISHING_BOBBER != (Object)null && CanReloadWith(interacting_FISHING_BOBBER))
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				_reloaded = true;
			}
		}
		else
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if ((Object)(object)interacting_FISHING_BOBBER != (Object)null)
			{
				CreateBobberProxy(interacting_FISHING_BOBBER);
				_reloaded = true;
			}
		}
	}

	public bool ValidateSecondary(IBase obj)
	{
		if ((Object)(object)_activeBobber != (Object)null)
		{
			return false;
		}
		if (!BeamTeam.IsNullOrDestroyed(obj))
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = obj as Interacting_FISHING_BOBBER;
			if ((Object)(object)interacting_FISHING_BOBBER != (Object)null && CanReloadWith(interacting_FISHING_BOBBER))
			{
				return true;
			}
		}
		else
		{
			Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
			if ((Object)(object)interacting_FISHING_BOBBER != (Object)null)
			{
				return true;
			}
		}
		return false;
	}

	public void PreSecondary()
	{
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)_charged).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
	}

	public void UseSecondary()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		Interacting_FISHING_BOBBER interacting_FISHING_BOBBER = Owner.Holder.Storage.Get<Interacting_FISHING_BOBBER>();
		if ((Object)(object)interacting_FISHING_BOBBER != (Object)null)
		{
			Owner.Holder.Storage.Pop(interacting_FISHING_BOBBER);
			interacting_FISHING_BOBBER.transform.position = ((Component)_bobberProxy).transform.position;
			BeamRay ray = Owner.PlayerCamera.GetRay(5f);
			float num = Vector3.Distance(interacting_FISHING_BOBBER.transform.position, ray.OriginPoint);
			interacting_FISHING_BOBBER.transform.position = ray.OriginPoint;
			interacting_FISHING_BOBBER.transform.LookAt(ray.EndPoint);
			Transform val = interacting_FISHING_BOBBER.transform;
			val.position += interacting_FISHING_BOBBER.transform.forward * num;
			Physics.IgnoreCollision(((Component)interacting_FISHING_BOBBER).GetComponent<Collider>(), ((Component)this).GetComponent<Collider>(), true);
			_activeBobber = interacting_FISHING_BOBBER;
			_activeBobber.AttachedFishingRod = this;
			interacting_FISHING_BOBBER.rigidbody.AddForce(interacting_FISHING_BOBBER.transform.forward * _bobberCastSpeed, (ForceMode)1);
			interacting_FISHING_BOBBER.transform.up = Vector3.up;
			interacting_FISHING_BOBBER.rigidbody.constraints = (RigidbodyConstraints)64;
			interacting_FISHING_BOBBER.rigidbody.constraints = (RigidbodyConstraints)16;
			_casting = true;
			((Component)_bobberProxy).gameObject.SetActive(false);
			_reloaded = false;
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)_usedSecondary).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		}
	}

	public void PostSecondary()
	{
	}

	private void ZigZagWaypoint(float dodgeRate)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		_fishAvoidTimer += Time.deltaTime;
		if (_fishAvoidTimer > dodgeRate)
		{
			_fishAvoidTimer = 0f;
			Vector3 val = Owner.transform.position - _activeBobber.transform.position;
			val.y = 0f;
			val.Normalize();
			Vector3 val2 = Vector3.Cross(val, Vector3.up);
			_fishRandomDir = ((Random.value < 0.5f) ? (-val2) : val2);
		}
	}

	private void CreateBobberProxy(Interacting_FISHING_BOBBER bobber)
	{
		_bobberProxy.MeshFilter.sharedMesh = bobber.Proxy.MeshFilter.sharedMesh;
		_bobberProxy.Renderer.sharedMaterial = ((Renderer)bobber.Proxy.MeshRenderer).sharedMaterial;
		((Component)_bobberProxy).gameObject.SetActive(true);
	}

	private float Spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	private Vector3 SpringVector(Vector3 from, Vector3 to, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
	}

	static Interacting_FISHINGROD()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
	}
}
