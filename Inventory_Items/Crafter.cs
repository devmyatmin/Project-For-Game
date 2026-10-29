using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Crafter : MonoBehaviour, INeedPlayerComponent, ISaveable
{
	private class ReplicateSetActive : I_MultiplayerMessage
	{
		[I_Replicate]
		public SaveablePrefab SaveablePrefab;

		[Replicate]
		public bool Active;

		public override void OnPeer()
		{
			SaveablePrefab.gameObject.SetActive(Active);
		}
	}

	private class ReplicatePlacePickupable : I_MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[I_Replicate]
		public IPickupable Pickupable;

		public override void OnPeer()
		{
			Player.Crafter.PlacePickupable(Pickupable);
		}
	}

	private class ReplicatePickupAndHandle : I_MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[I_Replicate]
		public ICraftable Craftable;

		[Replicate]
		public bool Animate;

		public override void OnPeer()
		{
			Player.Crafter.PickupAndHandle(Craftable, Animate);
		}
	}

	private class ReplicateGetCraftingMaterials : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Vector3 Position;

		public override void OnPeer()
		{
			Player.transform.position = Position;
			Player.Crafter.GetCraftingMaterials();
		}
	}

	private class ReplicateGetCraftingCombinations : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Vector3 Position;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		public override void OnPeer()
		{
			Player.transform.position = Position;
			Player.User.Controller.Raycaster.SetCurrentObject(RaycasterCurrentObject as IBase);
			Player.Crafter.GetCraftingCombinations();
		}
	}

	private class ReplicateCraft : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public short CombinationIndex;

		[Replicate]
		public MiniGuid ReferenceId;

		public override void OnPeer()
		{
			CraftingCombination combination = Player.Crafter._craftingCombinations.Combinations[CombinationIndex];
			Player.Crafter.Craft(combination, ReferenceId);
		}
	}

	private class ReplicateFinishPlacing : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Vector3 RayOrigin;

		[Replicate]
		public Vector3 RayDirection;

		[Replicate]
		public float RayLength;

		[Replicate]
		public float YRotation;

		[Replicate]
		public Constructing SnappedConnectorConstructing;

		[Replicate]
		public byte SnappedConnectorLocalId;

		public ReplicateFinishPlacing()
		{
		}

		public ReplicateFinishPlacing(IPlayer player, BeamRay ray, float yRotation, ICraftable craftable)
		{
			Player = player;
			RayOrigin = ray.OriginPoint;
			RayDirection = ray.Direction;
			RayLength = ray.Length;
			YRotation = yRotation;
			Constructing constructing = craftable as Constructing;
			if (constructing != null && constructing.SnappedConnector != null)
			{
				SnappedConnectorConstructing = constructing.SnappedConnector.Constructing;
				SnappedConnectorLocalId = (byte)constructing.SnappedConnector.GetLocalId();
			}
		}

		public override void OnPeer()
		{
			Player.Crafter.FinishPlacingByRecalculation(new BeamRay(RayOrigin, RayDirection, RayLength), YRotation, SnappedConnectorConstructing?.GetConnectorById(SnappedConnectorLocalId));
		}
	}

	private class ReplicateCancelPlacing : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.Crafter.CancelPlacing();
		}
	}

	private const float MATERIAL_POLLING_INTERVAL_SECONDS = 3f;

	private const float ROTATION_SENSITIVITY = 3f;

	public const float MIN_DISTANCE = 0.7f;

	public static Color CRAFTING_COLOR;

	[SerializeField]
	private Color _craftingHighlightColor;

	[SerializeField]
	private Projector _craftingAreaProjector;

	[SerializeField]
	private CrafterState _state;

	private List<ICraftingMaterialProvider> _pilesForCrafting = new List<ICraftingMaterialProvider>();

	private List<IBase> _backupCraftedMaterials = new List<IBase>();

	[SerializeField]
	private AudioClip _buildConfirmationSound;

	[SerializeField]
	private AudioClip _buildCancellationSound;

	[SerializeField]
	private ParticleSystem _buildConfirmationParticles;

	[SerializeField]
	private Ghost _ghost;

	private IPlayer _player;

	private bool _executingDelayedPlaceAction;

	private bool _isPlacing;

	private bool _cancelConstruction;

	private ICraftable _placingCraftable;

	private IPickupable _currentCraftable;

	private Vector3 _placingCraftableFinalPos;

	private Quaternion _placingCraftableFinalRot;

	private const int CACHE_SIZE = 128;

	private Collider[] _colliderCache = new Collider[128];

	private BaseObject[] _resultsCache = new BaseObject[128];

	private Dictionary<CraftingType, IList<IBase>> _cachedMaterialsLookup = new Dictionary<CraftingType, IList<IBase>>();

	private bool _polling;

	private CraftingCombinations _craftingCombinations;

	private CraftingCombination _currentCombination;

	private const float CRAFTED_NOTIF_TIMEOUT_SECS = 5f;

	private float _lastTimeCrafted;

	private bool _firstNotify;

	public Color HighlightColor
	{
		get
		{
			return CRAFTING_COLOR;
		}
		set
		{
			CRAFTING_COLOR = value;
		}
	}

	public CrafterState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (!_state.Equals(value))
			{
				_state = value;
				ResetHighlightColor();
			}
		}
	}

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsPlacing => _isPlacing;

	public ICraftable PlacingCraftable => _placingCraftable;

	public IPickupable CurrentCraftable => _currentCraftable;

	public CraftingCombinations CraftingCombinations => _craftingCombinations;

	public CraftingInfo LastCraftingInfo { get; private set; }

	public event EmptyEventHandler StartedPlacing;

	public event EmptyEventHandler StoppedPlacing;

	public event Action<CraftingCombination> Crafted;

	public event Action<IDictionary<CraftingType, IList<IBase>>> MaterialsUpdated;

	private void Awake()
	{
		_craftingCombinations = CraftingCombinations.Create();
		SetHighlightColor(_craftingHighlightColor);
		_ghost.transform.SetParent(null);
		_ghost.transform.position = Vector3.zero;
		_ghost.transform.rotation = Quaternion.identity;
		_ghost.Disable();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Holder.PickedUp += Holder_PickedUp;
	}

	private void OnDestroy()
	{
		if (!_player.IsNullOrDestroyed() && _player.Holder != null)
		{
			_player.Holder.PickedUp -= Holder_PickedUp;
		}
		this.StartedPlacing = null;
		this.StoppedPlacing = null;
		this.Crafted = null;
		this.MaterialsUpdated = null;
		if (_ghost.IsValid())
		{
			BeamTeam.Destroy(_ghost.gameObject);
		}
	}

	public void BeginPollMaterials()
	{
		if (!_polling)
		{
			InvokeRepeating("PollMaterials_InvokeRepeating", 3f, 3f);
			_polling = true;
		}
	}

	public void EndPollMaterials()
	{
		CancelInvoke("PollMaterials_InvokeRepeating");
		_polling = false;
	}

	private void PollMaterials_InvokeRepeating()
	{
		if (!_isPlacing)
		{
			PollMaterials();
		}
	}

	public void PollMaterials()
	{
		GetCraftingMaterials();
		this.MaterialsUpdated?.Invoke(_cachedMaterialsLookup);
	}

	private void Holder_PickedUp(IPickupable pickupable)
	{
		PollMaterials_InvokeRepeating();
	}

	private IDictionary<CraftingType, IList<IBase>> GetPlayerCraftingMaterials()
	{
		foreach (StorageSlot<IPickupable> slot in _player.Inventory.GetSlotStorage().GetSlots(StorageSlot<IPickupable>.QuantityComparison))
		{
			foreach (IPickupable @object in slot.Objects)
			{
				AddMaterial(@object);
			}
		}
		IPickupable currentObject = _player.Holder.CurrentObject;
		if (!currentObject.IsNullOrDestroyed())
		{
			AddMaterial(currentObject);
		}
		return _cachedMaterialsLookup;
	}

	private IDictionary<CraftingType, IList<IBase>> GetProximityCraftingMaterials()
	{
		int componentsInSphere_MinAlloc = BeamPhysics.GetComponentsInSphere_MinAlloc(_player.transform.position, 5f, (1 << Layers.INTERACTIVE_OBJECTS) | (1 << Layers.CONSTRUCTIONS_SMALL) | (1 << Layers.PROJECTILES), _colliderCache, _resultsCache);
		if (componentsInSphere_MinAlloc > 0)
		{
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			for (int i = 0; i < componentsInSphere_MinAlloc; i++)
			{
				BaseObject baseObject = _resultsCache[i];
				Vector3 rhs = baseObject.transform.position - _player.transform.position;
				rhs.y = 0f;
				rhs.Normalize();
				if (Vector3.Dot(forward, rhs) < 0.6f)
				{
					continue;
				}
				if (baseObject.CraftingType.InteractiveType == InteractiveType.CRAFTING_ITEM_PILE)
				{
					ICraftingMaterialProvider craftingMaterialProvider = baseObject as ICraftingMaterialProvider;
					List<IBase> list = new List<IBase>();
					foreach (IPickupable craftingMaterial in craftingMaterialProvider.GetCraftingMaterials())
					{
						list.Add(craftingMaterial);
						InsertMaterial(0, craftingMaterial);
					}
					_pilesForCrafting.Add(craftingMaterialProvider);
				}
				if (baseObject.CraftingType.InteractiveType == InteractiveType.CRAFTING_LOG && !(baseObject as InteractiveObject_PALM_LOG).CanPickUp)
				{
					continue;
				}
				if (baseObject.CraftingType.InteractiveType == InteractiveType.CRAFTING_FIRE_PIT)
				{
					Construction_PIT construction_PIT = baseObject as Construction_PIT;
					bool flag = false;
					for (int j = 0; j < construction_PIT.MyConnectors.Length; j++)
					{
						if (!construction_PIT.MyConnectors[j].IsValid)
						{
							flag = true;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (baseObject.CraftingType.InteractiveType == InteractiveType.CRAFTING_FIRE)
				{
					Construction_CAMPFIRE construction_CAMPFIRE = baseObject as Construction_CAMPFIRE;
					bool flag2 = false;
					for (int k = 0; k < construction_CAMPFIRE.MyConnectors.Length; k++)
					{
						if (!construction_CAMPFIRE.MyConnectors[k].IsValid)
						{
							flag2 = true;
						}
					}
					if (flag2)
					{
						continue;
					}
				}
				IAttachable attachable = baseObject as IAttachable;
				if (attachable.IsNullOrDestroyed() || !attachable.IsAttached)
				{
					AddMaterial(baseObject);
				}
			}
		}
		for (int l = 0; l < 128; l++)
		{
			_colliderCache[l] = null;
			_resultsCache[l] = null;
		}
		return _cachedMaterialsLookup;
	}

	private void AddMaterial(IBase obj)
	{
		CraftingType craftingType = obj.CraftingType;
		if (_cachedMaterialsLookup.ContainsKey(craftingType))
		{
			_cachedMaterialsLookup[craftingType].Add(obj);
			return;
		}
		_cachedMaterialsLookup.Add(craftingType, new List<IBase> { obj });
	}

	private void InsertMaterial(int index, IBase obj)
	{
		CraftingType craftingType = obj.CraftingType;
		if (_cachedMaterialsLookup.ContainsKey(craftingType))
		{
			_cachedMaterialsLookup[craftingType].Insert(index, obj);
			return;
		}
		_cachedMaterialsLookup.Add(craftingType, new List<IBase> { obj });
	}

	private bool RemoveMaterial(IBase obj)
	{
		CraftingType craftingType = obj.CraftingType;
		if (_cachedMaterialsLookup.ContainsKey(craftingType))
		{
			return _cachedMaterialsLookup[craftingType].Remove(obj);
		}
		return false;
	}

	public IDictionary<CraftingType, IList<IBase>> GetCraftingMaterials()
	{
		_cachedMaterialsLookup.Clear();
		_pilesForCrafting.Clear();
		GetProximityCraftingMaterials();
		GetPlayerCraftingMaterials();
		return _cachedMaterialsLookup;
	}

	private CraftingInfo GetCraftingInfo()
	{
		CraftingInfo craftingInfo = new CraftingInfo();
		craftingInfo.CurrentCraftsmanshipLevel = _player.PlayerSkills.CraftsmanshipLevel;
		if (_cachedMaterialsLookup.Count == 0)
		{
			return craftingInfo;
		}
		List<CraftingType> list = new List<CraftingType>(_cachedMaterialsLookup.Values.Count);
		foreach (IList<IBase> value in _cachedMaterialsLookup.Values)
		{
			foreach (IBase item in value)
			{
				list.Add(item.CraftingType);
			}
		}
		craftingInfo.CurrentMaterials = list;
		IBase currentObject = _player.GameInput.Raycaster.CurrentObject;
		if (currentObject.IsNullOrDestroyed() || currentObject.CraftingType == CraftingType.Empty)
		{
			currentObject = _player.Holder.CurrentObject;
		}
		SaveablePrefab saveablePrefab = currentObject as SaveablePrefab;
		if (saveablePrefab == null)
		{
			return craftingInfo;
		}
		craftingInfo.CurrentPrefabId = saveablePrefab.PrefabId;
		craftingInfo.CurrentType = currentObject.CraftingType;
		return craftingInfo;
	}

	public IList<CraftingCombination> GetCraftingCombinations()
	{
		GetCraftingMaterials();
		LastCraftingInfo = GetCraftingInfo();
		if (_cachedMaterialsLookup.Count == 0)
		{
			return null;
		}
		return (from category in _craftingCombinations.Categories
			where !(category.ToolRequired != new CraftingType(AttributeType.None, InteractiveType.None)) || _player.Holder.CurrentObject?.CraftingType == category.ToolRequired
			orderby category.ToolRequired == _player.Holder.CurrentObject?.CraftingType descending
			select category).SelectMany((CraftingCategory category) => category.Subcategories).SelectMany((CraftingSubcategory subcategory) => subcategory.Combinations.Where((CraftingCombination combination) => combination.IsValid(LastCraftingInfo)).ToList()).ToList();
	}

	private float GetCraftingDelay()
	{
		float result = 0f;
		if (_player.Id == 1)
		{
			result = MultiplayerMng.GetRoundTrip();
		}
		return result;
	}

	private void DisableCraftingInput(bool enable)
	{
		_player.Input.SetAndCacheMapsEnabled(!enable, 0);
		_player.Input.SetAndCacheMapsEnabled(enable, 15);
		_player.GameInput.enabled = !enable;
	}

	public IEnumerator PlaceCraftable_Sequence(ICraftable craftable)
	{
		_placingCraftable = craftable;
		if (_player.IsOwner)
		{
			this.StartedPlacing?.Invoke();
			DisableCraftingInput(enable: true);
		}
		_ghost.Enable();
		if (!_ghost.Initialize(craftable))
		{
			CancelCrafting(craftable);
			yield break;
		}
		ReplicatedSetActive(craftable, active: false);
		Constructing constructing = craftable as Constructing;
		MiniGuid replicantSnappedConnectorConstructingReferenceId = default(MiniGuid);
		int replicantSnappedConnectorLocalId = 0;
		Connector replicantSnappedConnector = null;
		float yRotation = 0f;
		_cancelConstruction = false;
		_isPlacing = true;
		while (_isPlacing && !_executingDelayedPlaceAction)
		{
			BeamRay orgRay;
			if (_player.IsOwner)
			{
				orgRay = _player.PlayerCamera.GetRay(craftable.PlacingDistance);
				_player.MultiplayerState.CrafterRayOrigin = orgRay.OriginPoint;
				_player.MultiplayerState.CrafterRayDirection = orgRay.Direction;
				_player.MultiplayerState.CrafterRayLength = orgRay.Length;
			}
			else
			{
				orgRay = new BeamRay(_player.MultiplayerState.CrafterRayOrigin, _player.MultiplayerState.CrafterRayDirection, _player.MultiplayerState.CrafterRayLength);
			}
			BeamRay ray = orgRay;
			if (Physics.Raycast(ray.OriginPoint, ray.Direction, out var hitInfo, ray.Length, 1 << Layers.TERRAIN, QueryTriggerInteraction.Ignore))
			{
				float magnitude = (ray.OriginPoint - hitInfo.point).magnitude;
				ray.Length = Mathf.Min(magnitude, ray.Length);
			}
			Vector3 endPoint = ray.EndPoint;
			Quaternion rotation = Quaternion.LookRotation(ray.Direction.Mask(Vector3Tools.xz).normalized);
			if (_player.IsOwner)
			{
				yRotation += _player.Input.GetAxis(16) * 3f;
				if (yRotation < 0f)
				{
					yRotation += 360f;
				}
				if (yRotation > 360f)
				{
					yRotation -= 360f;
				}
				_player.MultiplayerState.CrafterYRotation = yRotation;
			}
			else
			{
				yRotation = _player.MultiplayerState.CrafterYRotation;
			}
			_ghost.transform.position = endPoint;
			_ghost.transform.rotation = rotation;
			float num = Mathf.Min(ray.Length, 0.7f);
			ray.OriginPoint += ray.Direction * num;
			ray.Length -= num;
			bool flag;
			if (_player.IsOwner)
			{
				flag = craftable.OwnerPlacing(_ghost.transform, ray, yRotation);
				if (constructing != null)
				{
					if (constructing.IsSnapped)
					{
						MiniGuid referenceId = constructing.SnappedConnector.Constructing.ReferenceId;
						_player.MultiplayerState.CrafterSnappedConnectorConstructing[0] = referenceId.ToIntLow();
						_player.MultiplayerState.CrafterSnappedConnectorConstructing[1] = referenceId.ToIntHigh();
						_player.MultiplayerState.CrafterSnappedConnectorLocalId = constructing.SnappedConnector.GetLocalId();
					}
					else
					{
						_player.MultiplayerState.CrafterSnappedConnectorConstructing[0] = 0;
						_player.MultiplayerState.CrafterSnappedConnectorConstructing[1] = 0;
						_player.MultiplayerState.CrafterSnappedConnectorLocalId = 0;
					}
				}
			}
			else
			{
				if (constructing != null)
				{
					MiniGuid miniGuid = new MiniGuid(_player.MultiplayerState.CrafterSnappedConnectorConstructing[0], _player.MultiplayerState.CrafterSnappedConnectorConstructing[1]);
					int crafterSnappedConnectorLocalId = _player.MultiplayerState.CrafterSnappedConnectorLocalId;
					if (!replicantSnappedConnectorConstructingReferenceId.Equals(miniGuid) || replicantSnappedConnectorLocalId != crafterSnappedConnectorLocalId)
					{
						replicantSnappedConnector = MultiplayerMng.FindObject<Constructing>(miniGuid)?.GetConnectorById(crafterSnappedConnectorLocalId);
						replicantSnappedConnectorConstructingReferenceId = miniGuid;
						replicantSnappedConnectorLocalId = crafterSnappedConnectorLocalId;
					}
					constructing.SetSnapped(replicantSnappedConnector);
				}
				flag = craftable.ReplicantPlacing(_ghost.transform, ray, yRotation % 360f);
			}
			flag &= Mathf.Approximately(num, 0.7f);
			flag &= !_ghost.Colliding;
			bool flag2 = craftable.CheckProximity(_ghost.transform);
			flag = flag && !flag2;
			if (craftable.gameObject.TryGetComponent<PlacementModifier_SnapToTerrain>(out var component) && !component.Snapped)
			{
				flag = false;
			}
			if (flag && Physics.Raycast(_ghost.transform.position + Vector3.up * 5f, Vector3.down, out var hitInfo2, 15f, 1 << Layers.TERRAIN, QueryTriggerInteraction.Ignore) && hitInfo2.collider.CompareTag("NoBuilding") && craftable is IConstructable)
			{
				flag = false;
			}
			if (!AreAllMaterialsAvailable())
			{
				flag = false;
			}
			Color highlightColor = (flag ? _craftingHighlightColor : Color.red);
			SetHighlightColor(highlightColor);
			if (_player.IsOwner)
			{
				if (_player.Input.GetButtonDown(81))
				{
					if (flag2)
					{
						LocalizedNotification.Post(_player, NotificationPriority.Immediate, 4f, "CONSTRUCTING_NTF_CANT_BUILD_TOO_CLOSE_DESC");
						AudioManager.GetAudioPlayer().Play2D(_buildCancellationSound, AudioMixerChannels.UI);
					}
					else if (_ghost.Colliding)
					{
						LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
						localizedNotification.Priority = NotificationPriority.Immediate;
						localizedNotification.Duration = 4f;
						localizedNotification.PlayerId = _player.Id;
						localizedNotification.MessageText.SetTerm("CONSTRUCTING_NTF_CANT_BUILD_IN_WAY_DESC");
						localizedNotification.Raise();
						AudioManager.GetAudioPlayer().Play2D(_buildCancellationSound, AudioMixerChannels.UI);
					}
					else if (!flag)
					{
						AudioManager.GetAudioPlayer().Play2D(_buildCancellationSound, AudioMixerChannels.UI);
					}
					else
					{
						if (!_executingDelayedPlaceAction)
						{
							BeamTiming.WaitAnd(GetCraftingDelay(), delegate
							{
								_executingDelayedPlaceAction = false;
								if (!_ghost.Colliding && (AreAllMaterialsAvailable() || IsCraftingFree()))
								{
									ReplicatedFinishPlacing(_ghost.transform.position, _ghost.transform.rotation, orgRay, yRotation, craftable);
									_placingCraftableFinalPos = _ghost.transform.position;
									_placingCraftableFinalRot = _ghost.transform.rotation;
									_isPlacing = false;
								}
								else
								{
									CleanupCachedMaterials();
								}
							});
						}
						_executingDelayedPlaceAction = true;
					}
				}
				else if (_player.Input.GetButtonDown(82))
				{
					ReplicatedCancelPlacing();
				}
			}
			yield return null;
		}
		while (_executingDelayedPlaceAction)
		{
			yield return null;
		}
		if (craftable != null)
		{
			if (_cancelConstruction)
			{
				CancelCrafting(craftable);
			}
			else
			{
				craftable.transform.position = _placingCraftableFinalPos;
				craftable.transform.rotation = _placingCraftableFinalRot;
				ConfirmConstruction(craftable.transform.position);
				if (Game.Mode.IsMaster())
				{
					ReplicatedSetActive(craftable, active: true, MultiplayerMng.GetRoundTrip());
					craftable.Place();
					DoRefills(craftable);
				}
				else
				{
					bool num2 = (craftable as ISaveablePrefab)?.IsMultiplayerEntity ?? false;
					bool flag3 = num2 && ((ISaveablePrefab)craftable).IsOwner;
					bool flag4 = constructing?.IsStructurePrefabAMultiplayerEntity() ?? false;
					bool flag5 = constructing?.SnappedConnector == null;
					if ((num2 && flag3) || (flag5 && flag4))
					{
						DoRefills(craftable);
						MultiplayerMng.Destroy((SaveablePrefab)craftable, DestroyDelay.NOW);
					}
					else
					{
						craftable.gameObject.SetActive(value: true);
						craftable.Place();
					}
				}
				FinishCrafting(craftable);
			}
		}
		_ghost.Disable();
		if (_player.IsOwner)
		{
			this.StoppedPlacing?.Invoke();
			DisableCraftingInput(enable: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
		}
	}

	private void DoRefills(ICraftable craftable)
	{
		float units = GetRefills(craftable);
		if (units == 0f)
		{
			return;
		}
		SaveablePrefab saveablePrefab = (SaveablePrefab)craftable;
		if (Game.Mode.IsMaster())
		{
			ForceRefills(saveablePrefab, units);
			return;
		}
		MiniGuid referenceId = saveablePrefab.ReferenceId;
		StartCoroutine(WaitForExistence(referenceId, delegate(SaveablePrefab x)
		{
			ForceRefills(x, units);
		}));
	}

	private IEnumerator<float> WaitForExistence(MiniGuid referenceId, Action<SaveablePrefab> callback)
	{
		Stopwatch timer = Stopwatch.StartNew();
		SaveablePrefab instance = null;
		bool found = false;
		while (!found && !((float)timer.ElapsedMilliseconds > Peer.DEFAULT_TIMEOUT * 1000f))
		{
			instance = MultiplayerMng.FindObject<SaveablePrefab>(referenceId);
			found = instance;
			if (!found)
			{
				yield return 0f;
			}
		}
		timer.Stop();
		if (found)
		{
			callback?.Invoke(instance);
		}
	}

	private bool IsCraftingFree()
	{
		return false;
	}

	private bool IsPileMaterialAvailable(ICraftingMaterial material)
	{
		int num = 0;
		foreach (IBase backupCraftedMaterial in _backupCraftedMaterials)
		{
			IPickupable obj = backupCraftedMaterial as IPickupable;
			if (obj != null && obj.IsDummy() && backupCraftedMaterial.CraftingType == material.CraftingType)
			{
				num++;
			}
		}
		int num2 = 0;
		foreach (ICraftingMaterialProvider item in _pilesForCrafting)
		{
			if (item.MaterialsType != material.CraftingType)
			{
				continue;
			}
			foreach (IPickupable craftingMaterial in item.GetCraftingMaterials())
			{
				_ = craftingMaterial;
				num2++;
			}
		}
		return num2 >= num;
	}

	private bool AreAllMaterialsAvailable()
	{
		bool result = true;
		if (_currentCombination != null)
		{
			List<CraftingType> list = _currentCombination.Materials.Skip(_currentCombination.IsExtension ? 1 : 0).ToList();
			foreach (IBase backupCraftedMaterial in _backupCraftedMaterials)
			{
				list.Remove(backupCraftedMaterial.CraftingType);
			}
			if (list.Count > 0)
			{
				result = false;
			}
		}
		else
		{
			result = false;
		}
		foreach (IBase backupCraftedMaterial2 in _backupCraftedMaterials)
		{
			bool flag = false;
			InteractiveObject interactiveObject = backupCraftedMaterial2 as InteractiveObject;
			if (interactiveObject != null)
			{
				bool flag2 = interactiveObject.IsPickedUp;
				int obj = interactiveObject.OwnerId;
				foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
				{
					if (allPlayer.Holder.IsPickingUp(interactiveObject))
					{
						flag2 = true;
						obj = allPlayer.Id;
						break;
					}
				}
				bool flag3 = _player.Id.Equals(obj);
				bool flag4 = flag2 && !flag3;
				flag = flag2 && flag3;
				if (flag4)
				{
					result = false;
					break;
				}
			}
			if (backupCraftedMaterial2 is IPickupable pickupable && pickupable.IsDummy())
			{
				if (backupCraftedMaterial2.IsNullOrDestroyed() || !IsPileMaterialAvailable(pickupable))
				{
					result = false;
					break;
				}
			}
			else if (backupCraftedMaterial2.IsNullOrDestroyed() || !backupCraftedMaterial2.gameObject || (!flag && !backupCraftedMaterial2.gameObject.activeInHierarchy))
			{
				result = false;
				break;
			}
			if (_player.Id == 1)
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(0);
				if ((!IsPlacing || playerFromId.Crafter.IsPlacing) && playerFromId.Crafter._backupCraftedMaterials.Contains(backupCraftedMaterial2))
				{
					result = false;
				}
			}
			else if (_player.Id == 0 && PlayerRegistry.AllPlayers.Count > 1)
			{
				IPlayer playerFromId2 = PlayerUtilities.GetPlayerFromId(1);
				if (!IsPlacing && playerFromId2.Crafter.IsPlacing && playerFromId2.Crafter._backupCraftedMaterials.Contains(backupCraftedMaterial2))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public bool CraftingWithMaterial(IPickupable pickupable)
	{
		foreach (IBase backupCraftedMaterial in _backupCraftedMaterials)
		{
			if (backupCraftedMaterial == pickupable)
			{
				return true;
			}
		}
		return false;
	}

	private void FinishPlacing(Vector3 pos, Quaternion rot, BeamRay ray, float yRotation, ICraftable craftable)
	{
		_isPlacing = false;
		_cancelConstruction = false;
		_placingCraftableFinalPos = pos;
		_placingCraftableFinalRot = rot;
	}

	private void FinishPlacingByRecalculation(BeamRay orgRay, float yRotation, Connector snppedConnector)
	{
		ICraftable placingCraftable = _placingCraftable;
		Constructing constructing = placingCraftable as Constructing;
		BeamRay ray = orgRay;
		if (Physics.Raycast(ray.OriginPoint, ray.Direction, out var hitInfo, ray.Length, 1 << Layers.TERRAIN, QueryTriggerInteraction.Ignore))
		{
			float magnitude = (ray.OriginPoint - hitInfo.point).magnitude;
			ray.Length = Mathf.Min(magnitude, ray.Length);
		}
		Vector3 endPoint = ray.EndPoint;
		Quaternion rotation = Quaternion.LookRotation(ray.Direction.Mask(Vector3Tools.xz).normalized);
		_ghost.transform.position = endPoint;
		_ghost.transform.rotation = rotation;
		float num = Mathf.Min(ray.Length, 0.7f);
		ray.OriginPoint += ray.Direction * num;
		ray.Length -= num;
		if (snppedConnector != null)
		{
			constructing.SetSnapped(snppedConnector);
		}
		placingCraftable.ReplicantPlacing(_ghost.transform, ray, yRotation % 360f);
		_placingCraftableFinalPos = _ghost.transform.position;
		_placingCraftableFinalRot = _ghost.transform.rotation;
		_isPlacing = false;
		_cancelConstruction = false;
	}

	private void CleanupCachedMaterials()
	{
		_backupCraftedMaterials.Clear();
		_pilesForCrafting.Clear();
		_placingCraftable = null;
		_currentCraftable = null;
		_currentCombination = null;
	}

	public void CancelPlacing()
	{
		CleanupCachedMaterials();
		_isPlacing = false;
		_cancelConstruction = true;
	}

	public void Craft(CraftingCombination combination, MiniGuid referenceId)
	{
		_currentCombination = combination;
		_backupCraftedMaterials.Clear();
		foreach (CraftingType item in _currentCombination.Materials.Skip(_currentCombination.IsExtension ? 1 : 0))
		{
			if (_cachedMaterialsLookup.TryGetValue(item, out var value))
			{
				IBase @base = value.FirstOrDefault_NonAlloc();
				if (!@base.IsNullOrDestroyed())
				{
					_backupCraftedMaterials.Add(@base);
					value.Remove(@base);
				}
			}
		}
		if (_executingDelayedPlaceAction)
		{
			return;
		}
		if (_player.IsOwner)
		{
			DisableCraftingInput(enable: true);
		}
		BeamTiming.WaitAnd(GetCraftingDelay(), delegate
		{
			if (_player.IsOwner)
			{
				DisableCraftingInput(enable: false);
			}
			_executingDelayedPlaceAction = false;
			if (AreAllMaterialsAvailable() || IsCraftingFree())
			{
				BaseObject obj = MultiplayerMng.Instantiate<BaseObject>(_currentCombination.PrefabAssetPath, referenceId);
				Craft(obj);
			}
			else
			{
				CleanupCachedMaterials();
			}
		});
		_executingDelayedPlaceAction = true;
	}

	private void Craft(SaveablePrefab obj)
	{
		ICraftable craftable = obj as ICraftable;
		if (!craftable.IsNullOrDestroyed())
		{
			craftable.CreatorId = _player.Id;
			craftable.OnBeginCrafting();
			if (craftable.CraftImmediate)
			{
				FinishCrafting(craftable);
				return;
			}
			IPickupable pickupable = (_currentCraftable = obj as IPickupable);
			if (!pickupable.IsNullOrDestroyed() && pickupable.CanPickUp)
			{
				if (Game.Mode.IsMaster())
				{
					if (_player.Holder.CanPickup(pickupable))
					{
						ReplicatedPickupAndHandle(craftable, animate: false);
					}
					else
					{
						ReplicatedPlacePickupable(pickupable);
					}
				}
				else
				{
					MultiplayerMng.Destroy((SaveablePrefab)craftable, DestroyDelay.NOW);
				}
			}
			else
			{
				StartCoroutine(PlaceCraftable_Sequence(craftable));
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Crafter::Construct:: No [Constructing] component found on [" + obj.name + "]");
			BeamTeam.Destroy(obj.gameObject);
		}
	}

	public void PlacePickupable(IPickupable pickupable)
	{
		LocalizedNotification.Post(_player, NotificationPriority.Normal, 4f, "CRAFTING_MENU_NTF_ITEM_NOT_STORED_BACKPACK_DESC", pickupable.DisplayName);
		StartCoroutine(PlaceCraftable_Sequence((ICraftable)pickupable));
	}

	private void PickupAndHandle(ICraftable craftable, bool animate)
	{
		IPickupable pickupable = craftable as IPickupable;
		if (pickupable.IsNullOrDestroyed())
		{
			throw new Exception();
		}
		if (_player.Holder.Pickup(pickupable, animate: false))
		{
			bool flag = _firstNotify || Time.time - _lastTimeCrafted > 5f;
			if (_player.Inventory.GetSlotStorage().Has(pickupable) && flag)
			{
				LocalizedNotification.Post(_player, NotificationPriority.Normal, 4f, "CRAFTING_MENU_NTF_ITEM_STORED_BACKPACK_DESC", pickupable.DisplayName);
				_firstNotify = false;
			}
			FinishCrafting(craftable);
		}
		else
		{
			MultiplayerMng.LogError("PickupAndHandle " + pickupable.GetType().Name + " should be used when the action is doable");
		}
	}

	private float GetRefills(ICraftable craftable)
	{
		float result = 0f;
		if (craftable is Constructing_FUEL_STILL_BOILER)
		{
			for (int i = 0; i < _backupCraftedMaterials.Count; i++)
			{
				if (_backupCraftedMaterials[i] is InteractiveObject_FUELCAN interactiveObject_FUELCAN)
				{
					result = interactiveObject_FUELCAN.Fuel;
					break;
				}
			}
		}
		else if (craftable is Constructing_STILL)
		{
			for (int j = 0; j < _backupCraftedMaterials.Count; j++)
			{
				if (_backupCraftedMaterials[j] is ConstructionObject_WATERBOTTLE constructionObject_WATERBOTTLE)
				{
					result = constructionObject_WATERBOTTLE.Servings;
					break;
				}
			}
		}
		return result;
	}

	private static void ForceRefills(SaveablePrefab instance, float units)
	{
		if (instance is Constructing_FUEL_STILL_BOILER constructing_FUEL_STILL_BOILER)
		{
			constructing_FUEL_STILL_BOILER.ForceRefuel(units);
		}
		else if (instance is Constructing_STILL constructing_STILL)
		{
			constructing_STILL.ForceRefill((int)units);
		}
	}

	private void FinishCrafting(ICraftable craftable)
	{
		if (_currentCombination != null)
		{
			craftable.OnCrafted(_player);
			EventManager.RaiseEvent(new ExperienceEvent(_player.Id, PlayerSkills.Skills.CRAFTSMANSHIP, _currentCombination.ExpRewardOnCraft));
			EventManager.RaiseEvent(new AddStatsEvent(_player.Id, new Stats
			{
				ItemsCrafted = 1
			}));
			_currentCombination.CraftedCount++;
			this.Crafted?.Invoke(_currentCombination);
			DestroyBackupMaterials();
			_cachedMaterialsLookup.Clear();
			CleanupCachedMaterials();
			_lastTimeCrafted = Time.time;
		}
	}

	public void CancelCrafting(ICraftable craftable)
	{
		MultiplayerMng.Destroy((SaveablePrefab)craftable, DestroyDelay.NOW);
		ResetHighlightColor();
		CleanupCachedMaterials();
		AudioManager.GetAudioPlayer().Play2D(_buildCancellationSound, AudioMixerChannels.UI);
	}

	public void SetHighlightColor(Color color)
	{
		HighlightColor = color;
		_ghost.SetHighlightColor(HighlightColor.linear);
	}

	public void ResetHighlightColor()
	{
		SetHighlightColor(_craftingHighlightColor);
	}

	public void EnableProjector(bool enable)
	{
		_craftingAreaProjector.enabled = enable;
	}

	private void ConfirmConstruction(Vector3 position)
	{
		AudioManager.GetAudioPlayer().Play3D(_buildConfirmationSound, position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
		if ((bool)_buildConfirmationParticles)
		{
			_buildConfirmationParticles.transform.position = position;
			_buildConfirmationParticles.Play();
		}
	}

	private void DestroyBackupMaterials()
	{
		foreach (IBase backupCraftedMaterial in _backupCraftedMaterials)
		{
			ICraftingMaterial craftingMaterial = backupCraftedMaterial as ICraftingMaterial;
			if (!craftingMaterial.IsNullOrDestroyed())
			{
				craftingMaterial.Consume();
			}
		}
	}

	private void ReplicatedSetActive(ICraftable craftable, bool active, float delay = 0f)
	{
		SaveablePrefab saveablePrefab = craftable as SaveablePrefab;
		if (Game.Mode.IsMultiplayer() && !saveablePrefab.IsNullOrDestroyed() && saveablePrefab.IsMultiplayerEntity)
		{
			if (delay != 0f)
			{
				BeamTiming.WaitAnd(delay, delegate
				{
					ReplicateSetActive replicateSetActive2 = new ReplicateSetActive();
					replicateSetActive2.SaveablePrefab = saveablePrefab;
					replicateSetActive2.Active = active;
					replicateSetActive2.Post();
				});
			}
			else
			{
				ReplicateSetActive replicateSetActive = new ReplicateSetActive();
				replicateSetActive.SaveablePrefab = saveablePrefab;
				replicateSetActive.Active = active;
				replicateSetActive.Post();
			}
		}
		craftable.gameObject.SetActive(active);
	}

	private void ReplicatedPlacePickupable(IPickupable pickupable)
	{
		if (Game.Mode.IsServer())
		{
			ReplicatePlacePickupable replicatePlacePickupable = new ReplicatePlacePickupable();
			replicatePlacePickupable.Player = _player;
			replicatePlacePickupable.Pickupable = pickupable;
			replicatePlacePickupable.Post();
		}
		PlacePickupable(pickupable);
	}

	public void ReplicatedPickupAndHandle(ICraftable craftable, bool animate)
	{
		if (Game.Mode.IsServer())
		{
			ReplicatePickupAndHandle replicatePickupAndHandle = new ReplicatePickupAndHandle();
			replicatePickupAndHandle.Player = _player;
			replicatePickupAndHandle.Craftable = craftable;
			replicatePickupAndHandle.Animate = animate;
			replicatePickupAndHandle.Post();
		}
		PickupAndHandle(craftable, animate);
	}

	public IList<CraftingCombination> ReplicatedGetCraftingCombinations()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateGetCraftingCombinations replicateGetCraftingCombinations = new ReplicateGetCraftingCombinations();
			replicateGetCraftingCombinations.Player = _player;
			replicateGetCraftingCombinations.Position = _player.transform.position;
			replicateGetCraftingCombinations.RaycasterCurrentObject = _player.User.Controller.Raycaster.CurrentObject as ISaveablePrefab;
			replicateGetCraftingCombinations.Post();
		}
		return GetCraftingCombinations();
	}

	public IDictionary<CraftingType, IList<IBase>> ReplicatedGetCraftingMaterials()
	{
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateGetCraftingMaterials replicateGetCraftingMaterials = new ReplicateGetCraftingMaterials();
			replicateGetCraftingMaterials.Player = _player;
			replicateGetCraftingMaterials.Position = _player.transform.position;
			replicateGetCraftingMaterials.Post();
		}
		return GetCraftingMaterials();
	}

	public void ReplicatedCraft(CraftingCombination combination)
	{
		MiniGuid referenceId = MiniGuid.New();
		if (Game.Mode.IsMultiplayer() && _player.IsOwner)
		{
			ReplicateCraft replicateCraft = new ReplicateCraft();
			replicateCraft.Player = _player;
			replicateCraft.CombinationIndex = combination.SelfIndex;
			replicateCraft.ReferenceId = referenceId;
			replicateCraft.Post();
		}
		if (!_executingDelayedPlaceAction)
		{
			if (_player.IsOwner)
			{
				DisableCraftingInput(enable: true);
			}
			BeamTiming.WaitAnd(GetCraftingDelay(), delegate
			{
				if (_player.IsOwner)
				{
					DisableCraftingInput(enable: false);
				}
				_executingDelayedPlaceAction = false;
				Craft(combination, referenceId);
			});
		}
		_executingDelayedPlaceAction = true;
	}

	private void ReplicatedFinishPlacing(Vector3 pos, Quaternion rot, BeamRay ray, float yRotation, ICraftable craftable)
	{
		if (Game.Mode.IsMultiplayer() && _player.IsOwner)
		{
			new ReplicateFinishPlacing(_player, ray, yRotation, craftable).Post();
		}
		FinishPlacing(pos, rot, ray, yRotation, craftable);
	}

	private void ReplicatedCancelPlacing()
	{
		if (Game.Mode.IsMultiplayer() && _player.IsOwner)
		{
			ReplicateCancelPlacing replicateCancelPlacing = new ReplicateCancelPlacing();
			replicateCancelPlacing.Player = _player;
			replicateCancelPlacing.Post();
		}
		CancelPlacing();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject jObject2 = SavingUtilities.Save(_craftingCombinations);
		if (jObject2 != null)
		{
			jObject.AddField("Combinations", jObject2);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Combinations");
		SavingUtilities.Load(_craftingCombinations, field);
	}
}
