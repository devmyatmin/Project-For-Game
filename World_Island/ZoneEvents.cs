using System;
using Beam.Events;
using UnityEngine;

namespace Beam.Terrain;

public class ZoneEvents : MonoBehaviour
{
	public event Action<Zone> CampFirePlaced;

	public event Action<Zone, PlantType> CropPlanted;

	private void Awake()
	{
		EventManager.AddListener<CampFirePlacedEvent>(EventManager_CampFirePlacedEvent);
		EventManager.AddListener<CropPlantedEvent>(EventManager_PlantedCropEvent);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<CampFirePlacedEvent>(EventManager_CampFirePlacedEvent);
		EventManager.RemoveListener<CropPlantedEvent>(EventManager_PlantedCropEvent);
		this.CampFirePlaced = null;
		this.CropPlanted = null;
	}

	private void EventManager_CampFirePlacedEvent(CampFirePlacedEvent e)
	{
		Zone zone = StrandedWorld.GetZone(e.Position);
		if (zone != null)
		{
			zone.Attributes.CampFires++;
			this.CampFirePlaced?.Invoke(zone);
			Debug.Log("ZoneEvents: '" + zone.name + "' campfires: " + zone.Attributes.CampFires);
		}
	}

	private void EventManager_PlantedCropEvent(CropPlantedEvent e)
	{
		Zone zone = StrandedWorld.GetZone(e.Position);
		if (zone != null && zone.Attributes.Crops.ContainsKey(e.PlantType))
		{
			zone.Attributes.Crops[e.PlantType]++;
			this.CropPlanted?.Invoke(zone, e.PlantType);
			Debug.Log("ZoneEvents: '" + zone.name + "' crops: " + zone.Attributes.CampFires);
		}
	}
}
