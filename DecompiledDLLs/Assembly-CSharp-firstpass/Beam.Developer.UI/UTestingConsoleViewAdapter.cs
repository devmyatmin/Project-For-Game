using Beam.UI;
using UnityEngine;

namespace Beam.Developer.UI;

public class UTestingConsoleViewAdapter : ViewAdapterBase, ITestingConsoleViewAdapter, IViewAdapter
{
	[SerializeField]
	private GameObject _testingConsolePanel;

	[SerializeField]
	private ULabelButtonViewAdapter _flyModeButton;

	[SerializeField]
	private ULabelButtonViewAdapter _createPrefabButton;

	[SerializeField]
	private UPrefabListElement _prefabListElementPrefab;

	[SerializeField]
	private GameObject _prefabListContainer;

	public ILabelButtonViewAdapter FlyModeButton => _flyModeButton;

	public ILabelButtonViewAdapter CreatePrefabButton => _createPrefabButton;

	public UPrefabListElement PrefabListElementPrefab => _prefabListElementPrefab;

	public GameObject PrefabListContainer => _prefabListContainer;

	private void Awake()
	{
		Object.FindObjectOfType<TestingConsole>().Initialize(this);
	}

	public override void Show()
	{
		if (!_testingConsolePanel.activeSelf)
		{
			_testingConsolePanel.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_testingConsolePanel.activeSelf)
		{
			_testingConsolePanel.SetActive(false);
			base.Hide();
		}
	}
}
