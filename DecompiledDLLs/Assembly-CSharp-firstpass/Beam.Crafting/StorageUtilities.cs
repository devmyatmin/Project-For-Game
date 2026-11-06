using System.Collections.Generic;
using Beam.Serialization;
using Beam.Serialization.Json;
using Funlabs;
using MEC;

namespace Beam.Crafting;

public static class StorageUtilities
{
	public static void LoadStorage(SlotStorage storage, JObject data)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Load_Sequence(storage, data));
	}

	private static IEnumerator<float> Load_Sequence(SlotStorage storage, JObject data)
	{
		while (LevelLoader.IsLoading())
		{
			yield return 0f;
		}
		StorageLoader storageLoader = new StorageLoader
		{
			IsLoading = true
		};
		LevelLoader.CurrentLoader = storageLoader;
		storage.Load(data);
		if (MPExtensions.IsClient(Game.Mode))
		{
			while (!MultiplayerZoneSync.Instance.IsLinked())
			{
				yield return 0f;
			}
		}
		for (int i = 0; i < storageLoader.DeferredLoading.Count; i++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.DeferredLoading[i]);
		}
		for (int j = 0; j < storageLoader.PostLoading.Count; j++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.PostLoading[j]);
		}
		for (int k = 0; k < storageLoader.PhysicalLoading.Count; k++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.PhysicalLoading[k]);
		}
		storageLoader.IsLoading = false;
		LevelLoader.CurrentLoader = null;
	}
}
