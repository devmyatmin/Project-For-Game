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
		return OriginalDurabilityPoints;
	}

	public override string GetHealthTerm()
	{
		if (!(DurabilityPoints > 1f))
		{
			return "ITEM_DISPLAY_HEALTH_DOSE_LABEL";
		}
		return "ITEM_DISPLAY_HEALTH_DOSES_LABEL";
	}

	public override bool ValidatePrimary(IBase obj)
	{
		bool flag = false;
		if (CraftingType.InteractiveType == InteractiveType.MEDICAL_BANDAGE || CraftingType.InteractiveType == InteractiveType.MEDICAL_GAUZE)
		{
			flag = Owner.Statistics.RemoveStatusEffect(StatusEffect.BLEEDING);
			if (flag)
			{
				Owner.Statistics.AddHealth(200f);
			}
		}
		else if (CraftingType.InteractiveType == InteractiveType.MEDICAL_ANTIDOTE)
		{
			flag = Owner.Statistics.RemoveStatusEffect(StatusEffect.POISON);
		}
		else if (CraftingType.InteractiveType == InteractiveType.MEDICAL_ALOE_SALVE || CraftingType.InteractiveType == InteractiveType.MEDICAL_SUNSCREEN)
		{
			flag = Owner.Statistics.ApplyStatusEffect(StatusEffect.SUNBLOCK);
			if (flag)
			{
				Sunburn playerEffect = Owner.Statistics.GetPlayerEffect<Sunburn>();
				if ((Object)(object)playerEffect != (Object)null)
				{
					playerEffect.ResetExposure();
				}
				Owner.Statistics.RemoveStatusEffect(StatusEffect.SUNSTROKE);
			}
		}
		else if (CraftingType.InteractiveType == InteractiveType.MEDICAL_SPLINT)
		{
			flag = Owner.Statistics.ApplyStatusEffect(StatusEffect.SPLINT);
			if (flag)
			{
				Owner.Statistics.RemoveStatusEffect(StatusEffect.BROKEN_BONES);
			}
		}
		else if (CraftingType.InteractiveType == InteractiveType.MEDICAL_BREATH_BOOST)
		{
			flag = Owner.Statistics.ApplyStatusEffect(StatusEffect.BOOST_BREATH);
		}
		else if (CraftingType.InteractiveType == InteractiveType.MEDICAL_REPELLENT)
		{
			flag = Owner.Statistics.ApplyStatusEffect(StatusEffect.SHARK_REPELLENT);
			if (flag)
			{
				Owner.Statistics.RemoveStatusEffect(StatusEffect.POISON);
			}
		}
		return flag;
	}

	public override void Use()
	{
		base.Use();
		DurabilityPoints--;
		if (DurabilityPoints <= 0f)
		{
			if (!DestroyOnEmpty)
			{
				DurabilityPoints = 0f;
				CanUse = false;
				CanUseOnObjects = false;
				DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
			}
			else
			{
				Destroy();
			}
		}
	}

	public override void Destroy()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		IPlayer owner = Owner;
		base.Destroy();
		if (_refundPrefabId != 0 && !MPExtensions.IsClient(Game.Mode))
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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		PickupEmptyRefund(player, refund);
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicatePickupEmptyRefund replicatePickupEmptyRefund = new ReplicatePickupEmptyRefund();
			replicatePickupEmptyRefund.Player = player;
			replicatePickupEmptyRefund.Refund = refund;
			replicatePickupEmptyRefund.Post();
		}
	}
}
