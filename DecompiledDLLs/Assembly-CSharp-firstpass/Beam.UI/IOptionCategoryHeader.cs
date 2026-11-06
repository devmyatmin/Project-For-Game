using UnityEngine;

namespace Beam.UI;

public interface IOptionCategoryHeader
{
	Transform transform { get; }

	ILabelViewAdapter Label { get; }

	Transform ElementsContainer { get; }

	int Id { get; set; }
}
