using Beam.Crafting;
using Beam.Language;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class Interactive_FISHES : MonoBehaviour, IBase, IHasCraftingType, ISelectable, ISpearable
{
	private class ReplicateKillFishAndAttachProjectiles : I_MultiplayerMessage
	{
		[I_Replicate]
		public Interactive_FISH Fish;

		[Replicate]
		public byte PlayerId;

		[Replicate]
		public ProjectileObject[] Projectiles;

		[Replicate]
		public Vector3[] ProjectilePositions;

		[Replicate]
		public Quaternion[] ProjectileRotations;

		public override void OnPeer()
		{
			KillFish(Fish, PlayerId);
			Projectiles.AttachTo(Fish);
			Projectiles.SetPositions(ProjectilePositions, local: true);
			Projectiles.SetRotation(ProjectileRotations, local: true);
		}
	}

	private class ReplicateCreateKilledFishOnMaster : MultiplayerMessage
	{
		[Replicate]
		public uint PrefabId;

		[Replicate]
		public Vector3 Position;

		[Replicate]
		public Quaternion Rotation;

		[Replicate]
		public byte PlayerId;

		[Replicate]
		public ProjectileObject[] Projectiles;

		[Replicate]
		public Vector3[] ProjectilePositions;

		[Replicate]
		public Quaternion[] ProjectileRotations;

		public override void OnPeer()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			Projectiles.SetPositions(ProjectilePositions, local: false);
			Projectiles.SetRotation(ProjectileRotations, local: false);
			ReplicatedCreateKilledFishAndAttachProjectiles(PrefabId, Position, Rotation, PlayerId, Projectiles);
		}
	}

	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _displayName;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[SerializeField]
	private RaycastMode _raycastMode;

	[Tooltip("Health points used for spearing and shooting damage. 0=insta-kill")]
	[SerializeField]
	private float _healthPoints;

	private float _originalHealthPoints;

	[SerializeField]
	private FishBase _fishes;

	private int _lastPlayerIdToDamageThis = -1;

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType { get; } = new CraftingType(AttributeType.None, InteractiveType.ANIMALS_FISH);

	public bool CanParent { get; set; }

	public bool IsDamageable => _healthPoints > 0f;

	public string DisplayName => _displayName;

	public Selecting Selecting => _selecting;

	public RaycastMode RaycastMode => _raycastMode;

	InteractiveType ISpearable.Type => CraftingType.InteractiveType;

	bool ISpearable.CanSpear => _fishes.FishingData.FishType != FishSize.ExtraLarge;

	bool ISpearable.IsLocalFauna => true;

	uint ISpearable.LocalFaunaSpearedInstancePrefabId => _fishes.FishingData.FishPrefab.PrefabId;

	bool ISpearable.LocalFaunaCheckHit(Vector3 postion)
	{
		if (true)
		{
			return true;
		}
		return false;
	}

	void ISpearable.LocalFaunaRelease()
	{
		_fishes.Catch();
	}

	ISpearedInstance ISpearable.GetSpearedInstance(Vector3 position)
	{
		return null;
	}

	private void Awake()
	{
		_fishes.Freed += Fish_Freed;
		_originalHealthPoints = _healthPoints;
	}

	private void Fish_Freed(ICellObject obj)
	{
		_healthPoints = _originalHealthPoints;
	}

	public void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			_lastPlayerIdToDamageThis = pickupable.PreviousOwnerId;
		}
		BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
		BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		_healthPoints -= damage;
		if (!(_healthPoints > 0f))
		{
			CreateKilledFishAndAttachProjectilesOnMaster();
			_fishes.Catch();
		}
	}

	public void Damage(DamageUnityEventArgs args)
	{
		Damage(args.Damage, args.Damager);
	}

	public void Destroy()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		if ((Object)(object)_fishes != (Object)null)
		{
			_fishes.Freed -= Fish_Freed;
		}
	}

	public void Parent()
	{
	}

	private void CreateKilledFishAndAttachProjectilesOnMaster()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicatedCreateKilledFishAndAttachProjectiles(_fishes.FishingData.FishPrefab.PrefabId, ((Component)this).transform.position, ((Component)this).transform.rotation, (byte)_lastPlayerIdToDamageThis, global::ExtensionMethods.GetProjectiles(((Component)this).transform));
			return;
		}
		ProjectileObject[] projectiles = global::ExtensionMethods.GetProjectiles(((Component)this).transform);
		ReplicateCreateKilledFishOnMaster replicateCreateKilledFishOnMaster = new ReplicateCreateKilledFishOnMaster();
		replicateCreateKilledFishOnMaster.PrefabId = _fishes.FishingData.FishPrefab.PrefabId;
		replicateCreateKilledFishOnMaster.Position = ((Component)this).transform.position;
		replicateCreateKilledFishOnMaster.Rotation = ((Component)this).transform.rotation;
		replicateCreateKilledFishOnMaster.PlayerId = (byte)_lastPlayerIdToDamageThis;
		replicateCreateKilledFishOnMaster.Projectiles = projectiles;
		replicateCreateKilledFishOnMaster.ProjectilePositions = projectiles.GetPositions(local: false);
		replicateCreateKilledFishOnMaster.ProjectileRotations = projectiles.GetRotations(local: false);
		replicateCreateKilledFishOnMaster.Post();
	}

	private static void KillFish(Interactive_FISH fish, byte playerId)
	{
		fish.Parent();
		fish.Kill(playerId);
	}

	private static void ReplicatedCreateKilledFishAndAttachProjectiles(uint prefabId, Vector3 position, Quaternion rotation, byte playerId, ProjectileObject[] projectiles)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Interactive_FISH interactive_FISH = MultiplayerMng.Instantiate<Interactive_FISH>(prefabId, position, rotation, MiniGuid.New());
		KillFish(interactive_FISH, playerId);
		projectiles.AttachTo(interactive_FISH);
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateKillFishAndAttachProjectiles replicateKillFishAndAttachProjectiles = new ReplicateKillFishAndAttachProjectiles();
			replicateKillFishAndAttachProjectiles.Fish = interactive_FISH;
			replicateKillFishAndAttachProjectiles.PlayerId = playerId;
			replicateKillFishAndAttachProjectiles.Projectiles = projectiles;
			replicateKillFishAndAttachProjectiles.ProjectilePositions = projectiles.GetPositions(local: true);
			replicateKillFishAndAttachProjectiles.ProjectileRotations = projectiles.GetRotations(local: true);
			replicateKillFishAndAttachProjectiles.Post();
		}
	}
}
