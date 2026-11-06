using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class LandingSpotController : Singleton<LandingSpotController>
{
	[SerializeField]
	private LayerMask _avoidanceMask;

	private List<LandingSpot> _landingSpots = new List<LandingSpot>(100);

	public LayerMask AvoidanceMask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _avoidanceMask;
		}
	}

	public void AddLandingSpot(LandingSpot landingSpot)
	{
		_landingSpots.Add(landingSpot);
	}

	public void RemoveLandingSpot(LandingSpot landingSpot)
	{
		_landingSpots.Remove(landingSpot);
	}

	public LandingSpot FindLandingSpot(FlockSpecies species, Vector3 position, float maxDistance, Predicate<LandingSpot> predicate)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Beam.Utilities.ExtensionMethods.Shuffle(_landingSpots);
		LandingSpot result = null;
		float num = BeamMath.Sqr(maxDistance);
		for (int i = 0; i < _landingSpots.Count; i++)
		{
			if (_landingSpots[i].Species == species && !_landingSpots[i].Occupied && (predicate == null || predicate(_landingSpots[i])))
			{
				Vector3 val = ((Component)_landingSpots[i]).transform.position - position;
				float sqrMagnitude = val.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = _landingSpots[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}
}
