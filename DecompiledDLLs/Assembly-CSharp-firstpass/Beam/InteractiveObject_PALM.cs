using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Events;

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
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
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
						if (!BeamTeam.IsNullOrDestroyed(interactiveObject_PALM_LOG))
						{
							segments.Add(interactiveObject_PALM_LOG);
							SegmentsReferenceIds[i] = default;
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
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		List<InteractiveObject_PALM_LOG> list = new List<InteractiveObject_PALM_LOG>();
		float num = 1.638f;
		Vector3 localPosition = new Vector3(_treeTrunk.localPosition.x, num, _treeTrunk.localPosition.z);
		InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = MultiplayerMng.Instantiate<InteractiveObject_PALM_LOG>(Singleton<TreeManager>.Instance.TreeSegments[0].PrefabId, MiniGuid.New());
		interactiveObject_PALM_LOG.transform.rotation = transform.rotation;
		interactiveObject_PALM_LOG.transform.parent = transform;
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
				interactiveObject_PALM_LOG.transform.rotation = transform.rotation;
				interactiveObject_PALM_LOG.transform.parent = transform;
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
		interactiveObject_PALM_LOG2.transform.rotation = transform.rotation;
		interactiveObject_PALM_LOG2.transform.parent = transform;
		interactiveObject_PALM_LOG2.transform.localPosition = new Vector3(0f, 1.65f + 2.03f * (float)_treeNumber, 0f);
		interactiveObject_PALM_LOG2.transform.parent = null;
		interactiveObject_PALM_LOG2.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
		interactiveObject_PALM_LOG2.Parent();
		list.Add(interactiveObject_PALM_LOG2);
		return list;
	}

	private void GenerateFruit()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (_fruits.Count != 0)
		{
			return;
		}
		Vector3 localPosition = new Vector3(0f, 1f + 2.03f * (float)_treeNumber, 0f);
		GameObject val = Object.Instantiate<GameObject>(_fruitSpawnerPrefab);
		val.transform.parent = transform;
		val.transform.localPosition = localPosition;
		int num = Mathf.Min(MPExtensions.Range(Singleton<StrandedWorld>.Instance.GenerateLocalSeed(transform.position), 1, val.transform.childCount), 63);
		for (int i = 0; i < num; i++)
		{
			MiniGuid referenceId = MiniGuid.NewFrom(ReferenceId, i + 1);
			Transform fruitPosition = val.transform.GetChild(i);
			if (MPExtensions.IsMaster(Game.Mode))
			{
				AddFruit(MultiplayerMng.Instantiate<InteractiveObject_FOOD>(_fruit.PrefabId, fruitPosition.position, fruitPosition.rotation, referenceId), fruitPosition);
				continue;
			}
			MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.Palm, referenceId, (MultiplayerZoneSync.Command command) =>
			{
				AddFruit((InteractiveObject_FOOD)command.Instance, fruitPosition);
			});
		}
		BeamTeam.Destroy<GameObject>(val);
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
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
			if (!BeamTeam.IsNullOrDestroyed(playerFromId) && !playerFromId.Movement.ClimbingLadder)
			{
				BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
				_fallDir = playerFromId.transform.forward;
				base.Damage(damage, damager);
				Vector3 val = new Vector3(0f, 0.15f / (OriginalHealthPoints / damage), 0f);
				Transform treeTrunk = _treeTrunk;
				treeTrunk.localPosition += val;
			}
		}
	}

	public override void Destroy()
	{
		BaseUtilities.DoPlayerDamageExperience_Harvesting(OriginalHealthPoints, LastPlayerIdToDamageThis);
		Lop();
	}

	private void Lop()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((Collider)((Component)this).GetComponent<BoxCollider>()).enabled = false;
		DetachFruit();
		if (MPExtensions.IsMaster(Game.Mode))
		{
			List<InteractiveObject_PALM_LOG> list = GenerateTreeSegments();
			ReplicatedConnectTreeSegments(list);
			float num = Mathf.Max(10f, 20f / (float)list.Count);
			list[list.Count - 1].rigidbody.AddForce(_fallDir * num, (ForceMode)1);
		}
		Ladders componentInChildren = ((Component)this).GetComponentInChildren<Ladders>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
			{
				if ((Object)(object)allPlayer.Movement.activeLadder == (Object)(object)componentInChildren)
				{
					allPlayer.Movement.ExitLadder();
				}
			}
		}
		_stump.transform.parent = transform.parent;
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)Lopped)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
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
			FixedJoint val = ((Component)segments[i]).gameObject.AddComponent<FixedJoint>();
			((Joint)val).connectedBody = interactiveObject_PALM_LOG2.rigidbody;
			((Joint)val).breakForce = float.PositiveInfinity;
			((Joint)val).breakTorque = float.PositiveInfinity;
			interactiveObject_PALM_LOG.FixedJoint = val;
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
		if (BeamTeam.IsNullOrDestroyed(position))
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
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _fruits.Count; i++)
		{
			InteractiveObject_FOOD interactiveObject_FOOD = _fruits[i];
			interactiveObject_FOOD.DetachedEvent -= Fruit_Detached;
			interactiveObject_FOOD.DetachFromImpact();
			interactiveObject_FOOD.rigidbody.AddForce(_fallDir * 4f, (ForceMode)2);
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
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
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			for (int i = 0; i < fruitsData.Children.Count; i++)
			{
				InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.FindObject<InteractiveObject_FOOD>(Beam.Utilities.ExtensionMethods.ToMiniGuid(fruitsData.Children[i].GetValue<string>()));
				if ((Object)(object)interactiveObject_FOOD != (Object)null)
				{
					AttachFruit(interactiveObject_FOOD, interactiveObject_FOOD.transform);
				}
			}
		});
	}
}
