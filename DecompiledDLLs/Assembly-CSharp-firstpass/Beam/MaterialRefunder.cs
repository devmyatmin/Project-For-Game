using System.Collections.Generic;
using Beam.Crafting;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

[DisallowMultipleComponent]
public class MaterialRefunder : MonoBehaviour
{
	private class RefillFuelCanMsg : I_MultiplayerMessage
	{
		[I_Replicate]
		public InteractiveObject_FUELCAN FuelCan;

		[Replicate]
		public float Fuel;

		public override void OnPeer()
		{
			FuelCan.Refill(Fuel);
		}
	}

	private class RefillBottleMsg : I_MultiplayerMessage
	{
		[I_Replicate]
		public ConstructionObject_WATERBOTTLE Bottle;

		[Replicate]
		public int Water;

		public override void OnPeer()
		{
			RefillBottle(Bottle, Water);
		}
	}

	[SerializeField]
	private string nameToLookup;

	[SerializeField]
	private GameObject _gib;

	[SerializeField]
	private List<uint> _materials;

	public void RefundMaterials(int playerID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		SpawnGib();
		if (MPExtensions.IsClient(Game.Mode))
		{
			return;
		}
		IPlayer player = PlayerRegistry.AllPlayers[playerID];
		EventManager.RaiseEvent(new ExperienceEvent(playerID, PlayerSkills.Skills.PHYSICAL, PlayerSkills.ExpSource.DECONSTRUCTION_HIT));
		if (_materials.Count <= 0)
		{
			return;
		}
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		foreach (uint material in _materials)
		{
			if (!dictionary.ContainsKey(material))
			{
				dictionary.Add(material, 0);
			}
			dictionary[material]++;
		}
		BaseObject component = ((Component)this).GetComponent<BaseObject>();
		Construction_CAMPFIRE construction_CAMPFIRE = component as Construction_CAMPFIRE;
		Constructing_FUEL_STILL_BOILER constructing_FUEL_STILL_BOILER = component as Constructing_FUEL_STILL_BOILER;
		Constructing_STILL constructing_STILL = component as Constructing_STILL;
		foreach (uint key in dictionary.Keys)
		{
			float num = player.PlayerSkills.CraftsmanshipMultiplier * (float)dictionary[key];
			if (Object.op_Implicit((Object)(object)construction_CAMPFIRE))
			{
				num *= Mathf.Clamp(((IHaveAttribute)construction_CAMPFIRE).GetValue(), 0.2f, 1f);
			}
			int num2 = Mathf.Max(1, Mathf.CeilToInt(num));
			float num3 = 1f;
			for (int i = 0; i < num2; i++)
			{
				SaveablePrefab saveablePrefab = MultiplayerMng.Instantiate<SaveablePrefab>(key, MiniGuid.New());
				((Component)saveablePrefab).transform.position = ((Component)this).transform.position + Vector3.up * num3 + Random.insideUnitSphere;
				if (Object.op_Implicit((Object)(object)constructing_FUEL_STILL_BOILER) && constructing_FUEL_STILL_BOILER.Collector.Fuel > 0f)
				{
					if (saveablePrefab is InteractiveObject_FUELCAN fuelCan)
					{
						ReplicateRefillFuelCan(fuelCan, constructing_FUEL_STILL_BOILER.Collector.Fuel);
					}
				}
				else if (Object.op_Implicit((Object)(object)constructing_STILL) && saveablePrefab is ConstructionObject_WATERBOTTLE bottle)
				{
					ReplicateRefillBottle(bottle, constructing_STILL.Collector.Water);
				}
			}
		}
	}

	private void ReplicateRefillFuelCan(InteractiveObject_FUELCAN fuelCan, float fuel)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		fuelCan.Refill(fuel);
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			RefillFuelCanMsg refillFuelCanMsg = new RefillFuelCanMsg();
			refillFuelCanMsg.FuelCan = fuelCan;
			refillFuelCanMsg.Fuel = fuel;
			refillFuelCanMsg.Post();
		}
	}

	private void ReplicateRefillBottle(ConstructionObject_WATERBOTTLE bottle, int water)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		RefillBottle(bottle, water);
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			RefillBottleMsg refillBottleMsg = new RefillBottleMsg();
			refillBottleMsg.Bottle = bottle;
			refillBottleMsg.Water = water;
			refillBottleMsg.Post();
		}
	}

	private static void RefillBottle(ConstructionObject_WATERBOTTLE bottle, int water)
	{
		bottle.Servings = ((water != 0) ? Mathf.Min(bottle.Servings + water, bottle.OriginalServings) : 0);
	}

	private void SpawnGib()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(_gib))
		{
			return;
		}
		ISaveablePrefab saveablePrefab = default;
		if (_gib.TryGetComponent<ISaveablePrefab>(ref saveablePrefab))
		{
			if (!saveablePrefab.IsMultiplayerEntity || !MPExtensions.IsClient(Game.Mode))
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(saveablePrefab.PrefabId, ((Component)this).transform.position, ((Component)this).transform.rotation, MiniGuid.NewFrom(((Component)this).transform.position, saveablePrefab.PrefabId, 48879));
			}
		}
		else
		{
			Object.Instantiate<GameObject>(_gib, ((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}
}
