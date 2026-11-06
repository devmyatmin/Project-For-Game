using System.Collections.Generic;
using Beam.Utilities;
using Ceto;
using UnityEngine;

namespace Beam;

public class BuoyancyManager : Singleton<BuoyancyManager>
{
	private const int POLL_INTERVAL_SECONDS = 1;

	private const float OUT_OF_RANGE_DISTANCE = 120f;

	private readonly HashSet<IQueryWaves> _queries = new HashSet<IQueryWaves>();

	private int _stepCount;

	private bool _queriesDoneThisFrame;

	public void Add(IQueryWaves query)
	{
		_queries.Add(query);
	}

	public void Remove(IQueryWaves query)
	{
		_queries.Remove(query);
	}

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("Poll_InvokeRepeating", 1f, 1f);
	}

	protected override void OnDestroy()
	{
		_queries.Clear();
		base.OnDestroy();
	}

	private void FixedUpdate()
	{
		if (!_queriesDoneThisFrame)
		{
			_stepCount++;
			DoQueries();
			_stepCount = 0;
		}
	}

	private void LateUpdate()
	{
		_queriesDoneThisFrame = false;
	}

	private void DoQueries()
	{
		foreach (IQueryWaves query in _queries)
		{
			if (!query.Active)
			{
				continue;
			}
			foreach (WaveQuery waveQuery in query.WaveQueries)
			{
				Ceto.Ocean.Instance.QueryWaves(waveQuery);
			}
		}
		_queriesDoneThisFrame = true;
	}

	private void Poll_InvokeRepeating()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		foreach (IQueryWaves query in _queries)
		{
			if (!query.Active)
			{
				continue;
			}
			WaveQuery waveQuery = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(query.WaveQueries);
			if (waveQuery != null)
			{
				Vector3 queryPosition = new Vector3(waveQuery.posX, 0f, waveQuery.posZ);
				if (CheckInRangeOfPlayer(queryPosition))
				{
					query.InRange();
				}
				else if (CheckInRangeOfActiveZone(queryPosition))
				{
					query.InRange();
				}
				else
				{
					query.OutOfRange();
				}
			}
		}
	}

	private bool CheckInRangeOfPlayer(Vector3 queryPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Vector3 val = allPlayer.transform.position - queryPosition;
			if (val.sqrMagnitude < BeamMath.Sqr(120f))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckInRangeOfActiveZone(Vector3 queryPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(queryPosition);
		if ((Object)(object)zone != (Object)null && (zone.Loading || zone.Loaded))
		{
			return true;
		}
		return false;
	}
}
