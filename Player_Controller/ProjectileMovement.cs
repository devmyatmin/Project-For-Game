using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

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
			Projectile.GetComponent<ProjectileMovement>()._replicatedCollissions.Enqueue(this);
		}

		public void Execute(ProjectileMovement movement)
		{
			movement.HandleCollision(Rigidbody, Collider, Point);
			Projectile.transform.localPosition = ProjectilePosition;
			Projectile.transform.localRotation = ProjectileRotation;
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
			ProjectileMovement component = Projectile.GetComponent<ProjectileMovement>();
			component.SetupMultiplayerLaunch(fire: false);
			ProjectileEventDataVal projectileEventDataVal = new ProjectileEventDataVal(Point, (PhysicMaterial)Material);
			Projectile.transform.position = ProjectilePosition;
			Projectile.transform.forward = ProjectileForward;
			Projectile.LocalFaunaPierce();
			component._isStuck = true;
			component.Rigidbody.isKinematic = true;
			component.Rigidbody.velocity = Vector3.zero;
			component._trailRenderer.transform.SetParent(ParticleManager.TRAIL_CONTAINER, worldPositionStays: true);
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
			if (_rigidbody == null)
			{
				_rigidbody = GetComponent<Rigidbody>();
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
		_trailRenderer.transform.SetParent(ParticleManager.TRAIL_CONTAINER, worldPositionStays: false);
		_trailRenderer.transform.localPosition = Vector3.zero;
	}

	public void Initialize(IProjectile projectile)
	{
		_projectile = projectile;
	}

	private void OnDestroy()
	{
		this.Stuck = null;
		this.Broken = null;
		if (_trailRenderer != null)
		{
			BeamTeam.Destroy(_trailRenderer.gameObject);
		}
	}

	public void Fire(IPlayer player)
	{
		_collider.enabled = false;
		_thrower = player;
		_trailRenderer.transform.SetParent(base.transform, worldPositionStays: false);
		_trailRenderer.transform.localPosition = Vector3.zero;
		_trailRenderer.Clear();
		SetupMultiplayerLaunch(fire: true);
	}

	private void SetupMultiplayerLaunch(bool fire)
	{
		if (Game.Mode.IsMultiplayer())
		{
			Rigidbody.isKinematic = !fire;
			Rigidbody.useGravity = fire;
			((SaveablePrefab)_projectile).ParentId = (fire ? TransformId.Unknown : base.gameObject.transform.parent.GetTransfromId());
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
		if (_isStuck || Rigidbody == null)
		{
			return;
		}
		if (Rigidbody.velocity.magnitude > 0.001f)
		{
			Rigidbody.MoveRotation(Quaternion.LookRotation(Rigidbody.velocity));
		}
		if (_thrower.IsOwner)
		{
			CheckCollision(delegate(Vector3 position, RaycastHit hit)
			{
				Vector3 point = hit.point;
				Collider collider = hit.collider;
				Rigidbody rigidbody = hit.rigidbody;
				PhysicMaterialPenetration penetration = GetPenetration(collider);
				if (penetration == PhysicMaterialPenetration.STICK)
				{
					Rigidbody.position = position;
				}
				if (Game.Mode.IsMultiplayer())
				{
					if (PhysicMaterialPenetration.STICK == penetration && GetAttachedBase(collider, rigidbody) is Interactive_FISHES)
					{
						if (Game.Mode.IsClient())
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
						collider = (collider.IsReplicable() ? collider : null);
						rigidbody = (rigidbody.IsReplicable() ? rigidbody : null);
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
		if (_collisionDetection.IsNullOrDestroyed())
		{
			return;
		}
		Vector3 vector = ((Rigidbody.velocity.magnitude < 0.001f) ? base.transform.forward : Rigidbody.velocity.normalized);
		Quaternion quaternion = Quaternion.LookRotation(vector);
		float num = _projectileHead + Time.fixedDeltaTime * 120f;
		if (_collisionDetection.CheckCollisions(new Ray(Rigidbody.position, vector), out _hit, num, _collisionLayermask.value))
		{
			onCollision(Rigidbody.position, _hit);
			return;
		}
		for (int i = 0; i < _projectileAutoAimSamples; i++)
		{
			Vector3 zero = Vector3.zero;
			zero.x = UnityEngine.Random.Range(0f - _projectileAutoAimRadius, _projectileAutoAimRadius);
			zero.y = UnityEngine.Random.Range(0f - _projectileAutoAimRadius, _projectileAutoAimRadius);
			Vector3 vector2 = Rigidbody.position + quaternion * zero;
			if (Physics.Raycast(new Ray(vector2, vector), out _hit, num, _autoAimLayermask.value))
			{
				onCollision(vector2, _hit);
				break;
			}
		}
	}

	private void ReplicatedHandleCollision(Rigidbody rigidbody, Collider collider, Vector3 point)
	{
		if (HandleCollision(rigidbody, collider, point) && Game.Mode.IsMultiplayer())
		{
			ReplicateHandleCollision replicateHandleCollision = new ReplicateHandleCollision();
			replicateHandleCollision.Projectile = GetComponent<SaveablePrefab>();
			replicateHandleCollision.ProjectilePosition = base.transform.localPosition;
			replicateHandleCollision.ProjectileRotation = base.transform.localRotation;
			replicateHandleCollision.Rigidbody = rigidbody;
			replicateHandleCollision.Collider = collider;
			replicateHandleCollision.Point = point;
			replicateHandleCollision.Post();
		}
	}

	private bool HandleCollision(Rigidbody rigidbody, Collider collider, Vector3 hitPoint)
	{
		PhysicMaterial physicMaterialType = PhysicMaterialUtility.GetPhysicMaterialType(collider);
		ProjectileEventDataVal projectileEventDataVal = new ProjectileEventDataVal(hitPoint, physicMaterialType);
		PhysicMaterialPenetration penetration = GetPenetration(collider);
		if (DEBUG_COLLISION)
		{
			StrandedWorld.AddDebugAxis(hitPoint, base.transform.rotation, $"R {rigidbody?.name}\nC {collider?.name}\nP {penetration}");
		}
		switch (penetration)
		{
		case PhysicMaterialPenetration.STICK:
		{
			SetupMultiplayerLaunch(fire: false);
			_isStuck = true;
			Vector3 force2 = Rigidbody.mass * Rigidbody.velocity;
			Rigidbody.velocity = Vector3.zero;
			GetPositionAndForward(hitPoint, out var position, out var forward);
			base.transform.position = position;
			base.transform.forward = forward;
			AttachedBase = GetAttachedBase(collider, rigidbody);
			ProjectileHit args = new ProjectileHit(collider.transform, AttachedBase);
			_projectile.Pierce(args);
			if (!AttachedBase.IsNullOrDestroyed())
			{
				IAttachable attachable = AttachedBase as IAttachable;
				if (attachable.IsNullOrDestroyed() || !attachable.IsAttached)
				{
					rigidbody?.AddForce(force2, ForceMode.Impulse);
				}
				Shootable component2 = AttachedBase.transform.GetComponent<Shootable>();
				if ((bool)component2)
				{
					component2.Shot?.Invoke(new DamageUnityEventArgs(_projectileDamage, _projectile as IBase));
				}
			}
			_trailRenderer.transform.SetParent(ParticleManager.TRAIL_CONTAINER, worldPositionStays: true);
			_collider.enabled = true;
			this.Stuck?.Invoke(this, projectileEventDataVal);
			return true;
		}
		case PhysicMaterialPenetration.BREAK:
			SetupMultiplayerLaunch(fire: false);
			this.Broken?.Invoke(this, projectileEventDataVal);
			UnityEngine.Object.Instantiate(_gib, base.transform.position, base.transform.rotation);
			MultiplayerMng.Destroy(base.gameObject.GetComponent<SaveablePrefab>());
			return true;
		case PhysicMaterialPenetration.BREAK_DAMAGE:
		{
			SetupMultiplayerLaunch(fire: false);
			this.Broken?.Invoke(this, projectileEventDataVal);
			Vector3 force = Rigidbody.mass * Rigidbody.velocity;
			rigidbody.AddForce(force, ForceMode.Impulse);
			Shootable component = rigidbody.GetComponent<Shootable>();
			if ((bool)component)
			{
				component.Shot?.Invoke(new DamageUnityEventArgs(_projectileDamage, _projectile as IBase));
			}
			if (_gib != null)
			{
				UnityEngine.Object.Instantiate(_gib, base.transform.position, base.transform.rotation);
			}
			MultiplayerMng.Destroy(base.gameObject.GetComponent<SaveablePrefab>());
			return true;
		}
		default:
			return false;
		}
	}

	private void GetPositionAndForward(Vector3 hitPoint, out Vector3 position, out Vector3 forward)
	{
		Vector3 normalized = (hitPoint - Rigidbody.position).normalized;
		position = hitPoint - normalized * _projectileHead;
		forward = normalized;
	}

	private IBase GetAttachedBase(Collider collider, Rigidbody rigidbody)
	{
		IBase @base = collider.GetInterface<IBase>();
		if (@base.IsNullOrDestroyed() && rigidbody != null)
		{
			@base = rigidbody.GetInterface<IBase>();
			if (@base.IsNullOrDestroyed())
			{
				RagdollChild component = rigidbody.GetComponent<RagdollChild>();
				if (component != null)
				{
					@base = component.InteractiveObject;
				}
			}
		}
		return @base;
	}

	private PhysicMaterialPenetration GetPenetration(Collider collider)
	{
		if (collider == null)
		{
			return PhysicMaterialPenetration.BREAK;
		}
		return PhysicMaterialUtility.GetPenetration(collider.sharedMaterial?.name);
	}
}
