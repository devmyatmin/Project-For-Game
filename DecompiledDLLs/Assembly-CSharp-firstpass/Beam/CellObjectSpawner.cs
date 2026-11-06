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
		if ((Object)(object)_following != (Object)null && !BeamTeam.IsNullOrDestroyed(((Component)_following).gameObject))
		{
			return ((Component)_following).gameObject.GetComponent<T>();
		}
		return default;
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
		if (!BeamTeam.IsNullOrDestroyed(following) && following.Id == disconnectedEvent.PlayerId)
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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _cells.Length; i++)
		{
			if (Vector3Tools.Hash(_cells[i].Position) + _hashOffset == hash)
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected Obj, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected Obj, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		_cellsBuffers = new Cell<TCellObject>[2][];
		_cellsBuffers[0] = new Cell<TCellObject>[9];
		_cellsBuffers[1] = new Cell<TCellObject>[9];
		Vector3 val = new Vector3(Mathf.Round(Following.position.x / 16f) * 16f, 0f, Mathf.Round(Following.position.z / 16f) * 16f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float num = (float)i - 1.5f;
				float num2 = (float)j - 1.5f;
				Vector3 zero = Vector3.zero;
				zero.x = val.x + num * (float)_cellWidth + HalfCellWidth;
				zero.z = val.z + num2 * (float)_cellWidth + HalfCellWidth;
				GameObject val2 = new GameObject("Cell");
				val2.transform.parent = ((Component)this).transform;
				val2.transform.position = zero;
				GameObject val3 = new GameObject("Objects");
				val3.transform.parent = val2.transform;
				val3.transform.localPosition = Vector3.zero;
				Cell<TCellObject> cell = new Cell<TCellObject>(val2.transform, Vector3.one * (float)_cellWidth);
				cell.ObjectContainer = val3.transform;
				cell.CellObjectFreed += FreeCellObject;
				_cells[j * 3 + i] = cell;
			}
		}
		UpdateCellPositions();
		UpdateCellObjects();
		HashCollectionCache.Add(this);
		((MonoBehaviour)this).InvokeRepeating("UpdateCenterCell", 1f, 0.5f);
	}

	private void UpdateCenterCell()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(Following) || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		Vector3 position = Following.position;
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
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!cell.Generated && !HashCollectionCache.HasHash(this, Vector3Tools.Hash(cell.Position) + _hashOffset))
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
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
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
					((Component)(object)val).transform.parent = cell.ObjectContainer;
					((Component)(object)val).transform.position = placement.Position;
					((Component)(object)val).transform.rotation = placement.Rotation;
					cell.AddCellObject(val);
				}
			}
		}
	}

	private IList<Placement> RandomPlacement(Vector3 center, CellObjectSettings<TCellObject> settings)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
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
				Vector3 val = Vector3.zero;
				val.x = (float)_random.NextDouble() * num * settings.XPositionJitter;
				val.z = (float)_random.NextDouble() * num * settings.ZPositionJitter;
				if (val.sqrMagnitude > BeamMath.Sqr(HalfCellWidth))
				{
					val.Normalize();
					val *= HalfCellWidth;
				}
				Placement placement = new Placement(center + val, Quaternion.identity);
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
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		_placements.Clear();
		float num = (float)_cellWidth / Mathf.Min(8f, Mathf.Sqrt((float)settings.MaximumInstances));
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
		Debug.Log((object)("LOADING CELL " + cell.Id));
		cell.Loaded = true;
		for (int i = 0; i < cellObjectsData.Count; i++)
		{
			JObject jObject = cellObjectsData[i];
			int value = jObject.GetField("Id").GetValue<int>();
			TCellObject val = FindCellObjectPrefab(value);
			if ((Object)(object)val != (Object)null)
			{
				TCellObject val2 = Object.Instantiate<TCellObject>(val);
				((Component)(object)val2).transform.parent = cell.ObjectContainer;
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
		return default;
	}

	public JObject Save()
	{
		return null;
	}

	public void Load(JObject data)
	{
	}
}
