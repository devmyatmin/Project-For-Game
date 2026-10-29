using Beam.Terrain;
using UnityEngine;

namespace Beam.UI.MapEditor;

public interface IWorldMapSlot : IViewAdapter
{
	GameObject gameObject { get; }

	Map Map { get; }

	ITextureViewAdapter PreviewImage { get; }

	IButtonViewAdapter Button { get; }

	void SetMap(Map map);

	void SetState(MapEditorData.EMapState state);
}
