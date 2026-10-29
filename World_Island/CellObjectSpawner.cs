using System.Collections.Generic;
using Beam.Events;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using SharpNeatLib.Maths;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam;

public abstract class CellObjectSpawner : SerializedMonoBehaviour
{
	private Transform _following;

	protected Transform Following => _following;

	protected T GetFollowing<T>()
	{
		if (_following != null && !_following.gameObject.IsNullOrDestroyed())
		{
			return _following.gameObject.GetComponent<T>();
		}
		return default(T);
	}

	public virtual void Initialize(Transform follow)
	{
		_following = follow;
	}

	protected virtual void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	protected virtual void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		OnPlayerDisconnect(disconnectedEvent);
	}

	protected virtual void OnPlayerDisconnect(PlayerDisconnectedEvent disconnectedEvent)
	{
		IPlayer following = GetFollowing<IPlayer>();
		if (!following.IsNullOrDestroyed() && following.Id == disconnectedEvent.PlayerId)
		{
			_following = null;
		}
	}
}
public class CellObjectSpawner<TCellObject> : CellObjectSpawner, IHashCollection where TCellObject : MonoBehaviour, ICellObject
{
	[Tooltip("Hash offset to separate cell comparisons for spawners that share the same type if needed.")]
	[SerializeField]
	private int _hashOffset;

	private const int CELLS = 3;

	private const int CELL_RESOLUTION = 8;

	[SerializeField]
	private int _cellWidth = 20;

	[SerializeField]
	private ICreateCellObjects<TCellObject> _cellObjectCreator;

	[SerializeField]
	private IFreeCellObjects<TCellObject> _cellObjectDestroyer;

	[SerializeField]
	private List<CellObjectSettings<TCellObject>> _cellObjectsSettings;

	private Dictionary<int, IList<JObject>> _cellObjectsDataCache = new Dictionary<int, IList<JObject>>();

	private Cell<TCellObject>[][] _cellsBuffers;

	private List<Placement> _placements = new List<Placement>();

	private FastRandom _random = new FastRandom();

	public IList<CellObjectSettings<TCellObject>> CellObjectsSettings => _cellObjectsSettings;

	protected float HalfCellWidth => (float)_cellWidth * 0.5f;

	private Cell<TCellObject>[] _cells
	{
		get
		{
			return _cellsBuffers[0];
		}
		set
		{
			_cellsBuffers[0] = value;
		}
	}

	bool IHashCollection.HasHash(int hash)
	{
		for (int i = 0; i < _cells.Length; i++)
		{
			if (_cells[i].Position.Hash() + _hashOffset == hash)
			{
				return true;
			}
		}
		return false;
	}

	public override void Initialize(Transform follow)
	{
		base.Initialize(follow);
		CreateCells();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		HashCollectionCache.Remove(this);
	}

