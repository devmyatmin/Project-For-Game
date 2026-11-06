using Beam.Serialization.Json;

namespace Beam.Crafting;

public class InteractiveObject_AIRTANK : InteractiveObject
{
	private void CheckDisplayName()
	{
		if (DurabilityPoints <= 0f)
		{
			DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
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
		if (DurabilityPoints > 0f && Owner.Statistics.Oxygen < Owner.Statistics.MaxOxygen && Owner.Movement.InWater)
		{
			return !Owner.Movement.isFloating;
		}
		return false;
	}

	public override void Use()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		DurabilityPoints--;
		if (DurabilityPoints <= 0f)
		{
			DurabilityPoints = 0f;
		}
		Owner.Statistics.AddOxygen(Owner.Statistics.MaxOxygen);
		ParticleManager.CreateParticle(Singleton<ParticleManager>.Instance.bubbleBurst, transform.position);
		base.Use();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CanUse = true;
	}
}
