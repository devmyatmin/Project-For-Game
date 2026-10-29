using Beam.Crafting;
using UnityEngine;

namespace Beam.UI;

public class StorageLabelPresenter : MonoBehaviour
{
	[SerializeField]
	private StorageLabelViewAdapter _view;

	[SerializeField]
	private Interactive_STORAGE _storage;

	private void Start()
	{
		_storage.StorageNameChanged += Storage_StorageNameChanged;
	}

	private void OnDestory()
	{
		_storage.StorageNameChanged -= Storage_StorageNameChanged;
	}

	private void Storage_StorageNameChanged(string name)
	{
		_view.Label.Text = name;
	}
}
