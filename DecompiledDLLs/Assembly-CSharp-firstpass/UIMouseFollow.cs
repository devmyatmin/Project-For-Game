using UnityEngine;
using UnityEngine.UI;

public class UIMouseFollow : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler _canvasScaler;

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x * _canvasScaler.referenceResolution.x / (float)Screen.width, Input.mousePosition.y * _canvasScaler.referenceResolution.y / (float)Screen.height);
	}
}
