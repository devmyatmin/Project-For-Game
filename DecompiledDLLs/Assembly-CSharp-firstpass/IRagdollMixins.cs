using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

public static class IRagdollMixins
{
	public static JObject SaveRagdoll(this IRagdoll ragdoll)
	{
		JObject jObject = new JObject();
		for (int i = 0; i < ragdoll.RagdollChildren.Length; i++)
		{
			JObject data = JSerializer.Serialize((Component)(object)ragdoll.RagdollChildren[i].Rigidbody);
			jObject.Add(data);
		}
		return jObject;
	}

	public static void LoadRagdoll(this IRagdoll ragdoll, JObject ragdollData)
	{
		try
		{
			for (int i = 0; i < ragdoll.RagdollChildren.Length; i++)
			{
				JObject data = ragdollData.Children[i];
				RagdollChild ragdollChild = ragdoll.RagdollChildren[i];
				try
				{
					JSerializer.DeserializeInto(data, (Component)(object)((Component)ragdollChild).transform);
				}
				catch (JSerializerException)
				{
					Debug.Log((object)"IRagdollMixins::LoadRagdoll:: Couldn't load transform, trying rigidbody...");
					JSerializer.DeserializeInto(data, (Component)(object)ragdollChild.Rigidbody);
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("IRagdollMixins::LoadRagdoll:: Error loading ragdoll. Object has now been destroyed. \n\nName: " + ((Object)ragdoll.gameObject).name + "\nReference Id: " + ragdoll.ReferenceId.ToString() + "\nMessage: " + ex2));
			BeamTeam.Destroy<GameObject>(ragdoll.gameObject);
		}
	}
}
