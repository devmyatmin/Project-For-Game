using System.Runtime.CompilerServices;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class Interactive_FISH_RAGDOLL : Interactive_FISH, IRagdoll, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable
{
	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private RagdollChild[] _ragdollChildren;

	public RagdollChild[] RagdollChildren => _ragdollChildren;

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = this.SaveRagdoll();
		jObject.AddField("Ragdolls", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject ragdollData = data.GetField("Ragdolls");
		if (ragdollData != null)
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(delegate
			{
				this.LoadRagdoll(ragdollData);
			});
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
