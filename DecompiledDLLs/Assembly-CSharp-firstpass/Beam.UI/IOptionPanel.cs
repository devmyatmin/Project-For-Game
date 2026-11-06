using UnityEngine;

namespace Beam.UI;

public interface IOptionPanel
{
	Transform GetCategoriesContainer();

	void Show();

	void Hide();

	void Apply();
}
