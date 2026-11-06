using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam;

public struct ProjectileEventDataVal : IProjectileEventData
{
	public readonly Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
	}

	public PhysicMaterial Material { get; }

	public ProjectileEventDataVal(Vector3 position, PhysicMaterial material)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
		Material = material;
	}
}
