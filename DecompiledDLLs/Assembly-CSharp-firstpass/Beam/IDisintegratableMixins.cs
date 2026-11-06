using System.Collections.Generic;
using Beam.Utilities;
using MEC;
using UnityEngine;

namespace Beam;

public static class IDisintegratableMixins
{
	private const float DISINTEGRATE_TIME = 3f;

	public static void Disintegrate(this IDisintegratable disintegratable)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		disintegratable.Disintegrating = true;
		BaseExtensions.SetLayerRecursively(disintegratable.transform, Layers.IGNORE_RAYCAST, (1 << Layers.PROJECTILES) | (1 << Layers.PROJECTILE_HITBOXES));
		Beam.Utilities.ExtensionMethods.ForEachChild(disintegratable.transform, (Transform t) =>
		{
			Collider val = default;
			if (((Component)t).TryGetComponent<Collider>(ref val))
			{
				if (((Component)t).gameObject.layer != Layers.PROJECTILE_HITBOXES)
				{
					foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
					{
						if (!BeamTeam.IsNullOrDestroyed(allPlayer))
						{
							Physics.IgnoreCollision((Collider)(object)allPlayer.Movement.CharacterController, val, true);
						}
					}
					return;
				}
				val.enabled = false;
			}
		});
		Timing.RunCoroutine(Disintegrate_Sequence(disintegratable));
	}

	private static IEnumerator<float> Disintegrate_Sequence(IDisintegratable disintegratable)
	{
		float timer = 0f;
		float rate = 1f / 3f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			Stipple(disintegratable, Mathf.Clamp01(timer * rate));
			yield return 0f;
		}
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(disintegratable.transform);
		disintegratable.Destroy();
	}

	private static void Stipple(IDisintegratable disintegratable, float value)
	{
		for (int i = 0; i < disintegratable.renderers.Length; i++)
		{
			for (int j = 0; j < disintegratable.renderers[i].materials.Length; j++)
			{
				disintegratable.renderers[i].materials[j].SetFloat("Beam_LODFade", value);
			}
		}
	}
}
