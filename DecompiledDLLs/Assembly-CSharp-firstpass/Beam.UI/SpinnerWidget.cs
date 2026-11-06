using UnityEngine;

namespace Beam.UI;

[ExecuteInEditMode]
public class SpinnerWidget : MonoBehaviour
{
	[SerializeField]
	private UImageViewAdapter _spinnerImage;

	[SerializeField]
	private float _speed = 1f;

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void Update()
	{
		((Component)_spinnerImage).transform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime * _speed);
	}
}
