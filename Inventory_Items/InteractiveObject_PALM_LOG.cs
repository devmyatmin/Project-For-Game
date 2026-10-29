using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class InteractiveObject_PALM_LOG : InteractiveObject
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Splitting _splitting;

	private FixedJoint _fixedJoint;

	private InteractiveObject_PALM_LOG _connectedAboveLog;

	private InteractiveObject_PALM_LOG _connectedBelowLog;

	public FixedJoint FixedJoint
	{
		get
		{
			if (!_fixedJoint)
			{
				_fixedJoint = GetComponent<FixedJoint>();
			}
			return _fixedJoint;
		}
		set
		{
			_fixedJoint = value;
		}
	}

	public InteractiveObject_PALM_LOG ConnectedAboveLog
	{
		get
		{
			return _connectedAboveLog;
		}
		set
		{
			_connectedAboveLog = value;
		}
	}

	public InteractiveObject_PALM_LOG ConnectedBelowLog
	{
		get
		{
			return _connectedBelowLog;
		}
		set
		{
			_connectedBelowLog = value;
		}
	}

	public override bool CanDrag
	{
		get
		{
			InteractiveObject_PALM_LOG connectedAboveLog = ConnectedAboveLog;
			while (connectedAboveLog != null)
			{
				if (connectedAboveLog.IsDragging)
				{
					return false;
				}
				connectedAboveLog = connectedAboveLog.ConnectedAboveLog;
			}
			connectedAboveLog = ConnectedBelowLog;
			while (connectedAboveLog != null)
			{
				if (connectedAboveLog.IsDragging)
				{
					return false;
				}
				connectedAboveLog = connectedAboveLog.ConnectedBelowLog;
			}
			return base.CanDrag;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (!LevelLoader.IsLoading())
		{
			CheckSeperated();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		CheckSeperated();
	}

	public override void DisableCollisionsWithCollider(Collider collider)
	{
		base.DisableCollisionsWithCollider(collider);
		if ((bool)_connectedAboveLog)
		{
			for (int i = 0; i < _connectedAboveLog.AllColliders.Length; i++)
			{
				Physics.IgnoreCollision(_connectedAboveLog.AllColliders[i], collider, ignore: true);
			}
		}
		if ((bool)_connectedBelowLog)
		{
			for (int j = 0; j < _connectedBelowLog.AllColliders.Length; j++)
			{
				Physics.IgnoreCollision(_connectedBelowLog.AllColliders[j], collider, ignore: true);
			}
		}
	}

	public override void EnableCollisionsWithCollider(Collider collider)
	{
		base.EnableCollisionsWithCollider(collider);
		if ((bool)_connectedAboveLog)
		{
			for (int i = 0; i < _connectedAboveLog.AllColliders.Length; i++)
			{
				Physics.IgnoreCollision(_connectedAboveLog.AllColliders[i], collider, ignore: false);
			}
		}
		if ((bool)_connectedBelowLog)
		{
			for (int j = 0; j < _connectedBelowLog.AllColliders.Length; j++)
			{
				Physics.IgnoreCollision(_connectedBelowLog.AllColliders[j], collider, ignore: false);
			}
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		if (!_connectedAboveLog && !_connectedBelowLog)
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(base.OriginalHealthPoints, LastPlayerIdToDamageThis);
			base.transform.CheckForProjectilesInChildren();
			if (_splitting != null)
			{
				_splitting.Split();
			}
			else
			{
				base.Destroy();
			}
			return;
		}
		base.HealthPoints = base.OriginalHealthPoints;
		if (_connectedAboveLog != null)
		{
			if (_connectedAboveLog.ConnectedBelowLog != null)
			{
				_connectedAboveLog.ConnectedBelowLog = null;
			}
			if ((bool)_fixedJoint)
			{
				BeamTeam.Destroy(_fixedJoint);
			}
			_connectedAboveLog.CheckSeperated();
			AwakePhysicsIfDragged();
			_connectedAboveLog = null;
		}
		else if (_connectedBelowLog != null)
		{
			if ((bool)_connectedBelowLog.FixedJoint)
			{
				BeamTeam.Destroy(_connectedBelowLog.FixedJoint);
			}
			_connectedBelowLog.ConnectedAboveLog = null;
			_connectedBelowLog.CheckSeperated();
			AwakePhysicsIfDragged();
			_connectedBelowLog = null;
		}
		CheckSeperated();
	}

	public void CheckSeperated()
	{
		if (_connectedAboveLog == null && _connectedBelowLog == null)
		{
			CanPickUp = true;
			base.HealthPoints = base.OriginalHealthPoints;
		}
		else
		{
			CanPickUp = false;
		}
	}

	private void AwakePhysicsIfDragged()
	{
		try
		{
			bool flag = false;
			InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = this;
			while (interactiveObject_PALM_LOG != null)
			{
				if (interactiveObject_PALM_LOG.IsDragging)
				{
					flag = true;
					break;
				}
				interactiveObject_PALM_LOG = interactiveObject_PALM_LOG._connectedAboveLog;
			}
			if (!flag)
			{
				interactiveObject_PALM_LOG = _connectedBelowLog;
				while (interactiveObject_PALM_LOG != null)
				{
					if (interactiveObject_PALM_LOG.IsDragging)
					{
						flag = true;
						break;
					}
					interactiveObject_PALM_LOG = interactiveObject_PALM_LOG._connectedBelowLog;
				}
			}
			if (flag)
			{
				interactiveObject_PALM_LOG = _connectedAboveLog;
				while (interactiveObject_PALM_LOG != null)
				{
					interactiveObject_PALM_LOG.rigidbody.AddForce(Vector3.up * -0.1f, ForceMode.Impulse);
					interactiveObject_PALM_LOG = interactiveObject_PALM_LOG._connectedAboveLog;
				}
				interactiveObject_PALM_LOG = _connectedBelowLog;
				while (interactiveObject_PALM_LOG != null)
				{
					interactiveObject_PALM_LOG.rigidbody.AddForce(Vector3.up * -0.1f, ForceMode.Impulse);
					interactiveObject_PALM_LOG = interactiveObject_PALM_LOG._connectedBelowLog;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (_connectedAboveLog != null)
		{
			jObject.AddField("ConnectedAboveLog", JSerializer.Serialize(_connectedAboveLog.ReferenceId.ToString()));
		}
		if (_connectedBelowLog != null)
		{
			jObject.AddField("ConnectedBelowLog", JSerializer.Serialize(_connectedBelowLog.ReferenceId.ToString()));
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		if (base.rigidbody != null)
		{
			base.rigidbody.isKinematic = true;
		}
		base.Load(data);
		JObject field = data.GetField("ConnectedAboveLog");
		if (field != null)
		{
			MiniGuid connectedAboveLogData = field.GetValue<string>().ToMiniGuid();
			LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
			{
				InteractiveObject_PALM_LOG interactiveObject_PALM_LOG2 = MultiplayerMng.FindObject<InteractiveObject_PALM_LOG>(connectedAboveLogData);
				if (interactiveObject_PALM_LOG2 != null)
				{
					_connectedAboveLog = interactiveObject_PALM_LOG2;
					_fixedJoint = base.gameObject.AddComponent<FixedJoint>();
					_fixedJoint.connectedBody = _connectedAboveLog.rigidbody;
				}
				else
				{
					Debug.LogError("InteractiveObject_PALM_LOG::LoadLevel:: Error finding above log.");
				}
			});
		}
		JObject field2 = data.GetField("ConnectedBelowLog");
		if (field2 != null)
		{
			MiniGuid connectedBelowLogData = field2.GetValue<string>().ToMiniGuid();
			LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
			{
				InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.FindObject<InteractiveObject_PALM_LOG>(connectedBelowLogData);
				if (interactiveObject_PALM_LOG != null)
				{
					_connectedBelowLog = interactiveObject_PALM_LOG;
				}
				else
				{
					Debug.LogError("InteractiveObject_PALM_LOG::LoadLevel:: Error finding below log.");
				}
			});
		}
		if (field != null || field2 != null)
		{
			CanPickUp = false;
		}
	}
}
