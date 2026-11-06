using System.Collections.Generic;
using Beam.Rendering;
using Beam.Serialization;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam.Intro;

public class AircraftManager : MonoBehaviour, ISaveable
{
	[SerializeField]
	private AircraftObjectives _objectives;

	[SerializeField]
	private AircraftProximity _proximity;

	[SerializeField]
	private AircraftVFX _vfx;

	[SerializeField]
	private LodController _lodController;

	[SerializeField]
	private IPos[] _stationPrefabs;

	[SerializeField]
	private Interactive_AIRCRAFT_DOOR _door;

	private List<Interactive_AIRCRAFT_STATION> _stations = new List<Interactive_AIRCRAFT_STATION>();

	public AircraftObjectives Objectives => _objectives;

	public AircraftProximity Proximity => _proximity;

	private void Start()
	{
		_vfx.Initialize(this);
		CreateStations();
	}

	private void CreateStations()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _stationPrefabs.Length; i++)
		{
			IPos pos = _stationPrefabs[i];
			Interactive_AIRCRAFT_STATION interactive_AIRCRAFT_STATION = Object.Instantiate<Interactive_AIRCRAFT_STATION>(pos.Prefab);
			((Component)interactive_AIRCRAFT_STATION).transform.SetParent(((Component)this).transform, false);
			((Component)interactive_AIRCRAFT_STATION).transform.position = pos.Position.position;
			((Component)interactive_AIRCRAFT_STATION).transform.rotation = pos.Position.rotation;
			interactive_AIRCRAFT_STATION.Initialize(this);
			_stations.Add(interactive_AIRCRAFT_STATION);
		}
	}

	public void EndGameObjectives_ObjectivesCompleted(AircraftObjectivesCompletedArgs args)
	{
		for (int i = 0; i < _stations.Count; i++)
		{
			((Component)_stations[i]).gameObject.SetActive(false);
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject data = SavingUtilities.Save(_vfx);
		jObject.AddField("VFX", data);
		JObject data2 = SavingUtilities.Save(_objectives);
		jObject.AddField("Objectives", data2);
		JObject data3 = SavingUtilities.Save(_door);
		jObject.AddField("Door", data3);
		for (int i = 0; i < _stations.Count; i++)
		{
			Interactive_AIRCRAFT_STATION interactive_AIRCRAFT_STATION = _stations[i];
			JObject data4 = SavingUtilities.Save(interactive_AIRCRAFT_STATION);
			jObject.AddField(((Object)interactive_AIRCRAFT_STATION).name, data4);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("VFX");
		SavingUtilities.Load(_vfx, field);
		JObject field2 = data.GetField("Objectives");
		SavingUtilities.Load(_objectives, field2);
		JObject field3 = data.GetField("Door");
		if (field3 != null)
		{
			SavingUtilities.Load(_door, field3);
		}
		for (int i = 0; i < _stations.Count; i++)
		{
			Interactive_AIRCRAFT_STATION interactive_AIRCRAFT_STATION = _stations[i];
			JObject field4 = data.GetField(((Object)interactive_AIRCRAFT_STATION).name);
			SavingUtilities.Load(interactive_AIRCRAFT_STATION, field4);
		}
	}
}
