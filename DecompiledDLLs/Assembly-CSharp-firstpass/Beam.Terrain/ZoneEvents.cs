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
		CampFirePlaced = null;
		CropPlanted = null;
	}

	private void EventManager_CampFirePlacedEvent(CampFirePlacedEvent e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(e.Position);
		if ((Object)(object)zone != (Object)null)
		{
			zone.Attributes.CampFires++;
			CampFirePlaced?.Invoke(zone);
			Debug.Log((object)("ZoneEvents: '" + ((Object)zone).name + "' campfires: " + zone.Attributes.CampFires));
		}
	}

	private void EventManager_PlantedCropEvent(CropPlantedEvent e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(e.Position);
		if ((Object)(object)zone != (Object)null && zone.Attributes.Crops.ContainsKey(e.PlantType))
		{
			zone.Attributes.Crops[e.PlantType]++;
			CropPlanted?.Invoke(zone, e.PlantType);
			Debug.Log((object)("ZoneEvents: '" + ((Object)zone).name + "' crops: " + zone.Attributes.CampFires));
		}
	}
}
