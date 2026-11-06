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
			if (!Object.op_Implicit((Object)(object)_fixedJoint))
			{
				_fixedJoint = ((Component)this).GetComponent<FixedJoint>();
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
			while ((Object)(object)connectedAboveLog != (Object)null)
			{
				if (connectedAboveLog.IsDragging)
				{
					return false;
				}
				connectedAboveLog = connectedAboveLog.ConnectedAboveLog;
			}
			connectedAboveLog = ConnectedBelowLog;
			while ((Object)(object)connectedAboveLog != (Object)null)
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
		if (Object.op_Implicit((Object)(object)_connectedAboveLog))
		{
			for (int i = 0; i < _connectedAboveLog.AllColliders.Length; i++)
			{
				Physics.IgnoreCollision(_connectedAboveLog.AllColliders[i], collider, true);
			}
		}
		if (Object.op_Implicit((Object)(object)_connectedBelowLog))
		{
			for (int j = 0; j < _connectedBelowLog.AllColliders.Length; j++)
			{
				Physics.IgnoreCollision(_connectedBelowLog.AllColliders[j], collider, true);
			}
		}
	}

	public override void EnableCollisionsWithCollider(Collider collider)
	{
		base.EnableCollisionsWithCollider(collider);
		if (Object.op_Implicit((Object)(object)_connectedAboveLog))
		{
			for (int i = 0; i < _connectedAboveLog.AllColliders.Length; i++)
			{
				Physics.IgnoreCollision(_connectedAboveLog.AllColliders[i], collider, false);
			}
		}
		if (Object.op_Implicit((Object)(object)_connectedBelowLog))
		{
			for (int j = 0; j < _connectedBelowLog.AllColliders.Length; j++)
			{
				Physics.IgnoreCollision(_connectedBelowLog.AllColliders[j], collider, false);
			}
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		if (!Object.op_Implicit((Object)(object)_connectedAboveLog) && !Object.op_Implicit((Object)(object)_connectedBelowLog))
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(OriginalHealthPoints, LastPlayerIdToDamageThis);
			Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
			if ((Object)(object)_splitting != (Object)null)
			{
				_splitting.Split();
			}
			else
			{
				base.Destroy();
			}
			return;
		}
		HealthPoints = OriginalHealthPoints;
		if ((Object)(object)_connectedAboveLog != (Object)null)
		{
			if ((Object)(object)_connectedAboveLog.ConnectedBelowLog != (Object)null)
			{
				_connectedAboveLog.ConnectedBelowLog = null;
			}
			if (Object.op_Implicit((Object)(object)_fixedJoint))
			{
				BeamTeam.Destroy<FixedJoint>(_fixedJoint);
			}
			_connectedAboveLog.CheckSeperated();
			AwakePhysicsIfDragged();
			_connectedAboveLog = null;
		}
		else if ((Object)(object)_connectedBelowLog != (Object)null)
		{
			if (Object.op_Implicit((Object)(object)_connectedBelowLog.FixedJoint))
			{
				BeamTeam.Destroy<FixedJoint>(_connectedBelowLog.FixedJoint);
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
		if ((Object)(object)_connectedAboveLog == (Object)null && (Object)(object)_connectedBelowLog == (Object)null)
		{
			CanPickUp = true;
			HealthPoints = OriginalHealthPoints;
		}
		else
		{
			CanPickUp = false;
		}
	}

	private void AwakePhysicsIfDragged()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			bool flag = false;
			InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = this;
			while ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
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
				while ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
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
				while ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
				{
					interactiveObject_PALM_LOG.rigidbody.AddForce(Vector3.up * -0.1f, (ForceMode)1);
					interactiveObject_PALM_LOG = interactiveObject_PALM_LOG._connectedAboveLog;
				}
				interactiveObject_PALM_LOG = _connectedBelowLog;
				while ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
				{
					interactiveObject_PALM_LOG.rigidbody.AddForce(Vector3.up * -0.1f, (ForceMode)1);
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
		if ((Object)(object)_connectedAboveLog != (Object)null)
		{
			jObject.AddField("ConnectedAboveLog", JSerializer.Serialize((object)_connectedAboveLog.ReferenceId.ToString()));
		}
		if ((Object)(object)_connectedBelowLog != (Object)null)
		{
			jObject.AddField("ConnectedBelowLog", JSerializer.Serialize((object)_connectedBelowLog.ReferenceId.ToString()));
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		if ((Object)(object)rigidbody != (Object)null)
		{
			rigidbody.isKinematic = true;
		}
		base.Load(data);
		JObject field = data.GetField("ConnectedAboveLog");
		if (field != null)
		{
			MiniGuid connectedAboveLogData = Beam.Utilities.ExtensionMethods.ToMiniGuid(field.GetValue<string>());
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.FindObject<InteractiveObject_PALM_LOG>(connectedAboveLogData);
				if ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
				{
					_connectedAboveLog = interactiveObject_PALM_LOG;
					_fixedJoint = ((Component)this).gameObject.AddComponent<FixedJoint>();
					((Joint)_fixedJoint).connectedBody = _connectedAboveLog.rigidbody;
				}
				else
				{
					Debug.LogError((object)"InteractiveObject_PALM_LOG::LoadLevel:: Error finding above log.");
				}
			});
		}
		JObject field2 = data.GetField("ConnectedBelowLog");
		if (field2 != null)
		{
			MiniGuid connectedBelowLogData = Beam.Utilities.ExtensionMethods.ToMiniGuid(field2.GetValue<string>());
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.FindObject<InteractiveObject_PALM_LOG>(connectedBelowLogData);
				if ((Object)(object)interactiveObject_PALM_LOG != (Object)null)
				{
					_connectedBelowLog = interactiveObject_PALM_LOG;
				}
				else
				{
					Debug.LogError((object)"InteractiveObject_PALM_LOG::LoadLevel:: Error finding below log.");
				}
			});
		}
		if (field != null || field2 != null)
		{
			CanPickUp = false;
		}
	}
}
