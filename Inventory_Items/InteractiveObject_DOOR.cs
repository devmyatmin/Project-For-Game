using System.Runtime.CompilerServices;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class InteractiveObject_DOOR : InteractiveObject, IDoor, IBase, IHasCraftingType
{
	[SerializeField]
	private Door[] _doors;

	private bool _firstOpen = true;

	[HideInInspector]
	public BaseActionUnityEvent Opened;

	public Door[] Doors => _doors;

	public bool FirstOpen
	{
		get
		{
			return _firstOpen;
		}
		set
		{
			_firstOpen = value;
		}
	}

	public bool IsOpened { get; set; }

	public bool IsOpening { get; set; }

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!IsOpened)
		{
			return "ITEM_INTERACTION_DESCRIPTION_OPEN";
		}
		return "ITEM_INTERACTION_DESCRIPTION_CLOSE";
	}

	public override bool Interact(IPlayer player)
	{
		this.Open();
		Opened?.Invoke(this, BaseActionEventData.Empty);
		return true;
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("firstOpen", JSerializer.Serialize(FirstOpen));
		jObject.AddField("opened", JSerializer.Serialize(IsOpened));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		bool value = data.GetField("firstOpen").GetValue<bool>();
		_firstOpen = value;
		if (data.GetField("opened").GetValue<bool>())
		{
			this.Open();
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
