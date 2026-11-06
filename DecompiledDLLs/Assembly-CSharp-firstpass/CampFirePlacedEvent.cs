using Beam.Events;
using UnityEngine;

public class CampFirePlacedEvent : IEvent
{
	private readonly Vector3 _position;

	public Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
	}

	public CampFirePlacedEvent(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
	}
}
