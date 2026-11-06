using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beam.UI;

public class ButtonMarker : MonoBehaviour
{
	[SerializeField]
	private Image _marker;

	private GameObject _previous;

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.main.currentSelectedGameObject;
		if ((Object)(object)currentSelectedGameObject == (Object)null)
		{
			((Behaviour)_marker).enabled = false;
			((Component)_marker).transform.SetParent((Transform)null, false);
		}
		else if ((Object)(object)currentSelectedGameObject != (Object)(object)_previous)
		{
			Transform transform = currentSelectedGameObject.transform;
			SetPosition((RectTransform)(object)((transform is RectTransform) ? transform : null));
			_previous = currentSelectedGameObject;
		}
	}

	public void SetPosition(RectTransform rect)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			((Component)_marker).transform.SetParent((Transform)(object)rect, false);
			((Graphic)_marker).rectTransform.anchoredPosition3D = new Vector3(16f, 0f, 0f);
			((Behaviour)_marker).enabled = true;
		}
	}
}
