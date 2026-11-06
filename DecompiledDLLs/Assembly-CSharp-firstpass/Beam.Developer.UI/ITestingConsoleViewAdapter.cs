using Beam.UI;
using UnityEngine;

namespace Beam.Developer.UI;

public interface ITestingConsoleViewAdapter : IViewAdapter
{
	ILabelButtonViewAdapter FlyModeButton { get; }

	ILabelButtonViewAdapter CreatePrefabButton { get; }

	UPrefabListElement PrefabListElementPrefab { get; }

	GameObject PrefabListContainer { get; }
}
