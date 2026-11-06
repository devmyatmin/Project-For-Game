using System;
using Beam;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

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
		HealthPoints -= damage;
		if (HealthPoints <= 0f)
		{
			BaseUtilities.DoPlayerDamageExperience_Harvesting(OriginalHealthPoints, damager2);
			SpawnHarvest();
		}
	}

	private void SpawnHarvest()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		HealthPoints = OriginalHealthPoints;
		if (MPExtensions.IsMaster(Game.Mode))
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
				Vector3 val = new Vector3(0f, 1f, 0f);
				Vector3 val2 = new Vector3(1f - Mathf.Clamp01(val.x), 1f - Mathf.Clamp01(val.y), 1f - Mathf.Clamp01(val.z));
				int num2 = 0;
				for (int j = 0; j < harvestResources.SpawnAmount; j++)
				{
					float num3 = 360f / (float)harvestResources.SpawnAmount * (float)num2++;
					Vector3 val3 = transform.rotation * (Quaternion.AngleAxis(num3, val) * val2 * num);
					Vector3 val4 = harvestResources.HarvestPosition.position + val3;
					Transform val5 = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(harvestResources.HarvestPrefab.GetComponent<SaveablePrefab>().PrefabId, val4, harvestResources.HarvestPosition.rotation, MiniGuid.New())).transform;
					Vector3 val6 = transform.position - val4;
					val5.forward = val6.normalized;
				}
			}
		}
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(transform);
		((UnityEvent<IBase, IBaseActionEventData>)Harvested)?.Invoke((IBase)this, (IBaseActionEventData)BaseActionEventData.Empty);
		base.Detach();
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}
}
