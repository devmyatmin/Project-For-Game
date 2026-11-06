using System;
using System.Collections.Generic;
using Beam.UI;
using Ceto;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class Buoyancy : MonoBehaviour, IQueryWaves
{
	[Serializable]
	private class SlicesPerAxis
	{
		public int X = 2;

		public int Y = 2;

		public int Z = 2;
	}

	private const float WATER_DENSITY = 1000f;

	private List<WaveQuery> _waveQueries = new List<WaveQuery>();

	private int _outOfRangeCount;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private Collider _bouyancyCollider;

	[FormerlySerializedAs("density")]
	[SerializeField]
	private float _density = 500f;

	[SerializeField]
	private SlicesPerAxis _slicesPerAxis = new SlicesPerAxis();

	[FormerlySerializedAs("voxelsLimit")]
	[SerializeField]
	private int _voxelsLimit = 8;

	[FormerlySerializedAs("damper")]
	[SerializeField]
	private float _damper = 0.1f;

	private float _originalDamper;

	private float _voxelHalfHeight;

	private Vector3 _localArchimedesForce;

	private List<Vector3> _voxels;

	private bool _wind = true;

	private bool _waveQueriesInitialized;

	private bool _floating;

	private bool _isInWater;

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

	public bool Floating
	{
		get
		{
			if (_isInWater)
			{
				return _floating;
			}
			return false;
		}
	}

	public bool IsInWater => _isInWater;

	void IQueryWaves.InRange()
	{
		_outOfRangeCount = 0;
	}

	void IQueryWaves.OutOfRange()
	{
		if (_outOfRangeCount >= 3)
		{
			_outOfRangeCount++;
			BaseObject component = ((Component)this).GetComponent<BaseObject>();
			if (!((Object)(object)component == (Object)null))
			{
				LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
				localizedNotification.Priority = NotificationPriority.Immediate;
				localizedNotification.Duration = 8f;
				localizedNotification.MessageText.SetTerm("ITEM_NTF_LOST_AT_SEA_DESC");
				localizedNotification.MessageText.SetArgs(component.DisplayName);
				localizedNotification.Raise();
			}
		}
	}

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)this).transform.rotation;
		Vector3 position = ((Component)this).transform.position;
		((Component)this).transform.rotation = Quaternion.identity;
		((Component)this).transform.position = Vector3.zero;
		if (!Object.op_Implicit((Object)(object)_bouyancyCollider))
		{
			_bouyancyCollider = ((Component)this).GetComponent<Collider>();
		}
		Bounds bounds = _bouyancyCollider.bounds;
		if (bounds.size.x < bounds.size.y)
		{
			_voxelHalfHeight = bounds.size.x;
			_voxelHalfHeight /= 2 * _slicesPerAxis.X;
		}
		else
		{
			_voxelHalfHeight = bounds.size.y;
			_voxelHalfHeight /= 2 * _slicesPerAxis.Y;
		}
		float num = bounds.size.z / (float)(2 * _slicesPerAxis.Z);
		if (num < _voxelHalfHeight)
		{
			_voxelHalfHeight = num;
		}
		_rigidbody.centerOfMass = new Vector3(0f, (0f - bounds.extents.y) * 0f, 0f) + ((Component)this).transform.InverseTransformPoint(bounds.center);
		_voxels = SliceIntoVoxels();
		((Component)this).transform.rotation = rotation;
		((Component)this).transform.position = position;
		float num2 = _rigidbody.mass / _density;
		WeldPoints(_voxels, _voxelsLimit);
		CreateWaveQueries();
		float num3 = 1000f * Mathf.Abs(Physics.gravity.y) * num2;
		_localArchimedesForce = new Vector3(0f, num3, 0f) / (float)_voxels.Count;
		_originalDamper = _damper;
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
				AddWaveQueries();
			});
		}
		else
		{
			AddWaveQueries();
		}
	}

	private void AddWaveQueries()
	{
		if (!_waveQueriesInitialized)
		{
			if (Object.op_Implicit((Object)(object)Singleton<BuoyancyManager>.Instance))
			{
				Singleton<BuoyancyManager>.Instance.Add(this);
			}
			_waveQueriesInitialized = true;
		}
	}

	private void RemoveWaveQueries()
	{
		if (_waveQueriesInitialized)
		{
			if (Object.op_Implicit((Object)(object)Singleton<BuoyancyManager>.Instance))
			{
				Singleton<BuoyancyManager>.Instance.Remove(this);
			}
			_waveQueriesInitialized = false;
		}
	}

	private void OnDestroy()
	{
		RemoveWaveQueries();
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>(_slicesPerAxis.X * _slicesPerAxis.Y * _slicesPerAxis.Z);
		Bounds bounds = _bouyancyCollider.bounds;
		for (int i = 0; i < _slicesPerAxis.X; i++)
		{
			for (int j = 0; j < _slicesPerAxis.Y; j++)
			{
				for (int k = 0; k < _slicesPerAxis.Z; k++)
				{
					float num = bounds.min.x + bounds.size.x / (float)_slicesPerAxis.X * (0.5f + (float)i);
					float num2 = bounds.min.y + bounds.size.y / (float)_slicesPerAxis.Y * (0.5f + (float)j);
					float num3 = bounds.min.z + bounds.size.z / (float)_slicesPerAxis.Z * (0.5f + (float)k);
					Vector3 item = ((Component)this).transform.InverseTransformPoint(new Vector3(num, num2, num3));
					list.Add(item);
				}
			}
		}
		return list;
	}

	private static void FindClosestPoints(IList<Vector3> list, out int firstIndex, out int secondIndex)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		float num2 = float.MinValue;
		firstIndex = 0;
		secondIndex = 1;
		for (int i = 0; i < list.Count - 1; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				float num3 = Vector3.Distance(list[i], list[j]);
				if (num3 < num)
				{
					num = num3;
					firstIndex = i;
					secondIndex = j;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
		}
	}

	private void OnTransformParentChanged()
	{
		if (!Object.op_Implicit((Object)(object)_rigidbody))
		{
			_rigidbody = ((Component)this).GetComponent<Rigidbody>();
		}
	}

	private static void WeldPoints(IList<Vector3> list, int targetCount)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (list.Count > 2 && targetCount >= 2)
		{
			while (list.Count > targetCount)
			{
				FindClosestPoints(list, out var firstIndex, out var secondIndex);
				Vector3 item = (list[firstIndex] + list[secondIndex]) * 0.5f;
				list.RemoveAt(secondIndex);
				list.RemoveAt(firstIndex);
				list.Add(item);
			}
		}
	}

	private void UpdateWaterAndQueries()
	{
		CheckIfInWater();
		if (_isInWater)
		{
			AddWaveQueries();
		}
		else
		{
			RemoveWaveQueries();
		}
	}

	private void FixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		if (!Physics.autoSimulation)
		{
			return;
		}
		for (int i = 0; i < _voxels.Count; i++)
		{
			Vector3 val = _voxels[i];
			Vector3 val2 = ((Component)this).transform.TransformPoint(val);
			WaveQuery waveQuery = _waveQueries[i];
			waveQuery.posX = val2.x;
			waveQuery.posZ = val2.z;
		}
		if (!Object.op_Implicit((Object)(object)_rigidbody) || !_rigidbody.useGravity || !_bouyancyCollider.enabled)
		{
			return;
		}
		UpdateWaterAndQueries();
		if (!_isInWater)
		{
			_floating = false;
			return;
		}
		int num = 0;
		for (int j = 0; j < _voxels.Count; j++)
		{
			Vector3 val3 = _voxels[j];
			Vector3 val4 = ((Component)this).transform.TransformPoint(val3);
			float height = _waveQueries[j].result.height;
			if (val4.y - _voxelHalfHeight < height)
			{
				float num2 = (height - val4.y) / (2f * _voxelHalfHeight) + 0.5f;
				num2 = Mathf.Clamp01(num2);
				Vector3 val5 = -_rigidbody.GetPointVelocity(val4) * (_damper * _rigidbody.mass) + Mathf.Sqrt(num2) * _localArchimedesForce;
				_rigidbody.AddForceAtPosition(val5, val4);
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

	public void CheckIfInWater()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float num = Ceto.Ocean.Instance.QueryWaves(((Component)this).transform.position.x, ((Component)this).transform.position.z) + 0.1f;
		_isInWater = num >= ((Component)this).transform.position.y;
	}
}
