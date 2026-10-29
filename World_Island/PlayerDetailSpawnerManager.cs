using Beam;
using UnityEngine;

public class PlayerDetailSpawnerManager : MonoBehaviour
{
	public CellObjectSpawner Detail_Spawner;

	public CellObjectSpawner Fish_Spawner;

	public CellObjectSpawner FishMid_Spawner;

	private void Start()
	{
		if (Singleton<StrandedWorld>.Instance != null)
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated += StrandedWorld_WorldGenerated;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<StrandedWorld>.Instance != null)
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated -= StrandedWorld_WorldGenerated;
		}
	}

	public void StrandedWorld_WorldGenerated()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			CreateSpawner(Detail_Spawner, allPlayer);
			CreateSpawner(Fish_Spawner, allPlayer);
			CreateSpawner(FishMid_Spawner, allPlayer);
		}
	}

	private void CreateSpawner(CellObjectSpawner spawner, IPlayer player)
	{
		CellObjectSpawner cellObjectSpawner = Object.Instantiate(spawner);
		cellObjectSpawner.transform.SetParent(base.transform, worldPositionStays: false);
		cellObjectSpawner.transform.localPosition = Vector3.zero;
		cellObjectSpawner.transform.localRotation = Quaternion.identity;
		cellObjectSpawner.Initialize(player.transform);
	}
}
