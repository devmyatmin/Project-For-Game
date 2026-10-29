using Beam.Serialization.Json;

namespace Beam.Crafting;

public class InteractiveObject_AIRTANK : InteractiveObject
{
	private void CheckDisplayName()
	{
		if (base.DurabilityPoints <= 0f)
		{
			base.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
		}
	}

	public override string GetDisplayName()
	{
		CheckDisplayName();
		return base.GetDisplayName();
	}

	private void Selecting_Selected(IPlayer player)
	{
		CheckDisplayName();
	}

	public override bool ValidatePrimary(IBase obj)
	{
		if (base.DurabilityPoints > 0f && base.Owner.Statistics.Oxygen < base.Owner.Statistics.MaxOxygen && base.Owner.Movement.InWater)
		{
			return !base.Owner.Movement.isFloating;
		}
		return false;
	}

	public override void Use()
	{
		base.DurabilityPoints--;
		if (base.DurabilityPoints <= 0f)
		{
			base.DurabilityPoints = 0f;
		}
		base.Owner.Statistics.AddOxygen(base.Owner.Statistics.MaxOxygen);
		ParticleManager.CreateParticle(Singleton<ParticleManager>.Instance.bubbleBurst, base.transform.position);
		base.Use();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		base.CanUse = true;
	}
}
