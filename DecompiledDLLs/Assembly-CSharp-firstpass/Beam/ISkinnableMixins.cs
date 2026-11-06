using Beam.Serialization.Json;
using UnityEngine.Events;

namespace Beam;

public static class ISkinnableMixins
{
	public static void Skin(this ISkinnable skinnable, IPlayer player)
	{
		skinnable.Disintegrate();
		((UnityEvent<IPlayer>)skinnable.OnSkinned)?.Invoke(player);
	}

	public static JObject SaveSkinnable(this ISkinnable skinnable)
	{
		JObject jObject = new JObject();
		jObject.AddField("CanSkin", skinnable.CanSkin);
		jObject.AddField("Skinned", skinnable.Skinned);
		jObject.AddField("LeatherQuantity", skinnable.LeatherQuantity);
		jObject.AddField("Failed", skinnable.FailedAttempts);
		jObject.AddField("Disintegrating", skinnable.Disintegrating);
		return jObject;
	}

	public static void LoadSkinnable(this ISkinnable skinnable, JObject data)
	{
		JObject field = data.GetField("CanSkin");
		if (field != null)
		{
			skinnable.CanSkin = field.GetValue<bool>();
		}
		JObject field2 = data.GetField("Skinned");
		if (field2 != null)
		{
			skinnable.Skinned = field2.GetValue<int>();
		}
		JObject field3 = data.GetField("LeatherQuantity");
		if (field3 != null)
		{
			skinnable.LeatherQuantity = field3.GetValue<int>();
		}
		JObject field4 = data.GetField("Failed");
		if (field4 != null)
		{
			skinnable.FailedAttempts = field4.GetValue<int>();
		}
		JObject field5 = data.GetField("Disintegrating");
		if (field5 != null)
		{
			skinnable.Disintegrating = field5.GetValue<bool>();
			if (skinnable.Disintegrating)
			{
				skinnable.Destroy();
			}
		}
	}
}
