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
		for (int i = 0; i < _stationPrefabs.Length; i++)
		{
			IPos pos = _stationPrefabs[i];
			Interactive_AIRCRAFT_STATION interactive_AIRCRAFT_STATION = Object.Instantiate(pos.Prefab);
			interactive_AIRCRAFT_STATION.transform.SetParent(base.transform, worldPositionStays: false);
			interactive_AIRCRAFT_STATION.transform.position = pos.Position.position;
			interactive_AIRCRAFT_STATION.transform.rotation = pos.Position.rotation;
			interactive_AIRCRAFT_STATION.Initialize(this);
			_stations.Add(interactive_AIRCRAFT_STATION);
		}
	}

	public void EndGameObjectives_ObjectivesCompleted(AircraftObjectivesCompletedArgs args)
	{
		for (int i = 0; i < _stations.Count; i++)
		{
			_stations[i].gameObject.SetActive(value: false);
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
			jObject.AddField(interactive_AIRCRAFT_STATION.name, data4);
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
			JObject field4 = data.GetField(interactive_AIRCRAFT_STATION.name);
			SavingUtilities.Load(interactive_AIRCRAFT_STATION, field4);
		}
	}
}
