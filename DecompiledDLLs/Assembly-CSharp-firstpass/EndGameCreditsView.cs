using System;
using System.Collections;
using Beam.UI;
using UnityEngine;
using UnityEngine.UI;

public class EndGameCreditsView : MonoBehaviour
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private RectTransform _credits;

	[SerializeField]
	private TMPLabelViewAdapter _copyrightText;

	[SerializeField]
	private TMPLabelViewAdapter _creditsText;

	[SerializeField]
	private GameObject _headerGroup;

	[SerializeField]
	private GameObject _footerGroup;

	[SerializeField]
	private RectTransform _footer;

	[SerializeField]
	private float _speed = 1f;

	private RectTransform _canvas;

	public event Action Finished;

	private void OnDestroy()
	{
		Finished = null;
	}

	public void SetCreditsText(string text)
	{
		_creditsText.Text = text;
	}

	public void SetCopyrightText(string text)
	{
		_copyrightText.Text = text;
	}

	public void Show()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!_container.activeInHierarchy)
		{
			_container.SetActive(true);
		}
		_canvas = (RectTransform)((Component)((Component)this).GetComponentInParent<Canvas>()).transform;
		Vector3 val = Vector2.op_Implicit(_credits.anchoredPosition);
		val.y = 0f;
		_credits.anchoredPosition = Vector2.op_Implicit(val);
		LayoutElement component = _footerGroup.GetComponent<LayoutElement>();
		Rect rect = _canvas.rect;
		component.minHeight = rect.height * 1.1f;
		((MonoBehaviour)this).StartCoroutine(Scroll_Sequence());
	}

	public void Hide()
	{
		if (_container.activeInHierarchy)
		{
			_container.SetActive(false);
		}
	}

	private IEnumerator Scroll_Sequence()
	{
		yield return (object)new WaitForSeconds(3f);
		bool stop = false;
		while (!stop)
		{
			Vector3 val = Vector2.op_Implicit(_credits.anchoredPosition);
			val.y += _speed * Time.deltaTime;
			_credits.anchoredPosition = Vector2.op_Implicit(val);
			float y = val.y;
			Rect rect = _canvas.rect;
			float num = rect.height / 2f;
			rect = _credits.rect;
			float num2 = num + rect.height;
			rect = _footer.rect;
			if (y > num2 - rect.height / 2f)
			{
				stop = true;
			}
			yield return null;
		}
		yield return (object)new WaitForSeconds(3f);
		Finished?.Invoke();
	}
}
