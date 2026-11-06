using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class FlockFish : FishBase, IFixedTick, IPlayerProximityHandler
{
	[SerializeField]
	private FlockSettings _flockSettings;

	[SerializeField]
	private List<IAttracterRepeller> _attracters = new List<IAttracterRepeller>();

	[SerializeField]
	private List<IAttracterRepeller> _repellers = new List<IAttracterRepeller>();

	private List<Matrix4x4> _positions = new List<Matrix4x4>(100);

	private BoidFlock _flock;

	private int _ticks;

	FixedTickRate IFixedTick.Rate => FixedTickRate.Full;

	bool IFixedTick.Active => ((Component)this).gameObject.activeInHierarchy;

	Vector3 IPlayerProximityHandler.Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

	float IPlayerProximityHandler.ProximityDistance => _flockSettings.BoundsRadius * 2f;

	bool IPlayerProximityHandler.InProximity { get; set; }

	public IList<IAttracterRepeller> Attracters => _attracters;

	public IList<IAttracterRepeller> Repellers => _repellers;

	public override Bounds GetBounds()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(Vector3.zero, (Vector3)((_flockSettings.BoundsType == BoundsType.Sphere) ? Vector3.one : new Vector3(1f, 0f, 1f)) * (_flockSettings.BoundsRadius * 2f));
	}

	protected override void Start()
	{
	}

	private void OnEnable()
	{
		FixedTicker.Instance?.Add(this);
	}

	private void OnDisable()
	{
		FixedTicker.Instance?.Remove(this);
	}

	protected override void OnSubscribed(ICell cell)
	{
		if (_flock == null)
		{
			CreateBoids(_flockSettings.FlockCount);
		}
		else
		{
			ReinitializeBoids();
		}
		base.OnSubscribed(cell);
	}

	void IFixedTick.FixedTick()
	{
		if (!LevelLoader.IsLoading(((Component)this).gameObject) && FishRenderer.IsVisible())
		{
			UpdateBoids();
			FishRenderer.SetMatrices(_positions);
		}
	}

	private void CreateBoids(int populationCount)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		_flock = new BoidFlock(_flockSettings.BoundsRadius, _flockSettings.SimulationCells);
		_positions = new List<Matrix4x4>(populationCount);
		for (int i = 0; i < populationCount; i++)
		{
			Vector3 position = ((Component)this).transform.position + Random.insideUnitSphere * _flockSettings.BoundsRadius;
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			Boid boid = new Boid(position, insideUnitSphere, _flockSettings);
			_flock.Boids.Add(boid);
			_positions.Add(Matrix4x4.TRS(boid.Position, Quaternion.identity, Vector3.one));
		}
		_flock.Rebuild();
		FishRenderer.SetMatrices(_positions);
	}

	private void ReinitializeBoids()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _flock.Boids.Count; i++)
		{
			Boid boid = _flock.Boids[i];
			Vector3 position = ((Component)this).transform.position + Random.insideUnitSphere * _flockSettings.BoundsRadius;
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			boid.Reinitialize(position, insideUnitSphere);
		}
		_flock.Rebuild();
		FishRenderer.SetMatrices(_positions);
	}

	private void UpdateBoids()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_ticks++;
		Vector3 val;
		if (_ticks >= _flockSettings.SimulationTimestep)
		{
			for (int i = 0; i < _flock.Boids.Count; i++)
			{
				Boid boid = _flock.Boids[i];
				IList<Boid> boids = _flock.FindNeighborsNear(boid.Position.x, boid.Position.y, _flockSettings.CohesionDistance);
				boid.DoBounds(((Component)this).transform.position);
				boid.DoFlock(boids);
				boid.DoRepel(_repellers);
				boid.DoUpdate(Time.deltaTime, _ticks);
				List<Matrix4x4> positions = _positions;
				int index = i;
				Vector3 position = boid.Position;
				val = Vector3.Lerp(boid.Vel, boid.Velocity, Time.deltaTime);
				positions[index] = Matrix4x4.TRS(position, Quaternion.LookRotation(val.normalized, Vector3.up), Vector3.one);
			}
			_ticks = 0;
		}
		else
		{
			for (int j = 0; j < _flock.Boids.Count; j++)
			{
				Boid boid2 = _flock.Boids[j];
				boid2.DoBounds(((Component)this).transform.position);
				boid2.DoRepel(_repellers);
				boid2.DoUpdate(Time.deltaTime);
				List<Matrix4x4> positions2 = _positions;
				int index2 = j;
				Vector3 position2 = boid2.Position;
				val = Vector3.Lerp(boid2.Vel, boid2.Velocity, Time.deltaTime);
				positions2[index2] = Matrix4x4.TRS(position2, Quaternion.LookRotation(val.normalized, Vector3.up), Vector3.one);
			}
		}
		_flock.Rebuild();
	}

	void IPlayerProximityHandler.OnEnteredPlayerProximity(PlayerData player)
	{
	}

	void IPlayerProximityHandler.OnLeftPlayerProximity(PlayerData player)
	{
	}

	public override void Catch()
	{
		base.Catch();
	}

	public override bool CheckPositionInBounds(Vector3 position)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _flock.Boids.Count; i++)
		{
			Boid boid = _flock.Boids[i];
			Vector3 val = position - ((Component)this).transform.TransformPoint(boid.Position);
			if (val.sqrMagnitude < BeamMath.Sqr(0.1f))
			{
				return true;
			}
		}
		return false;
	}

	public override bool CheckPositionInInterestRadius(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - position;
		return val.sqrMagnitude < BeamMath.Sqr(_flockSettings.BoundsRadius);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Pop", _positions.Count);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		CreateBoids(3);
	}
}
