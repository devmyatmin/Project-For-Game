using System;
using System.Collections.Generic;
using Beam;
using Beam.Events;
using MEC;
using SharpNeatLib.Maths;
using UnityEngine;

public class FollowSpawn : CellObjectSpawner, IHashCollection
{
	private class SpawnGrid
	{
		public Transform Transform;

		public Transform Objects;

		public bool CanGenerate = true;

		public SpawnGrid(Transform transform)
		{
			Transform = transform;
		}
	}

	[Serializable]
	public class BiomeGrid
	{
		public string name;

		public bool allBiomes;

		public Zone.BiomeType _biome;

		public GridObject[] biomeParameters;
	}

	[Serializable]
	public class GridObject
	{
		public GameObject[] objectsToSpawn;

		public LayerMask rayCastMasks;

		public bool cycleGrid;

		public int gridCheckDensity = 1;

		public int maxObjects = 1;

		public int rarity;

		public float align = 0.5f;

		public float minslope;

		public float maxslope = 90f;

		public float minHeight;

		public float maxHeight = 50f;

		public bool randomYRotation;

		public bool grass;
	}

	public static int GRID_DIMENSION = 20;

	private const int GRID_SIZE = 3;

	private const int MAX_SAMPLES_PER_FRAME = 400;

	private SpawnGrid[][,] _spawnGridsBuffers;

	[SerializeField]
	private BiomeGrid[] GridObjects2;

	private IPointSampler _sampler;

	private FastRandom _random = new FastRandom();

	private CoroutineHandle _masterUpdateCoroutine;

	private CoroutineHandle _gridUpdateCoroutine;

	private int _samplesCounter;

	private bool _grass = true;

	private SpawnGrid[,] _spawnGrids
	{
		get
		{
			return _spawnGridsBuffers[0];
		}
		set
		{
			_spawnGridsBuffers[0] = value;
		}
	}

	public bool IsGenerating
	{
		get
		{
			if (_masterUpdateCoroutine.IsValid)
			{
				return _masterUpdateCoroutine.IsRunning;
			}
			return false;
		}
	}

	private void Awake()
	{
		_sampler = new ZoneTerrainPointSampler();
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		LoadOptions();
	}

	public override void Initialize(Transform follow)
	{
		base.Initialize(follow);
		CreateGrid();
	}

	protected override void OnDestroy()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroy();
		HashCollectionCache.Remove(this);
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		Timing.KillCoroutines(_gridUpdateCoroutine);
		Timing.KillCoroutines(_masterUpdateCoroutine);
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void LoadOptions()
	{
		_grass = Options.GraphicsSettings.Grass == Quality_Grass.ON;
	}

