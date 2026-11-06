using System.Collections.Generic;
using Beam;
using Ceto;
using UnityEngine;

public class SimpleBuoyancy : MonoBehaviour, IQueryWaves, IFixedTick
{
	private WaveQuery _waveQuery = new WaveQuery();

	private List<WaveQuery> _waveQueries;

	IEnumerable<WaveQuery> IQueryWaves.WaveQueries
	{
		get
		{
			if (_waveQueries == null)
			{
				_waveQueries = new List<WaveQuery> { _waveQuery };
			}
			return _waveQueries;
		}
	}

	public bool Active
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

	public FixedTickRate Rate => FixedTickRate.Quarter;

	void IQueryWaves.InRange()
	{
	}

	void IQueryWaves.OutOfRange()
	{
	}

	private void OnEnable()
	{
		FixedTicker.Instance?.Add(this);
		Singleton<BuoyancyManager>.Instance?.Add(this);
	}

	private void OnDisable()
	{
		FixedTicker.Instance?.Remove(this);
		Singleton<BuoyancyManager>.Instance?.Remove(this);
	}

	void IFixedTick.FixedTick()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_waveQuery.posX = ((Component)this).transform.position.x;
		_waveQuery.posZ = ((Component)this).transform.position.z;
		Vector3 position = ((Component)this).transform.position;
		position.y = _waveQuery.result.height;
		((Component)this).transform.position = position;
	}
}
