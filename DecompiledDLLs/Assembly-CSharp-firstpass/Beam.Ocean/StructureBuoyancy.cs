using System.Collections.Generic;
using Beam.UI;
using Ceto;
using Funlabs;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam.Ocean;

public class StructureBuoyancy : MultiplayerBehaviour, IQueryWaves
{
	private const float WATER_DENSITY = 1000f;

	private List<WaveQuery> _waveQueries = new List<WaveQuery>();

	private bool _outOfRange;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private BoxCollider _bouyancyCollider;

	[FormerlySerializedAs("density")]
	[SerializeField]
	private float _density = 500f;

	[FormerlySerializedAs("slicesPerAxis")]
	[SerializeField]
	private int _slicesPerAxis = 3;

	[FormerlySerializedAs("voxelsLimit")]
	[SerializeField]
	private int _voxelsLimit = 16;

	[FormerlySerializedAs("damper")]
	[SerializeField]
	private float _damper = 0.1f;

	private float _originalDamper;

	private float _voxelHalfHeight;

	private Vector3 _localArchimedesForce;

	private List<Vector3> _voxels = new List<Vector3>();

	private bool _wind = true;

	private bool _floating;

	bool IQueryWaves.Active
	{
		get
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null)
			{
				return ((Component)this).gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	IEnumerable<WaveQuery> IQueryWaves.WaveQueries => _waveQueries;

	public Rigidbody Rigidbody => _rigidbody;

	public BoxCollider BuoyancyCollider => _bouyancyCollider;

	public List<RaftBuoyancy> RaftBuoyancys { get; set; } = new List<RaftBuoyancy>();

	void IQueryWaves.InRange()
	{
		_outOfRange = false;
	}

	void IQueryWaves.OutOfRange()
	{
		if (!_outOfRange)
		{
			_outOfRange = true;
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.MessageText.SetTerm("ITEM_NTF_LOST_AT_SEA_DESC");
			localizedNotification.MessageText.SetArgs("INTERACTIVE_TYPE_STRUCTURE_RAFT");
			localizedNotification.Raise();
		}
	}

	private void Awake()
	{
		_originalDamper = _damper;
	}

	public override void Attached()
	{
		base.Attached();
		Resize();
	}

	public void Resize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Quaternion rotation = ((Component)this).transform.rotation;
		((Component)this).transform.position = Vector3.zero;
		((Component)this).transform.rotation = Quaternion.identity;
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.angularVelocity = Vector3.zero;
		_rigidbody.useGravity = false;
		_rigidbody.isKinematic = true;
		_bouyancyCollider.center = Vector3.zero;
		_bouyancyCollider.size = Vector3.zero;
		Bounds bounds = ((Collider)_bouyancyCollider).bounds;
		for (int i = 0; i < RaftBuoyancys.Count; i++)
		{
			RaftBuoyancy raftBuoyancy = RaftBuoyancys[i];
			bounds.Encapsulate(((Collider)raftBuoyancy.Collider).bounds);
		}
		_bouyancyCollider.center = bounds.center;
		_bouyancyCollider.size = bounds.size;
		bounds = ((Collider)_bouyancyCollider).bounds;
		_voxelHalfHeight = bounds.size.y / 2f;
		_voxels = SliceIntoVoxels();
		CreateWaveQueries();
		float num = _rigidbody.mass / _density;
		float num2 = 1000f * Mathf.Abs(Physics.gravity.y) * num;
		_localArchimedesForce = new Vector3(0f, num2, 0f) / (float)_voxels.Count;
		((Component)this).transform.rotation = rotation;
		((Component)this).transform.position = position;
		((Component)this).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
		_rigidbody.useGravity = true;
		bool isOwner = ((EntityBehaviour)this).entity.IsOwner;
		_rigidbody.isKinematic = !isOwner;
	}

	private void CreateWaveQueries()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		_waveQueries.Clear();
		for (int i = 0; i < _voxels.Count; i++)
		{
			Vector3 val = ((Component)this).transform.TransformPoint(_voxels[i]);
			_waveQueries.Add(new WaveQuery(val.x, val.z));
		}
	}

	private void Start()
	{
		if (LevelLoader.IsLoading(((Component)this).gameObject))
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(() =>
			{
				Singleton<BuoyancyManager>.Instance?.Add(this);
			});
		}
		else
		{
			Singleton<BuoyancyManager>.Instance?.Add(this);
		}
	}

	private void OnDestroy()
	{
		Singleton<BuoyancyManager>.Instance?.Remove(this);
	}

	public void EnableWind(bool enable)
	{
		_wind = enable;
	}

	public void SetDampening(float dampening)
	{
		_damper = dampening;
	}

	public void ResetDampening()
	{
		_damper = _originalDamper;
	}

	private List<Vector3> SliceIntoVoxels()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>(_slicesPerAxis * _slicesPerAxis);
		Bounds bounds = ((Collider)_bouyancyCollider).bounds;
		for (int i = 0; i < _slicesPerAxis; i++)
		{
			for (int j = 0; j < _slicesPerAxis; j++)
			{
				float num = bounds.min.x + bounds.size.x / (float)_slicesPerAxis * (0.5f + (float)i);
				float num2 = bounds.min.z + bounds.size.z / (float)_slicesPerAxis * (0.5f + (float)j);
				float num3 = bounds.min.y + bounds.size.y * 0.5f;
				Vector3 item = ((Component)this).transform.InverseTransformPoint(new Vector3(num, num3, num2));
				list.Add(item);
			}
		}
		return list;
	}

	private void FixedUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (!Physics.autoSimulation)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _voxels.Count; i++)
		{
			Vector3 val = _voxels[i];
			Vector3 val2 = ((Component)this).transform.TransformPoint(val);
			WaveQuery waveQuery = _waveQueries[i];
			waveQuery.posX = val2.x;
			waveQuery.posZ = val2.z;
			float height = waveQuery.result.height;
			if (val2.y - _voxelHalfHeight < height)
			{
				float num2 = (height - val2.y) / (2f * _voxelHalfHeight) + 0.5f;
				if (num2 > 7f)
				{
					num2 = 7f / (_rigidbody.mass / 40f);
				}
				else if (num2 < 0f)
				{
					num2 = 0f;
				}
				Vector3 val3 = -_rigidbody.GetPointVelocity(val2) * _damper * _rigidbody.mass;
				val3.x /= _voxels.Count;
				val3.z /= _voxels.Count;
				Vector3 val4 = val3 + Mathf.Sqrt(num2) * _localArchimedesForce;
				_rigidbody.AddForceAtPosition(val4, val2);
				num++;
			}
		}
		if (!LevelLoader.IsLoadingScene() && !LevelLoader.IsLoading(((Component)this).gameObject))
		{
			_floating = num > 0;
			if (_floating && _wind)
			{
				_rigidbody.AddForce(Current.GetVelocity(), (ForceMode)2);
			}
		}
	}
}
