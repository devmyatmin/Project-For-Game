using Beam;
using Beam.Crafting;
using Funlabs;
using UnityEngine;

public class InteractiveObject_MEDICAL : ConstructionObject
{
	private class ReplicatePickupEmptyRefund : I_MultiplayerMessage
	{
		[I_Replicate]
		public IPickupable Refund;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			PickupEmptyRefund(Player, Refund);
		}
	}

	[SerializeField]
	[PrefabIdPicker]
	private uint _refundPrefabId;

	public override float GetHealthRange()
	{
		return base.OriginalDurabilityPoints;
	}

	public override string GetHealthTerm()
	{
		if (!(base.DurabilityPoints > 1f))
		{
			return "ITEM_DISPLAY_HEALTH_DOSE_LABEL";
		}
		return "ITEM_DISPLAY_HEALTH_DOSES_LABEL";
	}

	public override bool ValidatePrimary(IBase obj)
	{
		bool flag = false;
		if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_BANDAGE || base.CraftingType.InteractiveType == InteractiveType.MEDICAL_GAUZE)
		{
			flag = base.Owner.Statistics.RemoveStatusEffect(StatusEffect.BLEEDING);
			if (flag)
			{
				base.Owner.Statistics.AddHealth(200f);
			}
		}
		else if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_ANTIDOTE)
		{
			flag = base.Owner.Statistics.RemoveStatusEffect(StatusEffect.POISON);
		}
		else if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_ALOE_SALVE || base.CraftingType.InteractiveType == InteractiveType.MEDICAL_SUNSCREEN)
		{
			flag = base.Owner.Statistics.ApplyStatusEffect(StatusEffect.SUNBLOCK);
			if (flag)
			{
				Sunburn playerEffect = base.Owner.Statistics.GetPlayerEffect<Sunburn>();
				if (playerEffect != null)
				{
					playerEffect.ResetExposure();
				}
				base.Owner.Statistics.RemoveStatusEffect(StatusEffect.SUNSTROKE);
			}
		}
		else if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_SPLINT)
		{
			flag = base.Owner.Statistics.ApplyStatusEffect(StatusEffect.SPLINT);
			if (flag)
			{
				base.Owner.Statistics.RemoveStatusEffect(StatusEffect.BROKEN_BONES);
			}
		}
		else if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_BREATH_BOOST)
		{
			flag = base.Owner.Statistics.ApplyStatusEffect(StatusEffect.BOOST_BREATH);
		}
		else if (base.CraftingType.InteractiveType == InteractiveType.MEDICAL_REPELLENT)
		{
			flag = base.Owner.Statistics.ApplyStatusEffect(StatusEffect.SHARK_REPELLENT);
			if (flag)
			{
				base.Owner.Statistics.RemoveStatusEffect(StatusEffect.POISON);
			}
		}
		return flag;
	}

	public override void Use()
	{
		base.Use();
		base.DurabilityPoints--;
		if (base.DurabilityPoints <= 0f)
		{
			if (!base.DestroyOnEmpty)
			{
				base.DurabilityPoints = 0f;
				base.CanUse = false;
				base.CanUseOnObjects = false;
				base.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
			}
			else
			{
				Destroy();
			}
		}
	}

	public override void Destroy()
	{
		IPlayer owner = base.Owner;
		base.Destroy();
		if (_refundPrefabId != 0 && !Game.Mode.IsClient())
		{
			ReplicatedPickupEmptyRefund(owner, (IPickupable)MultiplayerMng.Instantiate<SaveablePrefab>(_refundPrefabId, MiniGuid.New()));
		}
	}

	private static void PickupEmptyRefund(IPlayer player, IPickupable refund)
	{
		InteractiveObject_FOOD interactiveObject_FOOD = refund as InteractiveObject_FOOD;
		if (refund != null)
		{
			interactiveObject_FOOD.Servings = 0;
		}
		player.Holder.Pickup(refund, animate: false);
	}

	private static void ReplicatedPickupEmptyRefund(IPlayer player, IPickupable refund)
	{
		PickupEmptyRefund(player, refund);
		if (Game.Mode.IsMultiplayer())
		{
			ReplicatePickupEmptyRefund replicatePickupEmptyRefund = new ReplicatePickupEmptyRefund();
			replicatePickupEmptyRefund.Player = player;
			replicatePickupEmptyRefund.Refund = refund;
			replicatePickupEmptyRefund.Post();
		}
	}
}
