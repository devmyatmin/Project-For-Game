using System;

namespace Beam;

public interface IDraggable : IPhysical, IBase, IHasCraftingType
{
	bool CanDrag { get; set; }

	bool IsDragging { get; set; }

	float CurrentDrag { get; set; }

	float CurrentAngularDrag { get; set; }

	event Action<IDraggable> BeginDragged;

	event Action<IDraggable> EndDragged;

	void BeginDrag();

	void EndDrag();
}
