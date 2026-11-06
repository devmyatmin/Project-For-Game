using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

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
		if (!BeamTeam.IsNullOrDestroyed(interactiveObject))
		{
			IPlayer player = PlayerRegistry.AllPlayers[interactiveObject.PreviousOwnerId];
			if (!BeamTeam.IsNullOrDestroyed(player))
			{
				damage *= player.PlayerSkills.HarvestingMultiplier;
			}
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		BaseUtilities.DoPlayerDamageExperience_Harvesting(OriginalHealthPoints, LastPlayerIdToDamageThis);
		if (MPExtensions.IsMaster(Game.Mode))
		{
			for (int i = 0; i < _resources.Length; i++)
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(_resources[i].GetComponent<ISaveablePrefab>().PrefabId, _resourceSpawns[i].position, _resourceSpawns[i].rotation, MiniGuid.New());
			}
		}
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)Destroyed)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		base.Destroy();
	}
}
