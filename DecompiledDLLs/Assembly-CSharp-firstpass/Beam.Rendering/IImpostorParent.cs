using System.Collections.Generic;
using UnityEngine;

namespace Beam.Rendering;

public interface IImpostorParent
{
	GameObject gameObject { get; }

	int Id { get; }

	ImpostorMode Mode { get; }

	ImposterScope Scope { get; }

	Vector3 Position { get; }

	Quaternion Rotation { get; }

	ImpostorBase Impostor { get; }

	IList<Renderer> Renderables { get; }
}
