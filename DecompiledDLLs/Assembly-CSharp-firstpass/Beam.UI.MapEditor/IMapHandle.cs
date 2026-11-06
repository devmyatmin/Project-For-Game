using UnityEngine;

namespace Beam.UI.MapEditor;

public interface IMapHandle : IViewAdapter
{
	Vector3 Position { get; }

	ITextureViewAdapter HandleSprite { get; }

	void SetSize(Vector2 size);

	void SetScale(Vector2 scale);

	void SetPosition(Vector2 screenPosition);
}
