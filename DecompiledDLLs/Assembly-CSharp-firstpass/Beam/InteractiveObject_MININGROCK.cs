using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class InteractiveObject_MININGROCK : InteractiveObject
{
	[Serializable]
	public class RockStage
	{
		public GameObject obj;

		public Transform trans;
	}

	[SerializeField]
	private GameObject _spawnPrefab;

	[SerializeField]
	private RockStage[] _STAGES;

	[SerializeField]
	private GameObject _mineGib;

	[SerializeField]
	private bool _requirePick;

	[HideInInspector]
	public BaseActionUnityEvent Harvested;

	public bool RequirePick => _requirePick;

	protected override void Awake()
	{
		base.Awake();
		CheckHealth();
	}

	public override void Damage(float damage, IBase damager)
	{
		if (!_requirePick)
		{
			PickMine(damage, damager);
		}
	}

	public void PickMine(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, pickupable);
		}
		base.Damage(damage, damager);
		CheckHealth(hit: true);
	}

	public virtual void CheckHealth(bool hit = false)
	{
		if (_STAGES == null || _STAGES.Length == 0)
		{
			return;
		}
		HealthPoints = Mathf.Min(HealthPoints, OriginalHealthPoints);
		int num = Mathf.CeilToInt(HealthPoints / OriginalHealthPoints / (1f / (float)_STAGES.Length));
		num = _STAGES.Length - Mathf.Max(0, Mathf.Min(num, _STAGES.Length));
		int num2 = -1;
		for (int i = 0; i < _STAGES.Length; i++)
		{
			RockStage rockStage = _STAGES[i];
			bool flag = i == num;
			if (!flag && rockStage.obj.activeSelf)
			{
				num2 = i;
			}
			rockStage.obj.SetActive(flag);
		}
		if ((num2 >= 0) & hit)
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(OriginalHealthPoints / (float)_STAGES.Length, LastPlayerIdToDamageThis);
			HarvestResource(_STAGES[num2].trans);
			if (num2 == _STAGES.Length - 1)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	private void HarvestResource(Transform pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			MultiplayerMng.Instantiate<SaveablePrefab>(_spawnPrefab.GetComponent<ISaveablePrefab>().PrefabId, pos.position, pos.rotation, MiniGuid.New());
		}
		if ((Object)(object)_mineGib != (Object)null)
		{
			Object.Instantiate<GameObject>(_mineGib, pos.position, pos.rotation);
		}
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)Harvested)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CheckHealth();
	}
}
