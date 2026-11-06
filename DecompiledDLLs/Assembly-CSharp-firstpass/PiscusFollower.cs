using System;
using Beam;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

public class PiscusFollower : Singleton<PiscusFollower>, IPersistentSaveable, ISaveable
{
	[Serializable]
	public class CreatureToSpawn
	{
		public GameObject CreatureObj;

		public float Rarity;
	}

	private class PlayerGridGroup
	{
		private int PlayerId;

		public SpawnGrid[,] SpawnGrids;

		public IPlayer Player => PlayerUtilities.GetPlayerFromId(PlayerId);

		public PlayerGridGroup(int playerId)
		{
			PlayerId = playerId;
		}
	}

	private class SpawnGrid
	{
		public Transform Grid;

		public PiscusManager piscusManager;

		public bool CanGenerate = true;

		public bool Generated;

		public SpawnGrid(Transform grid)
		{
			Grid = grid;
		}
	}

	private const int GRID_DIMENSION = 250;

	private const int GRID_SIZE = 3;

	private Vector3[] _loadedCenterGridPos = new Vector3[2];

	public CreatureToSpawn[] Creatures;

	private PlayerGridGroup[] _playerGridGroup;

	private bool _removedWildlifeSetting;

	private void Start()
	{
		if (!LevelLoader.IsLoading() && Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance))
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated += CreateGrid;
		}
	}

	private void SetDifficultyParams()
	{
		_removedWildlifeSetting = PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Removed;
	}

	protected override void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance))
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated -= CreateGrid;
		}
		base.OnDestroy();
	}

	private void CreateGrid()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			return;
		}
		SetDifficultyParams();
		int count = PlayerRegistry.AllPlayers.Count;
		_playerGridGroup = new PlayerGridGroup[count];
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			_ = PlayerRegistry.AllPlayers[i];
			_playerGridGroup[i] = new PlayerGridGroup(i);
		}
		for (int j = 0; j < _playerGridGroup.Length; j++)
		{
			PlayerGridGroup playerGridGroup = _playerGridGroup[j];
			playerGridGroup.SpawnGrids = new SpawnGrid[3, 3];
			Transform transform = new GameObject("PlayerGrid_" + j).transform;
			((Component)transform).transform.parent = ((Component)this).transform;
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					Transform transform2 = new GameObject("Spawn_Grid_" + k + "_" + l).transform;
					transform2.parent = transform;
					((Component)transform2).gameObject.layer = ((Component)this).gameObject.layer;
					playerGridGroup.SpawnGrids[k, l] = new SpawnGrid(transform2);
					playerGridGroup.SpawnGrids[k, l].CanGenerate = true;
				}
			}
			Vector3 position = _loadedCenterGridPos[j];
			playerGridGroup.SpawnGrids[1, 1].Grid.position = position;
		}
		((MonoBehaviour)this).InvokeRepeating("MoveGrid", 0f, 1f);
		UpdateGridPositions();
	}

	private void MoveGrid()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		for (int i = 0; i < _playerGridGroup.Length; i++)
		{
			PlayerGridGroup playerGridGroup = _playerGridGroup[i];
			if (playerGridGroup.Player != null && playerGridGroup.SpawnGrids[2, 2] == null)
			{
				return;
			}
		}
		for (int j = 0; j < _playerGridGroup.Length; j++)
		{
			PlayerGridGroup playerGridGroup2 = _playerGridGroup[j];
			if (playerGridGroup2.Player == null)
			{
				continue;
			}
			Vector3 position = playerGridGroup2.Player.transform.position;
			SpawnGrid spawnGrid = null;
			int num = 0;
			int num2 = 0;
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					Vector3 position2 = playerGridGroup2.SpawnGrids[k, l].Grid.position;
					if (position.x >= position2.x - 125f && position.x <= position2.x + 125f && position.z >= position2.z - 125f && position.z <= position2.z + 125f)
					{
						spawnGrid = playerGridGroup2.SpawnGrids[k, l];
						num = 1 - k;
						num2 = 1 - l;
						break;
					}
				}
			}
			if (spawnGrid == playerGridGroup2.SpawnGrids[1, 1])
			{
				continue;
			}
			SpawnGrid[,] array = new SpawnGrid[3, 3];
			for (int m = 0; m < 3; m++)
			{
				for (int n = 0; n < 3; n++)
				{
					int num3 = m + num;
					int num4 = n + num2;
					if (num3 < 0 || num3 > 2 || num4 < 0 || num4 > 2)
					{
						if (Object.op_Implicit((Object)(object)playerGridGroup2.SpawnGrids[m, n].piscusManager))
						{
							Object.Destroy((Object)(object)((Component)playerGridGroup2.SpawnGrids[m, n].piscusManager).gameObject);
						}
						playerGridGroup2.SpawnGrids[m, n].CanGenerate = true;
						playerGridGroup2.SpawnGrids[m, n].Generated = false;
					}
					if (num3 < 0)
					{
						num3 = 2;
					}
					else if (num3 > 2)
					{
						num3 = 0;
					}
					if (num4 < 0)
					{
						num4 = 2;
					}
					else if (num4 > 2)
					{
						num4 = 0;
					}
					array[num3, num4] = playerGridGroup2.SpawnGrids[m, n];
					((Object)((Component)playerGridGroup2.SpawnGrids[m, n].Grid).gameObject).name = "Spawn_Grid_" + num3 + "_" + num4;
				}
			}
			playerGridGroup2.SpawnGrids = array;
			UpdateGridPositions();
		}
	}

	private void UpdateGridPositions()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _playerGridGroup.Length; i++)
		{
			PlayerGridGroup playerGridGroup = _playerGridGroup[i];
			if (playerGridGroup.Player == null)
			{
				continue;
			}
			playerGridGroup.SpawnGrids[0, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + 250f);
			playerGridGroup.SpawnGrids[0, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
			playerGridGroup.SpawnGrids[0, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x - 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - 250f);
			playerGridGroup.SpawnGrids[1, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + 250f);
			playerGridGroup.SpawnGrids[1, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - 250f);
			playerGridGroup.SpawnGrids[2, 0].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z + 250f);
			playerGridGroup.SpawnGrids[2, 1].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z);
			playerGridGroup.SpawnGrids[2, 2].Grid.position = new Vector3(playerGridGroup.SpawnGrids[1, 1].Grid.position.x + 250f, ((Component)this).transform.position.y, playerGridGroup.SpawnGrids[1, 1].Grid.position.z - 250f);
			if (_removedWildlifeSetting)
			{
				break;
			}
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					SpawnGrid spawnGrid = playerGridGroup.SpawnGrids[j, k];
					if (spawnGrid.CanGenerate)
					{
						CreateGridObjects(spawnGrid);
					}
				}
			}
		}
	}

	private void CreateGridObjects(SpawnGrid grid)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Zone[] zones = Singleton<StrandedWorld>.Instance.Zones;
		Vector3 val;
		foreach (Zone zone in zones)
		{
			val = grid.Grid.position - ((Component)zone).transform.position;
			if (val.magnitude < 150f || grid.Grid.position.x > 2500f || grid.Grid.position.x < -2500f || grid.Grid.position.z > 2500f || grid.Grid.position.z < -2500f)
			{
				return;
			}
		}
		for (int j = 0; j < _playerGridGroup.Length; j++)
		{
			PlayerGridGroup playerGridGroup = _playerGridGroup[j];
			if (playerGridGroup.Player == null)
			{
				continue;
			}
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					val = grid.Grid.position - ((Component)playerGridGroup.SpawnGrids[k, l].Grid).transform.position;
					if (val.magnitude < 10f && playerGridGroup.SpawnGrids[k, l].Generated)
					{
						return;
					}
				}
			}
		}
		grid.CanGenerate = false;
		grid.Generated = true;
		GameObject val2 = ChooseCreature();
		if (!((Object)(object)val2 == (Object)null))
		{
			GameObject val3 = Object.Instantiate<GameObject>(val2, grid.Grid.position, Quaternion.identity);
			grid.piscusManager = val3.GetComponent<PiscusManager>();
			grid.piscusManager.SpawnDistance = 150f;
			val3.transform.parent = ((Component)grid.Grid).transform;
			((Component)grid.piscusManager).transform.position = grid.Grid.position;
		}
	}

	private GameObject ChooseCreature()
	{
		float num = 0f;
		CreatureToSpawn[] creatures = Creatures;
		foreach (CreatureToSpawn creatureToSpawn in creatures)
		{
			num += creatureToSpawn.Rarity;
		}
		float num2 = Random.value * num;
		for (int j = 0; j < Creatures.Length; j++)
		{
			if (num2 < Creatures[j].Rarity)
			{
				return Creatures[j].CreatureObj;
			}
			num2 -= Creatures[j].Rarity;
		}
		return null;
	}

	public JObject Save()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = new JObject();
		for (int i = 0; i < _playerGridGroup.Length; i++)
		{
			PlayerGridGroup playerGridGroup = _playerGridGroup[i];
			jObject.AddField("CenterGridPos_" + i, JSerializer.Serialize((object)playerGridGroup.SpawnGrids[1, 1].Grid.position));
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		int playersCount = MPExtensions.GetPlayersCount(Game.Mode);
		_loadedCenterGridPos = new Vector3[playersCount];
		if (data != null && !data.IsNull())
		{
			JObject field = data.GetField("CenterGridPos");
			if (field != null)
			{
				Vector3 val = JSerializer.Deserialize<Vector3>(field);
				for (int i = 0; i < playersCount; i++)
				{
					_loadedCenterGridPos[i] = val;
				}
			}
			for (int j = 0; j < playersCount; j++)
			{
				JObject field2 = data.GetField("CenterGridPos_" + j);
				if (field2 != null)
				{
					_loadedCenterGridPos[j] = JSerializer.Deserialize<Vector3>(field2);
				}
			}
		}
		if (Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance))
		{
			Singleton<StrandedWorld>.Instance.WorldGenerated += CreateGrid;
		}
		CreateGrid();
	}
}
