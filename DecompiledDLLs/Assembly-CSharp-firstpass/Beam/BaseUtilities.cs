using System;
using Beam.Events;
using Beam.Utilities;

namespace Beam;

public static class BaseUtilities
{
	public static void DoPlayerDamageExperience_Hunting(int playerId)
	{
		EventManager.RaiseEvent(new ExperienceEvent(playerId, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.ANIMAL_HIT));
	}

	public static void DoPlayerDamageExperience_Hunting(IPickupable damager)
	{
		if (!BeamTeam.IsNullOrDestroyed(damager))
		{
			DoPlayerDamageExperience_Hunting(damager.PreviousOwnerId);
		}
	}

	public static void DoPlayerDamageExperience_Harvesting(float exp, int playerId)
	{
		EventManager.RaiseEvent(new ExperienceEvent(playerId, PlayerSkills.Skills.HARVESTING, exp * 0.5f));
	}

	public static void DoPlayerDamageExperience_Harvesting(float exp, IPickupable damager)
	{
		if (!BeamTeam.IsNullOrDestroyed(damager))
		{
			DoPlayerDamageExperience_Harvesting(exp, damager.PreviousOwnerId);
		}
	}

	public static void DoPlayerDamageMultipliers_Hunting(ref float damage, IPickupable damager)
	{
		DoPlayerDamageMultipliers(ref damage, damager, DoHuntingMultiplier);
	}

	public static void DoPlayerDamageMultipliers_Harvesting(ref float damage, IPickupable damager)
	{
		DoPlayerDamageMultipliers(ref damage, damager, DoHarvestingMultiplier);
	}

	private static void DoPlayerDamageMultipliers(ref float damage, IPickupable damager, Func<float, IPlayer, float> callback)
	{
		if (!BeamTeam.IsNullOrDestroyed(damager) && callback != null)
		{
			IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(damager.PreviousOwnerId);
			if (!BeamTeam.IsNullOrDestroyed(playerFromId))
			{
				damage = callback(damage, playerFromId);
			}
		}
	}

	private static float DoHuntingMultiplier(float damage, IPlayer player)
	{
		return damage * player.PlayerSkills.HuntingMultiplier;
	}

	private static float DoHarvestingMultiplier(float damage, IPlayer player)
	{
		return damage * player.PlayerSkills.HarvestingMultiplier;
	}
}
