using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class HidingSpotController : Singleton<HidingSpotController>
{
	private List<HidingSpot> _hidingSpots = new List<HidingSpot>(20);

	public IList<HidingSpot> HidingSpots => _hidingSpots;

	public HidingSpot FindHidingSpot(LandSpecies species, Vector3 position, float maxDistance, Predicate<HidingSpot> predicate)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Beam.Utilities.ExtensionMethods.Shuffle(_hidingSpots);
		HidingSpot result = null;
		float num = BeamMath.Sqr(maxDistance);
		for (int i = 0; i < _hidingSpots.Count; i++)
		{
			if (_hidingSpots[i].Species == species && !_hidingSpots[i].Occupied && (predicate == null || predicate(_hidingSpots[i])))
			{
				Vector3 val = ((Component)_hidingSpots[i]).transform.position - position;
				float sqrMagnitude = val.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = _hidingSpots[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}
}
