using UnityEngine;

namespace Beam.UI;

public interface IInputMapCategoryHeader
{
	Transform transform { get; }

	ILabelViewAdapter Label { get; }

	IGameObjectViewAdapter SubheadingsContainer { get; }
}
