using Beam.Terrain;
using UnityEngine;

namespace Beam.UI.MapEditor;

public interface IMapListElement : IViewAdapter
{
	GameObject gameObject { get; }

	Map Map { get; set; }

	bool Popular { get; set; }

	bool Featured { get; set; }

	ILabelViewAdapter NameLabel { get; }

	ILabelViewAdapter DescriptionLabel { get; }

	ILabelViewAdapter AuthorLabel { get; }

	ILabelViewAdapter VersionLabel { get; }

	ITextureViewAdapter PreviewImage { get; }

	ILabelButtonViewAdapter EditButton { get; }

	ILabelButtonViewAdapter DeleteButton { get; }

	event MapListElementHandler EditClick;

	event MapListElementHandler DeleteClick;

	event MapListElementHandler BeginDrag;

	event MapListElementHandler Drag;

	event MapListElementHandler EndDrag;
}
