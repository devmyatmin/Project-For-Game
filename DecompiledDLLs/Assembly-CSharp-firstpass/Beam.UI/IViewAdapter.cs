using UnityEngine;

namespace Beam.UI;

public interface IViewAdapter
{
	Transform transform { get; }

	bool Visible { get; }

	bool Enabled { get; set; }

	event EmptyEventHandler ShowView;

	event EmptyEventHandler HideView;

	void Show();

	void Hide();
}
