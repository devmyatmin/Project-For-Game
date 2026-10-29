using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

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
		if (!pickupable.IsNullOrDestroyed())
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
		base.HealthPoints = Mathf.Min(base.HealthPoints, base.OriginalHealthPoints);
		int a = Mathf.CeilToInt(base.HealthPoints / base.OriginalHealthPoints / (1f / (float)_STAGES.Length));
		a = _STAGES.Length - Mathf.Max(0, Mathf.Min(a, _STAGES.Length));
		int num = -1;
		for (int i = 0; i < _STAGES.Length; i++)
		{
			RockStage rockStage = _STAGES[i];
			bool flag = i == a;
			if (!flag && rockStage.obj.activeSelf)
			{
				num = i;
			}
			rockStage.obj.SetActive(flag);
		}
		if (num >= 0 && hit)
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(base.OriginalHealthPoints / (float)_STAGES.Length, LastPlayerIdToDamageThis);
			HarvestResource(_STAGES[num].trans);
			if (num == _STAGES.Length - 1)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void HarvestResource(Transform pos)
	{
		if (Game.Mode.IsMaster())
		{
			MultiplayerMng.Instantiate<SaveablePrefab>(_spawnPrefab.GetComponent<ISaveablePrefab>().PrefabId, pos.position, pos.rotation, MiniGuid.New());
		}
		if (_mineGib != null)
		{
			UnityEngine.Object.Instantiate(_mineGib, pos.position, pos.rotation);
		}
		base.ActionEventData.Populate(this, null);
		Harvested?.Invoke(this, base.ActionEventData);
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CheckHealth();
	}
}
