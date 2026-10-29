using Sirenix.OdinInspector;
using UnityEngine;

public class WorldSpaceAttributeController : SerializedMonoBehaviour
{
	[SerializeField]
	private WorldSpaceAttributeViewBase _view;

	[SerializeField]
	private IHaveAttribute _attribute;

	private void Awake()
	{
		if (_view == null)
		{
			_view = NullWorldSpaceAttributeView.Instance;
			Debug.LogError("");
		}
	}

	public void Activate(bool activate)
	{
		if (activate)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		_view.Show(_attribute);
	}

	public void Hide()
	{
		_view.Hide();
	}
}
