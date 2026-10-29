using System;
using Beam;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_YUCCA_HARVEST : InteractiveObject_FOOD
{
	[Serializable]
	public class HarvestResources
	{
		public GameObject HarvestPrefab;

		public Transform HarvestPosition;

		public int SpawnAmount = 1;
	}

	[SerializeField]
	private HarvestResources[] _resources;

	[HideInInspector]
	public BaseActionUnityEvent Harvested;

	public override void Damage(float damage, IBase damager)
	{
		OnDamaged(damage, damager);
		IPickupable damager2 = damager as IPickupable;
		BaseUtilities.DoPlayerDamageMultipliers_Harvesting(ref damage, damager2);
		base.HealthPoints -= damage;
		if (base.HealthPoints <= 0f)
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(base.OriginalHealthPoints, damager2);
			SpawnHarvest();
		}
	}

	private void SpawnHarvest()
	{
		base.HealthPoints = base.OriginalHealthPoints;
		if (Game.Mode.IsMaster())
		{
			for (int i = 0; i < _resources.Length; i++)
			{
				HarvestResources harvestResources = _resources[i];
				if (harvestResources.SpawnAmount <= 1)
				{
					MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, harvestResources.HarvestPosition.position, harvestResources.HarvestPosition.rotation, MiniGuid.New());
					continue;
				}
				float num = 0.5f;
				Vector3 axis = new Vector3(0f, 1f, 0f);
				Vector3 vector = new Vector3(1f - Mathf.Clamp01(axis.x), 1f - Mathf.Clamp01(axis.y), 1f - Mathf.Clamp01(axis.z));
				int num2 = 0;
				for (int j = 0; j < harvestResources.SpawnAmount; j++)
				{
					float angle = 360f / (float)harvestResources.SpawnAmount * (float)num2++;
					Vector3 vector2 = base.transform.rotation * (Quaternion.AngleAxis(angle, axis) * vector * num);
					Vector3 vector3 = harvestResources.HarvestPosition.position + vector2;
					MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, vector3, harvestResources.HarvestPosition.rotation, MiniGuid.New()).transform.forward = (base.transform.position - vector3).normalized;
				}
			}
		}
		base.transform.CheckForProjectilesInChildren();
		Harvested?.Invoke(this, BaseActionEventData.Empty);
		base.Detach();
		BeamTeam.Destroy(base.gameObject);
	}
}
