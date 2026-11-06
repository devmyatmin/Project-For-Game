using System.Collections.Generic;
using UnityEngine;

namespace Beam.Crafting;

public static class GhostExtensions
{
	public static void SetCollisionForConstructions(this Ghost ghost, IEnumerable<Constructing> constructions, bool ignore)
	{
		foreach (Constructing construction in constructions)
		{
			Collider[] colliders = construction.Colliders;
			foreach (Collider collider in colliders)
			{
				ghost.IgnoreCollider(collider, ignore);
			}
		}
	}
}
