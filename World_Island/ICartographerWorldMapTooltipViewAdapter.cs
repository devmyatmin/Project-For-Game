using Beam.Terrain;
using UnityEngine;

namespace Beam.UI;

public interface ICartographerWorldMapTooltipViewAdapter : IViewAdapter
{
	GameObject gameObject { get; }

	GameObject TooltipGameObject { get; }

	ICanvasGroupViewAdapter CanvasGroup { get; }

	ILabelViewAdapter NameLabel { get; }

	ILabelViewAdapter AuthorLabel { get; }

	ILabelViewAdapter DescriptionLabel { get; }

	ILabelViewAdapter VersionNumberLabel { get; }

	ILabelViewAdapter StatusLabel { get; }

	void SetPosition(Vector2 screenPosition);

	void SetSource(MapEditorData.EMapSource source);

	void SetState(MapEditorData.EMapState state);
}