	private void CreateCells()
	{
		_cellsBuffers = new Cell<TCellObject>[2][];
		_cellsBuffers[0] = new Cell<TCellObject>[9];
		_cellsBuffers[1] = new Cell<TCellObject>[9];
		Vector3 vector = new Vector3(Mathf.Round(base.Following.position.x / 16f) * 16f, 0f, Mathf.Round(base.Following.position.z / 16f) * 16f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float num = (float)i - 1.5f;
				float num2 = (float)j - 1.5f;
				Vector3 zero = Vector3.zero;
				zero.x = vector.x + num * (float)_cellWidth + HalfCellWidth;
				zero.z = vector.z + num2 * (float)_cellWidth + HalfCellWidth;
				GameObject gameObject = new GameObject("Cell");
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = zero;
				GameObject gameObject2 = new GameObject("Objects");
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				Cell<TCellObject> cell = new Cell<TCellObject>(gameObject.transform, Vector3.one * _cellWidth);
				cell.ObjectContainer = gameObject2.transform;
				cell.CellObjectFreed += FreeCellObject;
				_cells[j * 3 + i] = cell;
			}
		}
		UpdateCellPositions();
		UpdateCellObjects();
		HashCollectionCache.Add(this);
		InvokeRepeating("UpdateCenterCell", 1f, 0.5f);
	}

	private void UpdateCenterCell()
	{
		if (base.Following.IsNullOrDestroyed() || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		Vector3 position = base.Following.position;
		Cell<TCellObject> cell = null;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Cell<TCellObject> cell2 = _cells[j * 3 + i];
				if (position.x > cell2.Position.x - HalfCellWidth && position.x < cell2.Position.x + HalfCellWidth && position.z > cell2.Position.z - HalfCellWidth && position.z < cell2.Position.z + HalfCellWidth)
				{
					cell = cell2;
					num = 1 - i;
					num2 = 1 - j;
					break;
				}
			}
		}
		int num3 = 4;
		if (cell != _cells[num3])
		{
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					Cell<TCellObject> cell3 = _cells[l * 3 + k];
					int num4 = k + num;
					int num5 = l + num2;
					if (num4 < 0 || num4 > 2 || num5 < 0 || num5 > 2)
					{
						cell3.Generated = false;
						cell3.Loaded = false;
					}
					num4 = (3 + num4) % 3;
					num5 = (3 + num5) % 3;
					_cellsBuffers[1][num5 * 3 + num4] = cell3;
				}
			}
			Cell<TCellObject>[] array = _cellsBuffers[0];
			_cellsBuffers[0] = _cellsBuffers[1];
			_cellsBuffers[1] = array;
		}
		UpdateCellPositions();
		UpdateCellObjects();
	}

	private void UpdateCellPositions()
	{
		int num = 4;
		Cell<TCellObject> cell = _cells[num];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int num2 = j * 3 + i;
				float num3 = (1 - i) * -_cellWidth;
				float num4 = (1 - j) * -_cellWidth;
				_cells[num2].Position = new Vector3(cell.Position.x + num3, 0f, cell.Position.z + num4);
			}
		}
	}

	private void UpdateCellObjects()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int num = j * 3 + i;
				Cell<TCellObject> cell = _cells[num];
				UpdateCellObjects(cell);
				UpdateCellLodLevel(num);
			}
		}
	}

	private void UpdateCellObjects(Cell<TCellObject> cell)
	{
		if (!cell.Generated && !HashCollectionCache.HasHash(this, cell.Position.Hash() + _hashOffset))
		{
			ClearCellObjects(cell);
			CreateCellObjects(cell);
		}
	}

	private void UpdateCellLodLevel(int index)
	{
		int num = 4;
		int lod = ((index != num) ? 1 : 0);
		_cells[index].Lod = lod;
	}

	private void FreeCellObject(TCellObject cellObject)
	{
		_cellObjectDestroyer.FreeCellObject(cellObject);
	}

	private void ClearCellObjects(Cell<TCellObject> cell)
	{
		for (int num = cell.Objects.Count - 1; num >= 0; num--)
		{
			TCellObject cellObject = cell.Objects[num];
			cell.RemoveCellObject(cellObject);
			FreeCellObject(cellObject);
		}
		cell.Objects.Clear();
		cell.ResetLod();
	}

	private void CreateCellObjects(Cell<TCellObject> cell)
	{
		cell.Generated = true;
		_random.Reinitialise(cell.Id);
		for (int i = 0; i < _cellObjectsSettings.Count; i++)
		{
			CellObjectSettings<TCellObject> cellObjectSettings = _cellObjectsSettings[i];
			if (cellObjectSettings.MaximumInstances != 0)
			{
				IList<Placement> list = cellObjectSettings.DistributionType switch
				{
					DistributionType.Even => EvenPlacement(cell.Position, cellObjectSettings), 
					DistributionType.Radial => RadialPlacement(cell.Position, cellObjectSettings), 
					_ => RandomPlacement(cell.Position, cellObjectSettings), 
				};
				for (int j = 0; j < list.Count; j++)
				{
					Placement placement = list[j];
					TCellObject val = _cellObjectCreator.CreateCellObject(cellObjectSettings.Prefab);
					val.transform.parent = cell.ObjectContainer;
					val.transform.position = placement.Position;
					val.transform.rotation = placement.Rotation;
					cell.AddCellObject(val);
				}
			}
		}
	}

	private IList<Placement> RandomPlacement(Vector3 center, CellObjectSettings<TCellObject> settings)
	{
		_placements.Clear();
		for (int i = 0; i < settings.MaximumInstances; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				if ((float)_random.Next(100) > settings.Rarity)
				{
					continue;
				}
				float num = ((!((float)_random.NextDouble() > 0.5f)) ? 1 : (-1));
				float num2 = (float)_random.NextDouble() * settings.XPositionJitter * num;
				float num3 = (float)_random.NextDouble() * settings.ZPositionJitter * num;
				float num4 = settings.XPositionJitter + (float)_random.NextDouble() * ((float)_cellWidth - settings.XPositionJitter);
				float num5 = settings.ZPositionJitter + (float)_random.NextDouble() * ((float)_cellWidth - settings.ZPositionJitter);
				Vector3 zero = Vector3.zero;
				zero.x = center.x - HalfCellWidth + num4 + num2;
				zero.z = center.z - HalfCellWidth + num5 + num3;
				Placement placement = new Placement(zero, Quaternion.identity);
				for (int k = 0; k < settings.PlacementModifiers?.Length; k++)
				{
					settings.PlacementModifiers[k].ModifyPlacement(ref placement, settings);
				}
				bool flag = true;
				for (int l = 0; l < settings.PlacementValidators?.Length; l++)
				{
					if (!settings.PlacementValidators[l].ValidatePlacement(placement, settings))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_placements.Add(placement);
					break;
				}
			}
		}
		return _placements;
	}

	private IList<Placement> RadialPlacement(Vector3 center, CellObjectSettings<TCellObject> settings)
	{
		_placements.Clear();
		for (int i = 0; i < settings.MaximumInstances; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				if ((float)_random.Next(100) > settings.Rarity)
				{
					continue;
				}
				float num = ((!((float)_random.NextDouble() > 0.5f)) ? 1 : (-1));
				Vector3 zero = Vector3.zero;
				zero.x = (float)_random.NextDouble() * num * settings.XPositionJitter;
				zero.z = (float)_random.NextDouble() * num * settings.ZPositionJitter;
				if (zero.sqrMagnitude > BeamMath.Sqr(HalfCellWidth))
				{
					zero.Normalize();
					zero *= HalfCellWidth;
				}
				Placement placement = new Placement(center + zero, Quaternion.identity);
				for (int k = 0; k < settings.PlacementModifiers?.Length; k++)
				{
					settings.PlacementModifiers[k].ModifyPlacement(ref placement, settings);
				}
				bool flag = true;
				for (int l = 0; l < settings.PlacementValidators?.Length; l++)
				{
					if (!settings.PlacementValidators[l].ValidatePlacement(placement, settings))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_placements.Add(placement);
					break;
				}
			}
		}
		return _placements;
	}

	private IList<Placement> EvenPlacement(Vector3 center, CellObjectSettings<TCellObject> settings)
	{
		_placements.Clear();
		float num = (float)_cellWidth / Mathf.Min(8f, Mathf.Sqrt(settings.MaximumInstances));
		for (float num2 = num / 2f; num2 < (float)_cellWidth; num2 += num)
		{
			for (float num3 = num / 2f; num3 < (float)_cellWidth; num3 += num)
			{
				float num4 = ((!((float)_random.NextDouble() > 0.5f)) ? 1 : (-1));
				float num5 = (float)_random.NextDouble() * settings.XPositionJitter * num4;
				float num6 = (float)_random.NextDouble() * settings.ZPositionJitter * num4;
				if ((float)_random.Next(100) > settings.Rarity)
				{
					continue;
				}
				Vector3 zero = Vector3.zero;
				zero.x = center.x - HalfCellWidth + num2 + num5;
				zero.z = center.z - HalfCellWidth + num3 + num6;
				Placement placement = new Placement(zero, Quaternion.identity);
				for (int i = 0; i < settings.PlacementModifiers?.Length; i++)
				{
					settings.PlacementModifiers[i].ModifyPlacement(ref placement, settings);
				}
				bool flag = true;
				for (int j = 0; j < settings.PlacementValidators?.Length; j++)
				{
					if (!settings.PlacementValidators[j].ValidatePlacement(placement, settings))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					_placements.Add(placement);
					break;
				}
			}
		}
		return _placements;
	}

	private void SaveCellObjects(Cell<TCellObject> cell)
	{
		if (!_cellObjectsDataCache.ContainsKey(cell.Id))
		{
			_cellObjectsDataCache.Add(cell.Id, new List<JObject>());
		}
		IList<JObject> list = _cellObjectsDataCache[cell.Id];
		list.Clear();
		for (int i = 0; i < cell.Objects.Count; i++)
		{
			JObject jObject = SavingUtilities.Save(cell.Objects[i]);
			if (jObject != null && !jObject.IsNull())
			{
				list.Add(jObject);
			}
		}
	}

	private void LoadCellObjects(Cell<TCellObject> cell, IList<JObject> cellObjectsData)
	{
		Debug.Log("LOADING CELL " + cell.Id);
		cell.Loaded = true;
		for (int i = 0; i < cellObjectsData.Count; i++)
		{
			JObject jObject = cellObjectsData[i];
			int value = jObject.GetField("Id").GetValue<int>();
			TCellObject val = FindCellObjectPrefab(value);
			if ((Object)val != (Object)null)
			{
				TCellObject val2 = Object.Instantiate(val);
				val2.transform.parent = cell.ObjectContainer;
				val2.Subscribe(cell);
				if (SavingUtilities.Load(val2, jObject))
				{
					cell.Objects.Add(val2);
					continue;
				}
				val2.Unsubscribe(cell);
				val2.Destroy();
			}
		}
	}

	private TCellObject FindCellObjectPrefab(int prefabId)
	{
		for (int i = 0; i < _cellObjectsSettings.Count; i++)
		{
			CellObjectSettings<TCellObject> cellObjectSettings = _cellObjectsSettings[i];
			if (cellObjectSettings.Prefab.PrefabId == prefabId)
			{
				return cellObjectSettings.Prefab;
			}
		}
		return null;
	}

	public JObject Save()
	{
		return null;
	}

	public void Load(JObject data)
	{
	}
}
