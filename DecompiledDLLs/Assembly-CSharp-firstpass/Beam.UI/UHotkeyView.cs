using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public class UHotkeyView : HotkeyView
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private HotkeyElementView _hotkeyElementPrefab;

	[SerializeField]
	private GameObject _hotkeyElementContainer;

	[SerializeField]
	private HotkeyLayout _layout;

	[SerializeField]
	private UnityEvent _assignmentCreated;

	[SerializeField]
	private UnityEvent _assignmentRemoved;

	[SerializeField]
	private UnityEvent _assignmentFailed;

	private IList<HotkeyElementView> _elements;

	public override void OnAssignmentCreated()
	{
		UnityEvent assignmentCreated = _assignmentCreated;
		if (assignmentCreated != null)
		{
			assignmentCreated.Invoke();
		}
	}

	public override void OnAssignmentRemoved()
	{
		UnityEvent assignmentRemoved = _assignmentRemoved;
		if (assignmentRemoved != null)
		{
			assignmentRemoved.Invoke();
		}
	}

	public override void OnAssignmentFailed()
	{
		UnityEvent assignmentFailed = _assignmentFailed;
		if (assignmentFailed != null)
		{
			assignmentFailed.Invoke();
		}
	}

	public override void Initialize(IEnumerable<HotkeyData> hotkeysData)
	{
		CreateHotkeyElements(hotkeysData);
	}

	private void CreateHotkeyElements(IEnumerable<HotkeyData> hotkeysData)
	{
		BeamTeam.DestroyAllChildren(_hotkeyElementContainer.transform);
		_elements = new List<HotkeyElementView>();
		int num = 0;
		foreach (HotkeyData hotkeysDatum in hotkeysData)
		{
			HotkeyElementView hotkeyElementView = Object.Instantiate<HotkeyElementView>(_hotkeyElementPrefab);
			((Component)hotkeyElementView).transform.SetParent(_hotkeyElementContainer.transform, false);
			hotkeyElementView.Initialize(hotkeysDatum);
			_elements.Add(hotkeyElementView);
			num++;
			if (num > _layout.Elements - 1)
			{
				break;
			}
		}
		_layout.DoLayout(_elements);
	}

	private void Awake()
	{
		Visible = _container.activeSelf;
	}

	public override void Show()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject, true);
		_container.SetActive(true);
		foreach (HotkeyElementView element in _elements)
		{
			element.Show();
		}
		FadeInCanvas_OnBegin();
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeInCanvas_OnUpdate, ((Component)_canvasGroup).transform.localScale.x, 1f, 0.15f).setOnComplete((Action)FadeInCanvas_OnComplete).setEase((LeanTweenType)6)
			.setIgnoreTimeScale(true);
		base.Show();
	}

	public override void Hide()
	{
		LeanTween.cancel(((Component)this).gameObject, true);
		foreach (HotkeyElementView element in _elements)
		{
			element.Hide();
		}
		_container.SetActive(false);
		FadeInCanvas_OnComplete();
		base.Hide();
		base.Hide();
	}

	private void FadeInCanvas_OnBegin()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_canvasGroup.alpha = 0f;
		((Component)_canvasGroup).transform.localScale = 0.6f * Vector3.one;
	}

	private void FadeInCanvas_OnUpdate(float value)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_canvasGroup.alpha = value;
		((Component)_canvasGroup).transform.localScale = (0.6f + 0.4f * value) * Vector3.one;
	}

	private void FadeInCanvas_OnComplete()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_canvasGroup.alpha = 1f;
		((Component)_canvasGroup).transform.localScale = Vector3.one;
	}
}
