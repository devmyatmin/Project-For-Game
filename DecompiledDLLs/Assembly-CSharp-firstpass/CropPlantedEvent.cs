using Beam;
using Beam.Events;
using UnityEngine;

public class CropPlantedEvent : IEvent
{
	private readonly Vector3 _position;

	private readonly PlantType _plantType;

	public Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
	}

	public PlantType PlantType => _plantType;

	public CropPlantedEvent(Vector3 position, PlantType plantType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_plantType = plantType;
	}
}
