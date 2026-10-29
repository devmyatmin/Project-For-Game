using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam;

public class InteractiveObject_PALM : InteractiveObject
{
	private class ReplicateConnectTreeSegments : MultiplayerMessage
	{
		[Replicate]
		public MiniGuid[] SegmentsReferenceIds;

		public ReplicateConnectTreeSegments()
		{
		}

		public ReplicateConnectTreeSegments(List<InteractiveObject_PALM_LOG> segments)
		{
			SegmentsReferenceIds = new MiniGuid[segments.Count];
			for (int i = 0; i < segments.Count; i++)
			{
				SegmentsReferenceIds[i] = segments[i].ReferenceId;
			}
		}

		public override void OnPeer()
		{
			Timing.RunCoroutine(ConnectSegments());
		}

		private IEnumerator<float> ConnectSegments()
		{
			List<InteractiveObject_PALM_LOG> segments = new List<InteractiveObject_PALM_LOG>();
			float startTime = Time.unscaledTime;
			bool found = false;
			while (!found && !(Time.unscaledTime > startTime + Peer.DEFAULT_TIMEOUT))
			{
				for (int i = 0; i < SegmentsReferenceIds.Length; i++)
				{
					MiniGuid referenceId = SegmentsReferenceIds[i];
					if (!referenceId.IsDefault())
					{
						InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.FindObject<InteractiveObject_PALM_LOG>(referenceId);
						if (!interactiveObject_PALM_LOG.IsNullOrDestroyed())
						{
							segments.Add(interactiveObject_PALM_LOG);
							SegmentsReferenceIds[i] = default(MiniGuid);
						}
					}
				}
				found = SegmentsReferenceIds.Length == segments.Count;
				if (!found)
				{
					yield return 0f;
				}
			}
			if (found)
			{
				ConnectTreeSegments(segments);
			}
			else
			{
				PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Timeout_PalmSegments);
			}
		}
	}

	private const float SEGMENT_OFFSET = 2.03f;

	[SerializeField]
	private int _treeNumber = 1;

	[SerializeField]
	private Transform _treeTrunk;

	[SerializeField]
	private InteractiveObject_FOOD _fruit;

	[SerializeField]
	private GameObject _fruitSpawnerPrefab;

	[SerializeField]
	private GameObject _stump;

	[SerializeField]
	private GameObject _landingSpot;

	private List<InteractiveObject_FOOD> _fruits = new List<InteractiveObject_FOOD>();

	private Vector3 _fallDir;

	[HideInInspector]
	public BaseActionUnityEvent Lopped;

	protected override void Start()
	{
		base.Start();
		if (!LevelLoader.IsLoading())
		{
			GenerateFruit();
		}
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		GenerateFruit();
	}

	private List<InteractiveObject_PALM_LOG> GenerateTreeSegments()
	{
		List<InteractiveObject_PALM_LOG> list = new List<InteractiveObject_PALM_LOG>();
		float num = 1.638f;
		Vector3 localPosition = new Vector3(_treeTrunk.localPosition.x, num, _treeTrunk.localPosition.z);
		InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.Instantiate<InteractiveObject_PALM_LOG>(Singleton<TreeManager>.Instance.TreeSegments[0].PrefabId, MiniGuid.New());
		interactiveObject_PALM_LOG.transform.rotation = base.transform.rotation;
		interactiveObject_PALM_LOG.transform.parent = base.transform;
		interactiveObject_PALM_LOG.transform.localPosition = localPosition;
		interactiveObject_PALM_LOG.transform.parent = null;
		interactiveObject_PALM_LOG.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
		interactiveObject_PALM_LOG.Parent();
		list.Add(interactiveObject_PALM_LOG);
		if (_treeNumber != 1)
		{
			localPosition = new Vector3(_treeTrunk.localPosition.x, num + 2.03f, _treeTrunk.localPosition.z);
			int num2 = 1;
			if (_treeNumber == 2)
			{
				num2 = 2;
			}
			for (int i = 1; i < _treeNumber; i++)
			{
				interactiveObject_PALM_LOG = MultiplayerMng.Instantiate<InteractiveObject_PALM_LOG>(Singleton<TreeManager>.Instance.TreeSegments[num2].PrefabId, MiniGuid.New());
				interactiveObject_PALM_LOG.transform.rotation = base.transform.rotation;
				interactiveObject_PALM_LOG.transform.parent = base.transform;
				interactiveObject_PALM_LOG.transform.localPosition = localPosition;
				interactiveObject_PALM_LOG.transform.parent = null;
				interactiveObject_PALM_LOG.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
				interactiveObject_PALM_LOG.Parent();
				list.Add(interactiveObject_PALM_LOG);
				localPosition += new Vector3(0f, 2.03f, 0f);
				num2 = 2;
				if (_treeNumber == 4 && i == 3)
				{
					num2 = 1;
				}
			}
		}
		InteractiveObject_PALM_LOG interactiveObject_PALM_LOG2 = MultiplayerMng.Instantiate<InteractiveObject_PALM_LOG>(Singleton<TreeManager>.Instance.TreeTopSegement.PrefabId, MiniGuid.New());
		interactiveObject_PALM_LOG2.transform.rotation = base.transform.rotation;
		interactiveObject_PALM_LOG2.transform.parent = base.transform;
		interactiveObject_PALM_LOG2.transform.localPosition = new Vector3(0f, 1.65f + 2.03f * (float)_treeNumber, 0f);
		interactiveObject_PALM_LOG2.transform.parent = null;
		interactiveObject_PALM_LOG2.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
		interactiveObject_PALM_LOG2.Parent();
		list.Add(interactiveObject_PALM_LOG2);
		return list;
	}

	private void GenerateFruit()
	{
		if (_fruits.Count != 0)
		{
			return;
		}
		Vector3 localPosition = new Vector3(0f, 1f + 2.03f * (float)_treeNumber, 0f);
		GameObject gameObject = Object.Instantiate(_fruitSpawnerPrefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = localPosition;
		int num = Mathf.Min(Singleton<StrandedWorld>.Instance.GenerateLocalSeed(base.transform.position).Range(1, gameObject.transform.childCount), 63);
		for (int i = 0; i < num; i++)
		{
			MiniGuid referenceId = MiniGuid.NewFrom(base.ReferenceId, i + 1);
			Transform fruitPosition = gameObject.transform.GetChild(i);
			if (Game.Mode.IsMaster())
			{
				AddFruit(MultiplayerMng.Instantiate<InteractiveObject_FOOD>(_fruit.PrefabId, fruitPosition.position, fruitPosition.rotation, referenceId), fruitPosition);
				continue;
			}
			MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.Palm, referenceId, delegate(MultiplayerZoneSync.Command command)
			{
				AddFruit((InteractiveObject_FOOD)command.Instance, fruitPosition);
			});
		}
		BeamTeam.Destroy(gameObject);
	}

	private void AddFruit(InteractiveObject_FOOD fruit, Transform fruitPosition)
	{
		fruit.transform.parent = null;
		fruit.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
		fruit.Parent();
		AttachFruit(fruit, fruitPosition);
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
			if (!playerFromId.IsNullOrDestroyed() && !playerFromId.Movement.ClimbingLadder)
			{
				BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
				_fallDir = playerFromId.transform.forward;
				base.Damage(damage, damager);
				Vector3 vector = new Vector3(0f, 0.15f / (base.OriginalHealthPoints / damage), 0f);
				_treeTrunk.localPosition += vector;
			}
		}
	}

	public override void Destroy()
	{
		BaseUtilities.DoPlayerDamageExperience_Harvesting(base.OriginalHealthPoints, LastPlayerIdToDamageThis);
		Lop();
	}

	private void Lop()
	{
		GetComponent<BoxCollider>().enabled = false;
		DetachFruit();
		if (Game.Mode.IsMaster())
		{
			List<InteractiveObject_PALM_LOG> list = GenerateTreeSegments();
			ReplicatedConnectTreeSegments(list);
			float num = Mathf.Max(10f, 20f / (float)list.Count);
			list[list.Count - 1].rigidbody.AddForce(_fallDir * num, ForceMode.Impulse);
		}
		Ladders componentInChildren = GetComponentInChildren<Ladders>();
		if (componentInChildren != null)
		{
			foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
			{
				if (allPlayer.Movement.activeLadder == componentInChildren)
				{
					allPlayer.Movement.ExitLadder();
				}
			}
		}
		_stump.transform.parent = base.transform.parent;
		base.transform.CheckForProjectilesInChildren();
		base.ActionEventData.Populate(this, null);
		Lopped?.Invoke(this, base.ActionEventData);
		BeamTeam.Destroy(base.gameObject);
	}

	private static void ConnectTreeSegments(List<InteractiveObject_PALM_LOG> segments)
	{
		int num = 0;
		for (int i = 0; i < segments.Count - 1; i++)
		{
			InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = segments[i];
			InteractiveObject_PALM_LOG interactiveObject_PALM_LOG2 = segments[i + 1];
			if (i > 0)
			{
				InteractiveObject_PALM_LOG connectedBelowLog = segments[i - 1];
				interactiveObject_PALM_LOG.ConnectedBelowLog = connectedBelowLog;
			}
			interactiveObject_PALM_LOG.ConnectedAboveLog = interactiveObject_PALM_LOG2;
			FixedJoint fixedJoint = segments[i].gameObject.AddComponent<FixedJoint>();
			fixedJoint.connectedBody = interactiveObject_PALM_LOG2.rigidbody;
			fixedJoint.breakForce = float.PositiveInfinity;
			fixedJoint.breakTorque = float.PositiveInfinity;
			interactiveObject_PALM_LOG.FixedJoint = fixedJoint;
			num++;
		}
		if (num > 0)
		{
			InteractiveObject_PALM_LOG connectedBelowLog2 = segments[num - 1];
			segments[num].ConnectedBelowLog = connectedBelowLog2;
		}
		foreach (InteractiveObject_PALM_LOG segment in segments)
		{
			segment.CheckSeperated();
		}
	}

	public void AttachFruit(InteractiveObject_FOOD fruit, Transform position)
	{
		if (position.IsNullOrDestroyed())
		{
			fruit.Attach(null);
		}
		else
		{
			fruit.Attach(new StaticAttachingAnchor(position));
		}
		fruit.DetachedEvent += Fruit_Detached;
		_fruits.Add(fruit);
	}

	private void DetachFruit()
	{
		for (int i = 0; i < _fruits.Count; i++)
		{
			InteractiveObject_FOOD interactiveObject_FOOD = _fruits[i];
			interactiveObject_FOOD.DetachedEvent -= Fruit_Detached;
			interactiveObject_FOOD.DetachFromImpact();
			interactiveObject_FOOD.rigidbody.AddForce(_fallDir * 4f, ForceMode.VelocityChange);
		}
		_fruits.Clear();
	}

	private void Fruit_Detached(IAttachable sender)
	{
		InteractiveObject_FOOD interactiveObject_FOOD = sender as InteractiveObject_FOOD;
		interactiveObject_FOOD.DetachedEvent -= Fruit_Detached;
		_fruits.Remove(interactiveObject_FOOD);
	}

	private void ReplicatedConnectTreeSegments(List<InteractiveObject_PALM_LOG> segments)
	{
		if (Game.Mode.IsMultiplayer())
		{
			new ReplicateConnectTreeSegments(segments).Post();
		}
		ConnectTreeSegments(segments);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (_fruits.Count > 0)
		{
			JObject jObject2 = new JObject();
			for (int i = 0; i < _fruits.Count; i++)
			{
				jObject2.Add(_fruits[i].ReferenceId.ToString());
			}
			jObject.AddField("Fruits", jObject2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject fruitsData = data.GetField("Fruits");
		if (fruitsData == null)
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
		{
			for (int i = 0; i < fruitsData.Children.Count; i++)
			{
				InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.FindObject<InteractiveObject_FOOD>(fruitsData.Children[i].GetValue<string>().ToMiniGuid());
				if (interactiveObject_FOOD != null)
				{
					AttachFruit(interactiveObject_FOOD, interactiveObject_FOOD.transform);
				}
			}
		});
	}
}
