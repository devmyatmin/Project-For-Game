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
		Timing.RunCoroutine(Load_Sequence(storage, data));
	}

	private static IEnumerator<float> Load_Sequence(SlotStorage storage, JObject data)
	{
		while (LevelLoader.IsLoading())
		{
			yield return 0f;
		}
		StorageLoader storageLoader = (StorageLoader)(LevelLoader.CurrentLoader = new StorageLoader
		{
			IsLoading = true
		});
		storage.Load(data);
		if (Game.Mode.IsClient())
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
