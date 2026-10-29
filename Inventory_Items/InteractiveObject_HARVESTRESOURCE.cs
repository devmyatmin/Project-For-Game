using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class InteractiveObject_HARVESTRESOURCE : InteractiveObject
{
	[SerializeField]
	private GameObject[] _resources;

	[SerializeField]
	private Transform[] _resourceSpawns;

	[HideInInspector]
	public BaseActionUnityEvent Destroyed;

	public override void Damage(float damage, IBase damager)
	{
		InteractiveObject interactiveObject = damager as InteractiveObject;
		if (!interactiveObject.IsNullOrDestroyed())
		{
			IPlayer player = PlayerRegistry.AllPlayers[interactiveObject.PreviousOwnerId];
			if (!player.IsNullOrDestroyed())
			{
				damage *= player.PlayerSkills.HarvestingMultiplier;
			}
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		BaseUtilities.DoPlayerDamageExperience_Harvesting(base.OriginalHealthPoints, LastPlayerIdToDamageThis);
		if (Game.Mode.IsMaster())
		{
			for (int i = 0; i < _resources.Length; i++)
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(_resources[i].GetComponent<ISaveablePrefab>().PrefabId, _resourceSpawns[i].position, _resourceSpawns[i].rotation, MiniGuid.New());
			}
		}
		base.ActionEventData.Populate(this, null);
		Destroyed?.Invoke(this, base.ActionEventData);
		base.Destroy();
	}
}
