using Beam;
using UnityEngine;

public class PlayerDetailSpawnerManager : MonoBehaviour
{
	public CellObjectSpawner Detail_Spawner;

	public CellObjectSpawner Fish_Spawner;

	public CellObjectSpawner FishMid_Spawner;

	private void Start()
	{
		if ((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null)
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated += StrandedWorld_WorldGenerated;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null)
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
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		CellObjectSpawner cellObjectSpawner = Object.Instantiate<CellObjectSpawner>(spawner);
		((Component)cellObjectSpawner).transform.SetParent(((Component)this).transform, false);
		((Component)cellObjectSpawner).transform.localPosition = Vector3.zero;
		((Component)cellObjectSpawner).transform.localRotation = Quaternion.identity;
		cellObjectSpawner.Initialize(player.transform);
	}
}