	private void CreateGrid()
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		_spawnGridsBuffers = new SpawnGrid[2][,];
		_spawnGridsBuffers[0] = new SpawnGrid[3, 3];
		_spawnGridsBuffers[1] = new SpawnGrid[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Transform transform = new GameObject("Spawn_Grid_" + i + "_" + j).transform;
				((Component)transform).gameObject.layer = ((Component)this).gameObject.layer;
				((Component)transform).transform.parent = ((Component)this).transform;
				_spawnGrids[i, j] = new SpawnGrid(transform);
				new GameObject(((Object)transform).name + "_Combined").transform.parent = ((Component)transform).transform;
				Transform transform2 = new GameObject(((Object)transform).name + "_Objects").transform;
				transform2.parent = ((Component)transform).transform;
				_spawnGrids[i, j].Objects = transform2;
				_spawnGrids[i, j].CanGenerate = true;
			}
		}
		Vector3 position = new Vector3((float)(Mathf.RoundToInt(Following.position.x / (float)GRID_DIMENSION) * GRID_DIMENSION), 0f, (float)(Mathf.RoundToInt(Following.position.z / (float)GRID_DIMENSION) * GRID_DIMENSION));
		_spawnGrids[1, 1].Transform.position = position;
		UpdateGridPositions();
		UpdateGridObjects();
		((MonoBehaviour)this).InvokeRepeating("CheckFollowingPosition", 1f, 0.5f);
		HashCollectionCache.Add(this);
	}

	private void CheckFollowingPosition()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (IsGenerating || (Object)(object)Following == (Object)null || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		SpawnGrid spawnGrid = null;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Vector3 position = _spawnGrids[i, j].Transform.position;
				if (Following.position.x >= position.x - (float)(GRID_DIMENSION / 2) && Following.position.x <= position.x + (float)(GRID_DIMENSION / 2) && Following.position.z >= position.z - (float)(GRID_DIMENSION / 2) && Following.position.z <= position.z + (float)(GRID_DIMENSION / 2))
				{
					spawnGrid = _spawnGrids[i, j];
					num = 1 - i;
					num2 = 1 - j;
					break;
				}
			}
		}
		if (spawnGrid != _spawnGrids[1, 1])
		{
			if (spawnGrid == null)
			{
				Vector3 position2 = new Vector3(Mathf.Round(Following.position.x / (float)GRID_DIMENSION) * (float)GRID_DIMENSION, 0f, Mathf.Round(Following.position.z / (float)GRID_DIMENSION) * (float)GRID_DIMENSION);
				_spawnGrids[1, 1].Transform.position = position2;
				num = 3;
				num2 = 3;
			}
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					int num3 = k + num;
					int num4 = l + num2;
					if (num3 < 0 || num3 > 2 || num4 < 0 || num4 > 2)
					{
						int childCount = _spawnGrids[k, l].Objects.childCount;
						for (int m = 0; m < childCount; m++)
						{
							ObjectPoolManager.DestroyPooled(((Component)_spawnGrids[k, l].Objects.GetChild(0)).gameObject);
						}
						_spawnGrids[k, l].CanGenerate = true;
					}
					num3 = (3 + num3) % 3;
					num4 = (3 + num4) % 3;
					_spawnGridsBuffers[1][num3, num4] = _spawnGrids[k, l];
				}
			}
			SpawnGrid[,] array = _spawnGridsBuffers[0];
			_spawnGridsBuffers[0] = _spawnGridsBuffers[1];
			_spawnGridsBuffers[1] = array;
			UpdateGridPositions();
		}
		UpdateGridObjects();
	}

	private void UpdateGridPositions()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		_spawnGrids[0, 0].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x - (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z + (float)GRID_DIMENSION);
		_spawnGrids[0, 1].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x - (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z);
		_spawnGrids[0, 2].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x - (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z - (float)GRID_DIMENSION);
		_spawnGrids[1, 0].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z + (float)GRID_DIMENSION);
		_spawnGrids[1, 2].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z - (float)GRID_DIMENSION);
		_spawnGrids[2, 0].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x + (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z + (float)GRID_DIMENSION);
		_spawnGrids[2, 1].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x + (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z);
		_spawnGrids[2, 2].Transform.position = new Vector3(_spawnGrids[1, 1].Transform.position.x + (float)GRID_DIMENSION, ((Component)this).transform.position.y, _spawnGrids[1, 1].Transform.position.z - (float)GRID_DIMENSION);
	}

	private void UpdateGridObjects()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_masterUpdateCoroutine = Timing.RunCoroutine(UpdateGridObjects_Sequence(distributed: true));
	}

	private IEnumerator<float> UpdateGridObjects_Sequence(bool distributed)
	{
		_samplesCounter = 0;
		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 3; y++)
			{
				SpawnGrid spawnGrid = _spawnGrids[x, y];
				if (spawnGrid.CanGenerate && !HashCollectionCache.HasHash(this, Vector3Tools.Hash(spawnGrid.Transform.position)))
				{
					_gridUpdateCoroutine = Timing.RunCoroutine(CreateGridObjects_Sequence(spawnGrid, 400));
					yield return Timing.WaitUntilDone(_gridUpdateCoroutine);
				}
			}
		}
	}

	private IEnumerator<float> CreateGridObjects_Sequence(SpawnGrid grid, int samplesPerStep)
	{
		if ((Object)(object)Following == (Object)null)
		{
			yield break;
		}
		grid.CanGenerate = false;
		_random.Reinitialise((int)(grid.Transform.position.x + grid.Transform.position.z));
		Zone.BiomeType currentBiome = StrandedWorld.GetBiome(Following.position);
		if (currentBiome == Zone.BiomeType.DEEP_SEA)
		{
			yield break;
		}
		_sampler.Init(grid.Transform.position);
		BiomeGrid[] gridObjects = GridObjects2;
		foreach (BiomeGrid biomeGrid in gridObjects)
		{
			GridObject[] biomeParameters = biomeGrid.biomeParameters;
			foreach (GridObject gridObject in biomeParameters)
			{
				if ((!_grass && gridObject.grass) || (currentBiome == Zone.BiomeType.MISSION && biomeGrid._biome != Zone.BiomeType.MISSION))
				{
					continue;
				}
				Matrix4x4 worldToLocalMatrix;
				Vector3 val2;
				if (gridObject.cycleGrid)
				{
					int curObjectCount = 0;
					for (int x = 0; x < GRID_DIMENSION; x += gridObject.gridCheckDensity)
					{
						for (int y = 0; y < GRID_DIMENSION; y += gridObject.gridCheckDensity)
						{
							if (curObjectCount >= gridObject.maxObjects)
							{
								continue;
							}
							Vector3 pos = grid.Transform.position + new Vector3((float)(x - GRID_DIMENSION / 2), 50f, (float)(y - GRID_DIMENSION / 2));
							int randRot = _random.Next(360);
							if (_random.Next(100) > gridObject.rarity)
							{
								continue;
							}
							_samplesCounter++;
							if (_samplesCounter % samplesPerStep == 0)
							{
								yield return 0f;
							}
							if (!_sampler.TryGetSample(pos, out var sample))
							{
								continue;
							}
							float num = Mathf.Acos(Vector3.Dot(sample.Normal, Vector3.up)) * 57.29578f;
							Vector3 point = sample.Point;
							if (num >= gridObject.minslope && num <= gridObject.maxslope && point.y >= gridObject.minHeight && point.y <= gridObject.maxHeight)
							{
								worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
								point = worldToLocalMatrix.MultiplyPoint3x4(point);
								worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
								Vector3 val = worldToLocalMatrix.MultiplyVector(sample.Normal);
								val2 = Vector3.Lerp(Vector3.up, val, gridObject.align);
								Vector3 normalized = val2.normalized;
								Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normalized);
								GameObject val3 = ObjectPoolManager.CreatePooled(gridObject.objectsToSpawn[_random.Next(gridObject.objectsToSpawn.Length)], point, rotation);
								if (gridObject.randomYRotation)
								{
									val3.transform.Rotate(Vector3.up * (float)randRot);
								}
								val3.transform.parent = grid.Objects;
								curObjectCount++;
							}
						}
					}
					continue;
				}
				for (int curObjectCount = 0; curObjectCount < gridObject.maxObjects; curObjectCount++)
				{
					Vector3 pos = grid.Transform.position + new Vector3((float)_random.Next(-GRID_DIMENSION / 2, GRID_DIMENSION / 2), 50f, (float)_random.Next(-GRID_DIMENSION / 2, GRID_DIMENSION / 2));
					int x = _random.Next(360);
					if (_random.Next(100) > gridObject.rarity)
					{
						continue;
					}
					_samplesCounter++;
					if (_samplesCounter % samplesPerStep == 0)
					{
						yield return 0f;
					}
					if (!_sampler.TryGetSample(pos, out var sample2))
					{
						continue;
					}
					float num2 = Mathf.Acos(Vector3.Dot(sample2.Normal, Vector3.up)) * 57.29578f;
					Vector3 point2 = sample2.Point;
					if (num2 >= gridObject.minslope && num2 <= gridObject.maxslope && point2.y >= gridObject.minHeight && point2.y <= gridObject.maxHeight)
					{
						worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
						point2 = worldToLocalMatrix.MultiplyPoint3x4(point2);
						worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
						Vector3 val4 = worldToLocalMatrix.MultiplyVector(sample2.Normal);
						val2 = Vector3.Lerp(Vector3.up, val4, gridObject.align);
						Vector3 normalized2 = val2.normalized;
						Quaternion rotation2 = Quaternion.FromToRotation(Vector3.up, normalized2);
						GameObject val5 = ObjectPoolManager.CreatePooled(gridObject.objectsToSpawn[_random.Next(gridObject.objectsToSpawn.Length)], point2, rotation2);
						if (gridObject.randomYRotation)
						{
							val5.transform.Rotate(Vector3.up * (float)x);
						}
						val5.transform.parent = grid.Objects;
					}
				}
			}
		}
	}

	public void ClearPatch(Vector3 position, float radius)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		SpawnGrid[,] spawnGrids = _spawnGrids;
		int upperBound = spawnGrids.GetUpperBound(0);
		int upperBound2 = spawnGrids.GetUpperBound(1);
		for (int i = spawnGrids.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = spawnGrids.GetLowerBound(1); j <= upperBound2; j++)
			{
				foreach (Transform item in spawnGrids[i, j].Transform)
				{
					Transform val = item;
					if (Vector3.Distance(val.position, position) <= radius)
					{
						ObjectPoolManager.DestroyPooled(((Component)val).gameObject);
					}
				}
			}
		}
	}

	bool IHashCollection.HasHash(int hash)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Vector3Tools.Hash(_spawnGrids[i, j].Transform.position) == hash)
				{
					return true;
				}
			}
		}
		return false;
	}
}
