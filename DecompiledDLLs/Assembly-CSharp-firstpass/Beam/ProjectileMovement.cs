using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class ProjectileMovement : MonoBehaviour
{
	private class ReplicateHandleCollision : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab Projectile;

		[Replicate]
		public Vector3 ProjectilePosition;

		[Replicate]
		public Quaternion ProjectileRotation;

		[Replicate]
		public Rigidbody Rigidbody;

		[Replicate]
		public Collider Collider;

		[Replicate]
		public Vector3 Point;

		public override void OnPeer()
		{
			((Component)Projectile).GetComponent<ProjectileMovement>()._replicatedCollissions.Enqueue(this);
		}

		public void Execute(ProjectileMovement movement)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			movement.HandleCollision(Rigidbody, Collider, Point);
			((Component)Projectile).transform.localPosition = ProjectilePosition;
			((Component)Projectile).transform.localRotation = ProjectileRotation;
		}
	}

	private class ReplicateLocalFaunaStick : MultiplayerMessage
	{
		[Replicate]
		public ProjectileObject Projectile;

		[Replicate]
		public Vector3 ProjectilePosition;

		[Replicate]
		public Vector3 ProjectileForward;

		[Replicate]
		public Vector3 Point;

		[Replicate]
		public byte Material;

		public override void OnPeer()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			ProjectileMovement component = ((Component)Projectile).GetComponent<ProjectileMovement>();
			component.SetupMultiplayerLaunch(fire: false);
			ProjectileEventDataVal projectileEventDataVal = new ProjectileEventDataVal(Point, (PhysicMaterial)Material);
			Projectile.transform.position = ProjectilePosition;
			Projectile.transform.forward = ProjectileForward;
			Projectile.LocalFaunaPierce();
			component._isStuck = true;
			component.Rigidbody.isKinematic = true;
			component.Rigidbody.velocity = Vector3.zero;
			((Component)component._trailRenderer).transform.SetParent(ParticleManager.TRAIL_CONTAINER, true);
			component._collider.enabled = true;
			component.Stuck?.Invoke(component, projectileEventDataVal);
		}
	}

	public static bool DEBUG_COLLISION;

	[SerializeField]
	private ProjectileCollisionDetectionBase _collisionDetection;

	[SerializeField]
	private LayerMask _collisionLayermask;

	[SerializeField]
	private LayerMask _autoAimLayermask;

	[SerializeField]
	private GameObject _impactParticle;

	[SerializeField]
	private GameObject _gib;

	[SerializeField]
	private float _projectileHead;

	[SerializeField]
	private float _projectileAutoAimRadius;

	[SerializeField]
	private int _projectileAutoAimSamples = 8;

	[SerializeField]
	private float _projectileDamage;

	[SerializeField]
	private TrailRenderer _trailRenderer;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Collider _collider;

	protected IPlayer _thrower;

	private IProjectile _projectile;

	private bool _isStuck;

	private RaycastHit _hit;

	private static string _raycastLabel;

	private Queue<ReplicateHandleCollision> _replicatedCollissions = new Queue<ReplicateHandleCollision>();

	public Rigidbody Rigidbody
	{
		get
		{
			if ((Object)(object)_rigidbody == (Object)null)
			{
				_rigidbody = ((Component)this).GetComponent<Rigidbody>();
			}
			return _rigidbody;
		}
	}

	public IBase AttachedBase { get; set; }

	public IPlayer Thrower => _thrower;

	public event Action<ProjectileMovement, IProjectileEventData> Stuck;

	public event Action<ProjectileMovement, IProjectileEventData> Broken;

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)_trailRenderer).transform.SetParent(ParticleManager.TRAIL_CONTAINER, false);
		((Component)_trailRenderer).transform.localPosition = Vector3.zero;
	}

	public void Initialize(IProjectile projectile)
	{
		_projectile = projectile;
	}

	private void OnDestroy()
	{
		Stuck = null;
		Broken = null;
		if ((Object)(object)_trailRenderer != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)_trailRenderer).gameObject);
		}
	}

	public void Fire(IPlayer player)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		_collider.enabled = false;
		_thrower = player;
		((Component)_trailRenderer).transform.SetParent(((Component)this).transform, false);
		((Component)_trailRenderer).transform.localPosition = Vector3.zero;
		_trailRenderer.Clear();
		SetupMultiplayerLaunch(fire: true);
	}

	private void SetupMultiplayerLaunch(bool fire)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			Rigidbody.isKinematic = !fire;
			Rigidbody.useGravity = fire;
			((SaveablePrefab)_projectile).ParentId = (fire ? TransformId.Unknown : MPExtensions.GetTransfromId(((Component)this).gameObject.transform.parent));
		}
	}

	public void Reset()
	{
		_collider.enabled = true;
		_thrower = null;
		_isStuck = false;
		AttachedBase = null;
	}

	private void FixedUpdate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (_isStuck || (Object)(object)Rigidbody == (Object)null)
		{
			return;
		}
		Vector3 velocity = Rigidbody.velocity;
		if (velocity.magnitude > 0.001f)
		{
			Rigidbody.MoveRotation(Quaternion.LookRotation(Rigidbody.velocity));
		}
		if (_thrower.IsOwner)
		{
			CheckCollision((Vector3 position, RaycastHit hit) =>
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00da: Unknown result type (might be due to invalid IL or missing references)
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Unknown result type (might be due to invalid IL or missing references)
				//IL_0081: Unknown result type (might be due to invalid IL or missing references)
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
				Vector3 point = hit.point;
				Collider collider = hit.collider;
				Rigidbody rigidbody = hit.rigidbody;
				PhysicMaterialPenetration penetration = GetPenetration(collider);
				if (penetration == PhysicMaterialPenetration.STICK)
				{
					Rigidbody.position = position;
				}
				if (MPExtensions.IsMultiplayer(Game.Mode))
				{
					if (PhysicMaterialPenetration.STICK == penetration && GetAttachedBase(collider, rigidbody) is Interactive_FISHES)
					{
						if (MPExtensions.IsClient(Game.Mode))
						{
							GetPositionAndForward(point, out var position2, out var forward);
							ReplicateLocalFaunaStick replicateLocalFaunaStick = new ReplicateLocalFaunaStick();
							replicateLocalFaunaStick.Projectile = (ProjectileObject)_projectile;
							replicateLocalFaunaStick.Point = point;
							replicateLocalFaunaStick.Material = (byte)PhysicMaterialUtility.GetPhysicMaterialType(collider);
							replicateLocalFaunaStick.ProjectilePosition = position2;
							replicateLocalFaunaStick.ProjectileForward = forward;
							replicateLocalFaunaStick.Post();
						}
						HandleCollision(rigidbody, collider, point);
					}
					else
					{
						collider = (MPExtensions.IsReplicable(collider) ? collider : null);
						rigidbody = (MPExtensions.IsReplicable(rigidbody) ? rigidbody : null);
						ReplicatedHandleCollision(rigidbody, collider, point);
					}
				}
				else
				{
					HandleCollision(rigidbody, collider, point);
				}
			});
		}
		if (_replicatedCollissions.Count != 0)
		{
			_replicatedCollissions.Dequeue().Execute(this);
		}
	}

	private void CheckCollision(Action<Vector3, RaycastHit> onCollision)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(_collisionDetection))
		{
			return;
		}
		Vector3 velocity = Rigidbody.velocity;
		Vector3 val;
		if (!(velocity.magnitude < 0.001f))
		{
			velocity = Rigidbody.velocity;
			val = velocity.normalized;
		}
		else
		{
			val = ((Component)this).transform.forward;
		}
		Vector3 val2 = val;
		Quaternion val3 = Quaternion.LookRotation(val2);
		float num = _projectileHead + Time.fixedDeltaTime * 120f;
		if (_collisionDetection.CheckCollisions(new Ray(Rigidbody.position, val2), out _hit, num, _collisionLayermask.value))
		{
			onCollision(Rigidbody.position, _hit);
			return;
		}
		for (int i = 0; i < _projectileAutoAimSamples; i++)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Random.Range(0f - _projectileAutoAimRadius, _projectileAutoAimRadius);
			zero.y = Random.Range(0f - _projectileAutoAimRadius, _projectileAutoAimRadius);
			Vector3 val4 = Rigidbody.position + val3 * zero;
			if (Physics.Raycast(new Ray(val4, val2), ref _hit, num, _autoAimLayermask.value))
			{
				onCollision(val4, _hit);
				break;
			}
		}
	}

	private void ReplicatedHandleCollision(Rigidbody rigidbody, Collider collider, Vector3 point)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (HandleCollision(rigidbody, collider, point) && MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateHandleCollision replicateHandleCollision = new ReplicateHandleCollision();
			replicateHandleCollision.Projectile = ((Component)this).GetComponent<SaveablePrefab>();
			replicateHandleCollision.ProjectilePosition = ((Component)this).transform.localPosition;
			replicateHandleCollision.ProjectileRotation = ((Component)this).transform.localRotation;
			replicateHandleCollision.Rigidbody = rigidbody;
			replicateHandleCollision.Collider = collider;
			replicateHandleCollision.Point = point;
			replicateHandleCollision.Post();
		}
	}

	private bool HandleCollision(Rigidbody rigidbody, Collider collider, Vector3 hitPoint)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		PhysicMaterial physicMaterialType = PhysicMaterialUtility.GetPhysicMaterialType(collider);
		ProjectileEventDataVal projectileEventDataVal = new ProjectileEventDataVal(hitPoint, physicMaterialType);
		PhysicMaterialPenetration penetration = GetPenetration(collider);
		if (DEBUG_COLLISION)
		{
			StrandedWorld.AddDebugAxis(hitPoint, ((Component)this).transform.rotation, $"R {((rigidbody != null) ? ((Object)rigidbody).name : null)}\nC {((collider != null) ? ((Object)collider).name : null)}\nP {penetration}");
		}
		switch (penetration)
		{
		case PhysicMaterialPenetration.STICK:
		{
			SetupMultiplayerLaunch(fire: false);
			_isStuck = true;
			Vector3 val2 = Rigidbody.mass * Rigidbody.velocity;
			Rigidbody.velocity = Vector3.zero;
			GetPositionAndForward(hitPoint, out var position, out var forward);
			((Component)this).transform.position = position;
			((Component)this).transform.forward = forward;
			AttachedBase = GetAttachedBase(collider, rigidbody);
			ProjectileHit args = new ProjectileHit(((Component)collider).transform, AttachedBase);
			_projectile.Pierce(args);
			if (!BeamTeam.IsNullOrDestroyed(AttachedBase))
			{
				IAttachable attachable = AttachedBase as IAttachable;
				if ((BeamTeam.IsNullOrDestroyed(attachable) || !attachable.IsAttached) && rigidbody != null)
				{
					rigidbody.AddForce(val2, (ForceMode)1);
				}
				Shootable component2 = ((Component)AttachedBase.transform).GetComponent<Shootable>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					((UnityEvent<DamageUnityEventArgs>)component2.Shot)?.Invoke(new DamageUnityEventArgs(_projectileDamage, _projectile as IBase));
				}
			}
			((Component)_trailRenderer).transform.SetParent(ParticleManager.TRAIL_CONTAINER, true);
			_collider.enabled = true;
			Stuck?.Invoke(this, projectileEventDataVal);
			return true;
		}
		case PhysicMaterialPenetration.BREAK:
			SetupMultiplayerLaunch(fire: false);
			Broken?.Invoke(this, projectileEventDataVal);
			Object.Instantiate<GameObject>(_gib, ((Component)this).transform.position, ((Component)this).transform.rotation);
			MultiplayerMng.Destroy(((Component)this).gameObject.GetComponent<SaveablePrefab>());
			return true;
		case PhysicMaterialPenetration.BREAK_DAMAGE:
		{
			SetupMultiplayerLaunch(fire: false);
			Broken?.Invoke(this, projectileEventDataVal);
			Vector3 val = Rigidbody.mass * Rigidbody.velocity;
			rigidbody.AddForce(val, (ForceMode)1);
			Shootable component = ((Component)rigidbody).GetComponent<Shootable>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((UnityEvent<DamageUnityEventArgs>)component.Shot)?.Invoke(new DamageUnityEventArgs(_projectileDamage, _projectile as IBase));
			}
			if ((Object)(object)_gib != (Object)null)
			{
				Object.Instantiate<GameObject>(_gib, ((Component)this).transform.position, ((Component)this).transform.rotation);
			}
			MultiplayerMng.Destroy(((Component)this).gameObject.GetComponent<SaveablePrefab>());
			return true;
		}
		default:
			return false;
		}
	}

	private void GetPositionAndForward(Vector3 hitPoint, out Vector3 position, out Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = hitPoint - Rigidbody.position;
		Vector3 normalized = val.normalized;
		position = hitPoint - normalized * _projectileHead;
		forward = normalized;
	}

	private IBase GetAttachedBase(Collider collider, Rigidbody rigidbody)
	{
		IBase obj = BeamTeam.GetInterface<IBase>((Component)(object)collider);
		if (BeamTeam.IsNullOrDestroyed(obj) && (Object)(object)rigidbody != (Object)null)
		{
			obj = BeamTeam.GetInterface<IBase>((Component)(object)rigidbody);
			if (BeamTeam.IsNullOrDestroyed(obj))
			{
				RagdollChild component = ((Component)rigidbody).GetComponent<RagdollChild>();
				if ((Object)(object)component != (Object)null)
				{
					obj = component.InteractiveObject;
				}
			}
		}
		return obj;
	}

	private PhysicMaterialPenetration GetPenetration(Collider collider)
	{
		if ((Object)(object)collider == (Object)null)
		{
			return PhysicMaterialPenetration.BREAK;
		}
		PhysicMaterial sharedMaterial = collider.sharedMaterial;
		return PhysicMaterialUtility.GetPenetration((sharedMaterial != null) ? ((Object)sharedMaterial).name : null);
	}
}
