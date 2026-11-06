using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public static class IPhysicalMixins
{
	public static void InitializeKinematic(this IPhysical physical, bool isKinematic, bool isOwner)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(physical) && !((Object)(object)physical.rigidbody == (Object)null))
		{
			if (LevelLoader.IsLoading(physical.gameObject))
			{
				physical.rigidbody.velocity = Vector3.zero;
				physical.SetKinematic(kinematic: true, isOwner);
			}
			else
			{
				physical.SetKinematic(isKinematic, isOwner);
			}
		}
	}

	public static void SetKinematic(this IPhysical physical, bool kinematic, bool isOwner = true)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(physical) && !((Object)(object)physical.rigidbody == (Object)null))
		{
			kinematic |= !isOwner;
			if (kinematic && (int)physical.rigidbody.collisionDetectionMode != 0)
			{
				physical.rigidbody.collisionDetectionMode = (CollisionDetectionMode)3;
			}
			physical.rigidbody.useGravity = !kinematic;
			physical.rigidbody.isKinematic = kinematic;
			if (!kinematic)
			{
				physical.rigidbody.collisionDetectionMode = physical.CollisionDetectionMode;
			}
		}
	}

	public static void LoadRigidbody(this IPhysical physical, JObject data)
	{
		if (!BeamTeam.IsNullOrDestroyed(physical) && !((Object)(object)physical.rigidbody == (Object)null))
		{
			JObject field = data.GetField("Rigidbody");
			if (field != null)
			{
				JSerializer.DeserializeInto(field, (Component)(object)physical.rigidbody);
			}
		}
	}

	public static void CheckRigidbody(this IPhysical physical, bool IsOwner)
	{
		if (!BeamTeam.IsNullOrDestroyed(physical) && !((Object)(object)physical.rigidbody != (Object)null))
		{
			Rigidbody val = physical.gameObject.AddComponent<Rigidbody>();
			if (!IsOwner)
			{
				val.isKinematic = true;
			}
		}
	}

	public static void RemoveRigidbody(this IPhysical physical, bool IsOwner, bool immediate = false)
	{
		if (!BeamTeam.IsNullOrDestroyed(physical) && !((Object)(object)physical.rigidbody == (Object)null))
		{
			if (immediate)
			{
				BeamTeam.DestroyImmediate<Rigidbody>(physical.rigidbody, false);
			}
			else
			{
				BeamTeam.Destroy<Rigidbody>(physical.rigidbody);
			}
		}
	}
}
