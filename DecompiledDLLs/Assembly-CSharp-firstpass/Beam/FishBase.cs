using System;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public abstract class FishBase : MonoBehaviour, IFishable, IFishRenderer, ICellObject, ISaveable
{
	private ICell _cell;

	[SerializeField]
	private int _prefabId = -1;

	[SerializeField]
	private FishingData _fishingData;

	[SerializeField]
	private RenderableWithLods _fishTemplate;

	[FormerlySerializedAs("_renderer")]
	[SerializeField]
	private FishRendererBase _fishRenderer;

	protected ICell Cell => _cell;

	public int PrefabId => _prefabId;

	public FishingData FishingData => _fishingData;

	public RenderableWithLods FishTemplate => _fishTemplate;

	public FishRendererBase FishRenderer => _fishRenderer;

	public event Action<int> LodChanged;

	public event Action<bool> Hooked;

	public event Action<ICellObject> Freed;

	public event Action<ICellObject> Destroyed;

	public event Action<ICell> Subscribed;

	protected virtual void OnSubscribed(ICell cell)
	{
		_fishRenderer.SetActive(active: true);
		Subscribed?.Invoke(cell);
	}

	protected virtual void Start()
	{
		if (!((Object)(object)_fishingData.FishPrefab == (Object)null))
		{
			FishingManager.Instance?.Add(this);
		}
	}

	protected virtual void OnDestroy()
	{
		Destroyed?.Invoke(this);
		Destroyed = null;
		Subscribed = null;
		LodChanged = null;
		Hooked = null;
		Freed = null;
		FishingManager.Instance?.Remove(this);
	}

	void ICellObject.Subscribe(ICell cell)
	{
		_cell = cell;
		_cell.LodChanged += Cell_LodChanged;
		OnSubscribed(cell);
	}

	void ICellObject.Unsubscribe(ICell cell)
	{
		if (cell != null && cell == _cell)
		{
			_cell.LodChanged -= Cell_LodChanged;
			_cell = null;
		}
	}

	void ICellObject.Destroy()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}

	public void Free()
	{
		Freed?.Invoke(this);
	}

	protected virtual void Cell_LodChanged(int lod)
	{
		FishTemplate.SetLod(lod);
		_fishRenderer.SetMesh(FishTemplate.Renderable.Mesh);
		_fishRenderer.SetMaterial(FishTemplate.Renderable.Material);
		LodChanged?.Invoke(lod);
	}

	public virtual void Hook(bool hooked)
	{
		_fishRenderer.SetActive(!hooked);
		Hooked?.Invoke(hooked);
	}

	public virtual void Catch()
	{
		((ICellObject)this).Free();
	}

	public abstract bool CheckPositionInBounds(Vector3 position);

	public abstract bool CheckPositionInInterestRadius(Vector3 position);

	public abstract Bounds GetBounds();

	public virtual JObject Save()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = new JObject();
		jObject.AddField("Id", _prefabId);
		jObject.AddField("Pos", JSerializer.Serialize((object)((Component)this).transform.position));
		return jObject;
	}

	public virtual void Load(JObject data)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_prefabId = data.GetField("Id").GetValue<int>();
		((Component)this).transform.position = JSerializer.Deserialize<Vector3>(data.GetField("Pos"));
	}
}
