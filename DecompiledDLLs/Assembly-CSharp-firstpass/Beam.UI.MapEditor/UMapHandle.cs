using UnityEngine;

namespace Beam.UI.MapEditor;

public class UMapHandle : ViewAdapterBase, IMapHandle, IViewAdapter
{
	[SerializeField]
	private RectTransform _handle;

	[SerializeField]
	private UTextureViewAdapter _handleSprite;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

	public ITextureViewAdapter HandleSprite => _handleSprite;

	public override void Show()
	{
		if (!((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
			base.Hide();
		}
	}

	public void SetSize(Vector2 size)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_handle.sizeDelta = size;
	}

	public void SetScale(Vector2 scale)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((Transform)_handle).localScale = new Vector3(scale.x, scale.y, 1f);
	}

	public void SetPosition(Vector2 screenPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((Component)this).transform.parent;
		float num = val.sizeDelta.x / (float)Screen.width;
		float num2 = val.sizeDelta.y / (float)Screen.height;
		Vector2 anchoredPosition = new Vector2(screenPosition.x * num, screenPosition.y * num2);
		((RectTransform)((Component)this).transform).anchoredPosition = anchoredPosition;
	}
}
