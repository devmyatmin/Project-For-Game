using System;
using Beam;
using UnityEngine;

public class PiscusGenerator : MonoBehaviour, IGenerationObject
{
	[Serializable]
	private class spawnPoints
	{
		public GameObject[] objectsToSpawn;

		public Vector3 localPosition;
	}

	private bool _removedWildlifeSetting;

	[SerializeField]
	private spawnPoints[] points;

	private void Start()
	{
		SetDifficultyParams();
		Initialize();
	}

	private void SetDifficultyParams()
	{
		_removedWildlifeSetting = PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Removed;
	}

	private void Initialize()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!_removedWildlifeSetting)
		{
			spawnPoints[] array = points;
			foreach (spawnPoints spawnPoints2 in array)
			{
				GameObject val = Object.Instantiate<GameObject>(spawnPoints2.objectsToSpawn[Random.Range(0, spawnPoints2.objectsToSpawn.Length)], spawnPoints2.localPosition, Quaternion.identity);
				val.transform.parent = ((Component)this).transform.parent;
				val.transform.localPosition = spawnPoints2.localPosition;
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
