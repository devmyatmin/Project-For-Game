using System;
using System.Collections.Generic;
using Beam.Utilities;
using Rewired;
using SharpNeatLib.Maths;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public abstract class RadialMenuPresenterBase<TModel, TElement, TView> : ControllerBase where TModel : class, IRadialMenuModel where TElement : MonoBehaviour, IRadialMenuElement<TModel> where TView : RadialMenuViewAdapterBase<TModel, TElement>
{
	public enum EElementDistributionType
	{
		None,
		Even,
		FanAroundNorth
	}

	[SerializeField]
	private TView _view;

	protected Player Input;

	[SerializeField]
	private float _sensitivity = 7f;

	[SerializeField]
	private int _elementLayoutCount = 8;

	[SerializeField]
	private EElementDistributionType _elementsDistributionType = EElementDistributionType.Even;

	[SerializeField]
	private float _angleSteps;

	[SerializeField]
	private float _elementRadius = 240f;

	[SerializeField]
	private float _selectorRadius;

	[SerializeField]
	private float _rotationAngle;

	[Range(1f, 100f)]
	[SerializeField]
	private float _angleThreshold = 100f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _activationPercentage = 0.7f;

	[SerializeField]
	private float _easeTime = 0.15f;

	[SerializeField]
	private LeanTweenType _easeInType = (LeanTweenType)6;

	[SerializeField]
	private LeanTweenType _easeOutType = (LeanTweenType)5;

	private IList<IList<TModel>> _models;

	private Stack<TElement> _elementsStack = new Stack<TElement>();

	private List<TElement> _activeElements = new List<TElement>();

	private List<TElement> _extraElements = new List<TElement>();

	private TElement _currentElement;

	private HashSet<int> _reservedIndexes = new HashSet<int>();

	private Vector3 _selectorMove;

	private Vector3 _selectingPosition;

	private bool _animating;

	private bool _active;

	private int _fadeInTweenId;

	private int _fadeOutTweenId;

	private bool _secondaryPressTime;

	private float _secondaryPressTimer;

	private FastRandom _random = new FastRandom();

	[SerializeField]
	private UnityEvent _opened;

	[SerializeField]
	private UnityEvent _closed;

	public TView View => _view;

	public virtual string Title { get; set; } = string.Empty;

	public int ElementLayoutCount
	{
		get
		{
			return _elementLayoutCount;
		}
		set
		{
			_elementLayoutCount = value;
		}
	}

	protected List<TElement> ActiveElements => _activeElements;

	protected TElement CurrentElement => _currentElement;

	public event Action<IRadialMenuElement<TModel>> SelectionChanged;

	public event Action<IRadialMenuElement<TModel>> ElementClick;

	public event Action<IRadialMenuElement<TModel>> ElementSecondaryClick;

	public event Action<IRadialMenuElement<TModel>> ElementSecondaryShortPress;

	protected virtual void OnSelectionChanged(IRadialMenuElement<TModel> element)
	{
		SelectionChanged?.Invoke(element);
	}

	protected virtual void OnElementClick(IRadialMenuElement<TModel> element)
	{
		ElementClick?.Invoke(element);
	}

	protected virtual void OnElementSecondaryClick(IRadialMenuElement<TModel> element)
	{
		ElementSecondaryClick?.Invoke(element);
	}

	protected virtual void OnElementSecondaryShortPress(IRadialMenuElement<TModel> element)
	{
		ElementSecondaryShortPress?.Invoke(element);
	}

	protected virtual void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)(((object)this).GetType()?.ToString() + ":: No view component assigned!"));
			_view = NullRadialMenuViewAdapterBase.Instance as TView;
		}
		CreateElements();
		Hide();
	}

	protected virtual void OnDestroy()
	{
		SelectionChanged = null;
		ElementClick = null;
		ElementSecondaryClick = null;
		ElementSecondaryShortPress = null;
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as TView;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)(((object)this).GetType()?.ToString() + ":: No view component assigned!"));
			_view = NullRadialMenuViewAdapterBase.Instance as TView;
		}
	}

	public void Initialize(IList<IList<TModel>> models)
	{
		_models = models;
	}

	private void Update()
	{
		if (Input.GetButtonDown(30))
		{
			Element_Click(_currentElement);
		}
		else if (_secondaryPressTime)
		{
			float buttonShortPressTime = Input.controllers.maps.GetInputBehavior(0).buttonShortPressTime;
			_secondaryPressTimer += Time.deltaTime;
			if (_secondaryPressTimer > buttonShortPressTime || !Input.GetButton(31))
			{
				_secondaryPressTime = false;
				if (_secondaryPressTimer < buttonShortPressTime)
				{
					Element_SecondaryClick(_currentElement);
				}
				else
				{
					Element_SecondaryShortPress(_currentElement);
				}
			}
		}
		else if (Input.GetButtonDown(31))
		{
			_secondaryPressTimer = 0f;
			_secondaryPressTime = true;
		}
	}

	public virtual void Show()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject, true);
		_selectingPosition = Vector3.zero;
		Refresh();
		FadeInCanvas_OnBegin();
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeInCanvas_OnUpdate, _view.CanvasGroup.transform.localScale.x, 1f, _easeTime).setOnComplete((Action)FadeInCanvas_OnComplete).setEase(_easeInType)
			.setIgnoreTimeScale(true);
		UnityEvent opened = _opened;
		if (opened != null)
		{
			opened.Invoke();
		}
		_view.Show();
	}

	public virtual void Hide()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject, true);
		_currentElement = default;
		_animating = true;
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeInCanvas_OnUpdate, _view.CanvasGroup.transform.localScale.x, 0f, _easeTime).setOnComplete((Action)(() =>
		{
			FadeInCanvas_OnComplete();
			UnityEvent closed = _closed;
			if (closed != null)
			{
				closed.Invoke();
			}
			_view.Hide();
		})).setEase(_easeOutType)
			.setIgnoreTimeScale(true);
	}

	public void Refresh()
	{
		LeanTween.cancel(((Component)this).gameObject, true);
		Element_Exit(_currentElement);
		_currentElement = default;
		PoolElements();
		Layout();
	}

	private TElement CreateElement()
	{
		return Object.Instantiate<TElement>(_view.ElementPrefab);
	}

	private void CreateElements()
	{
		for (int i = 0; i < _elementLayoutCount; i++)
		{
			TElement element = CreateElement();
			PushElement(element);
		}
	}

	private void PoolElements()
	{
		foreach (TElement activeElement in _activeElements)
		{
			PushElement(activeElement);
		}
		foreach (TElement extraElement in _extraElements)
		{
			((Component)(object)extraElement).transform.SetParent(_view.PooledElementContainer.Transform, false);
		}
		_activeElements.Clear();
		_extraElements.Clear();
		_reservedIndexes.Clear();
		BeamTeam.DestroyAllChildren(_view.ElementContainer);
	}

	protected TElement PopElement()
	{
		TElement val = ((_elementsStack.Count > 0) ? _elementsStack.Pop() : CreateElement());
		((Component)(object)val).transform.SetParent(_view.ElementContainer.Transform, false);
		return val;
	}

	private void PushElement(TElement element)
	{
		_elementsStack.Push(element);
		((Component)(object)element).transform.SetParent(_view.PooledElementContainer.Transform, false);
	}

	public void UpdateSelector()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		float num = Input.GetAxis(24) / (50f / _sensitivity);
		float num2 = Input.GetAxis(25) / (50f / _sensitivity);
		float num3 = Mathf.Clamp(num, -1f, 1f);
		float num4 = Mathf.Clamp(num2, -1f, 1f);
		Vector3 selectingPosition = _selectingPosition;
		_selectorMove = selectingPosition + new Vector3(num3, num4, 0f) * _elementRadius;
		_view.SelectorImage.transform.localPosition = GetConstrainedPosition(((Component)this).transform.localPosition, _selectorMove);
		_selectingPosition = GetConstrainedPosition(((Component)this).transform.localPosition, _selectorMove);
		Vector2 val = Vector2.op_Implicit(_view.SelectorImage.transform.position - ((Component)this).transform.position);
		float num5 = Mathf.Atan2(val.y, val.x) * 57.29578f;
		_view.SelectorImage.transform.rotation = Quaternion.AngleAxis(num5 - 90f, Vector3.forward);
		CheckSelectorPosition(_selectingPosition);
	}

	private void CheckSelectorPosition(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		CheckElements(_activeElements, position);
		CheckElements(_extraElements, position);
	}

	private void CheckElements(IList<TElement> elements, Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		float num = Angle(new Vector2(position.x, position.y));
		float angleStep = GetAngleStep();
		foreach (TElement element in elements)
		{
			float num2 = Angle(Vector2.op_Implicit(((Component)(object)element).transform.position - ((Component)this).transform.position));
			if (Mathf.Abs(Mathf.DeltaAngle(num, num2)) < angleStep / 2f * _angleThreshold / 100f && _active && !_animating)
			{
				if ((Object)(object)_currentElement != (Object)(object)element)
				{
					if (!BeamTeam.IsNullOrDestroyed(_currentElement))
					{
						Element_Exit(_currentElement);
					}
					_currentElement = element;
					Element_Enter(element);
				}
			}
			else if ((Object)(object)_currentElement == (Object)(object)element)
			{
				Element_Exit(element);
				_currentElement = default;
			}
		}
	}

	private void LayoutElementPlaceholders()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_random.Reinitialise(1);
		int num = Mathf.Min(_reservedIndexes.Count + (_models?.Count ?? 0), _elementLayoutCount);
		for (int i = 0; i < num; i++)
		{
			Transform val = Object.Instantiate<Transform>(_view.ElementPlaceholderPrefab.Transform);
			((Component)val).transform.SetParent(_view.ElementContainer.Transform, false);
			((Component)val).transform.SetAsFirstSibling();
			float angleForIndex = GetAngleForIndex(i);
			float num2 = Mathf.Sin(angleForIndex);
			float num3 = Mathf.Cos(angleForIndex);
			((Component)val).transform.localPosition = new Vector3(num2, num3, 0f) * _elementRadius;
			((Component)val).transform.localEulerAngles = new Vector3(0f, 0f, (float)_random.Next() * 360f);
		}
	}

	protected virtual void Layout()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		LayoutElementPlaceholders();
		if (_models == null || _models.Count == 0)
		{
			return;
		}
		float rotationOffsetDeg = GetRotationOffsetDeg();
		int i = 0;
		int num = 0;
		for (; i < _elementLayoutCount; i++)
		{
			if (!_reservedIndexes.Contains(i))
			{
				TElement val = PopElement();
				float angleForIndex = GetAngleForIndex(i);
				float num2 = Mathf.Sin(angleForIndex);
				float num3 = Mathf.Cos(angleForIndex);
				((Component)(object)val).transform.localPosition = new Vector3(num2, num3, 0f) * _elementRadius;
				((Component)(object)val).transform.localEulerAngles = new Vector3(0f, 0f, 0f - rotationOffsetDeg);
				val.Initialize(_models[num]);
				_activeElements.Add(val);
				num++;
				if (num > _models.Count - 1)
				{
					break;
				}
			}
		}
		_view.ElementContainer.Transform.localEulerAngles = new Vector3(0f, 0f, rotationOffsetDeg);
	}

	public void SelectCurrentElement()
	{
		Element_Select(_currentElement);
	}

	protected virtual void InsertElement(TElement element, int index)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!_reservedIndexes.Contains(index))
		{
			((Component)(object)element).transform.SetParent(_view.ElementContainer.Transform, false);
			float rotationOffsetDeg = GetRotationOffsetDeg();
			float num = (float)Math.PI * 2f / (float)_elementLayoutCount * (float)index;
			float num2 = Mathf.Sin(num);
			float num3 = Mathf.Cos(num);
			((Component)(object)element).transform.localPosition = new Vector3(num2, num3, 0f) * _elementRadius;
			((Component)(object)element).transform.localEulerAngles = new Vector3(0f, 0f, 0f - rotationOffsetDeg);
			_extraElements.Add(element);
			_reservedIndexes.Add(index);
		}
	}

	protected virtual void InsertElementAtAngle(TElement element, float ang, bool respectOffset = false)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		((Component)(object)element).transform.SetParent(_view.ElementContainer.Transform, false);
		float rotationOffsetDeg = GetRotationOffsetDeg();
		float num = (respectOffset ? ang : (ang + rotationOffsetDeg)) * ((float)Math.PI / 180f);
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		((Component)(object)element).transform.localPosition = new Vector3(num2, num3, 0f) * _elementRadius;
		((Component)(object)element).transform.localEulerAngles = new Vector3(0f, 0f, 0f - rotationOffsetDeg);
		_extraElements.Add(element);
	}

	private void Element_Select(TElement element)
	{
		if ((Object)(object)element != (Object)null)
		{
			OnSelectionChanged(element);
		}
	}

	protected virtual void Element_Enter(TElement element)
	{
		LeanTween.cancel(((Component)this).gameObject, _fadeOutTweenId);
		element.Enter();
		TModel selectedValue = element.SelectedValue;
		if (selectedValue == null)
		{
			Clear();
		}
		else
		{
			_view.TitleLabel.Text = selectedValue.GetDisplayName();
			_view.DescriptionLabel.Text = selectedValue.GetDescription();
		}
		FadeDetailsCanvasGroup_OnUpdate(1f);
	}

	private void Element_Exit(TElement element)
	{
		if (!((Object)(object)element == (Object)null))
		{
			element.Exit();
			if (element.SelectedValue == null)
			{
				FadeDetailsCanvasGroup_OnUpdate(1f);
			}
			else
			{
				_fadeOutTweenId = LeanTween.value(((Component)this).gameObject, (Action<float>)FadeDetailsCanvasGroup_OnUpdate, _view.DetailsCanvasGroup.Alpha, 0f, 0.2f).setOnComplete((Action)FadeDetailsCanvasGroup_OnComplete).setEase((LeanTweenType)6)
					.setIgnoreTimeScale(true)
					.id;
			}
		}
	}

	private void Element_Click(TElement element)
	{
		if ((Object)(object)element != (Object)null)
		{
			OnElementClick(element);
		}
	}

	private void Element_SecondaryClick(TElement element)
	{
		if ((Object)(object)element != (Object)null)
		{
			OnElementSecondaryClick(element);
		}
	}

	private void Element_SecondaryShortPress(TElement element)
	{
		if ((Object)(object)element != (Object)null)
		{
			OnElementSecondaryShortPress(element);
		}
	}

	public void NavigateLeft()
	{
		if ((Object)(object)_currentElement != (Object)null)
		{
			_currentElement.NavigateLeft();
			Element_Enter(_currentElement);
		}
	}

	public void NavigateRight()
	{
		if ((Object)(object)_currentElement != (Object)null)
		{
			_currentElement.NavigateRight();
			Element_Enter(_currentElement);
		}
	}

	private void FadeInCanvas_OnBegin()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		_animating = true;
		_view.CanvasGroup.Alpha = 0f;
		_view.CanvasGroup.transform.localScale = 0.6f * Vector3.one;
		_view.DetailsCanvasGroup.Alpha = 1f;
		Clear();
	}

	private void FadeInCanvas_OnUpdate(float value)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_view.CanvasGroup.Alpha = value;
		_view.CanvasGroup.transform.localScale = (0.6f + 0.4f * value) * Vector3.one;
	}

	private void FadeInCanvas_OnComplete()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		_animating = false;
		_view.CanvasGroup.Alpha = 1f;
		_view.CanvasGroup.transform.localScale = Vector3.one;
	}

	private void FadeDetailsCanvasGroup_OnUpdate(float value)
	{
		_view.DetailsCanvasGroup.Alpha = value;
	}

	private void FadeDetailsCanvasGroup_OnComplete()
	{
		Clear();
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeDetailsCanvasGroup_OnUpdate, _view.DetailsCanvasGroup.Alpha, 1f, 0.2f).setEase((LeanTweenType)6).setIgnoreTimeScale(true);
	}

	protected virtual void Clear()
	{
		_view.TitleLabel.Text = Title;
		_view.DescriptionLabel.Text = "";
	}

	private float GetAngleStep()
	{
		if (_elementsDistributionType == EElementDistributionType.Even)
		{
			return 360f / (float)_elementLayoutCount;
		}
		return _angleSteps;
	}

	private float GetAngleForIndex(int index)
	{
		if (_elementsDistributionType == EElementDistributionType.Even)
		{
			return (float)Math.PI * 2f / (float)_elementLayoutCount * (float)index;
		}
		return GetAngleStep() * ((float)Math.PI / 180f) * (float)index;
	}

	private float GetRotationOffsetDeg()
	{
		return _elementsDistributionType switch
		{
			EElementDistributionType.Even => 0f, 
			EElementDistributionType.FanAroundNorth => (float)(_elementLayoutCount - 1) * GetAngleStep() / 2f, 
			_ => _rotationAngle, 
		};
	}

	private Vector3 GetConstrainedPosition(Vector3 midPoint, Vector3 endPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = midPoint - endPoint;
		float magnitude = val.magnitude;
		_active = magnitude > _selectorRadius * _activationPercentage;
		if (magnitude > _selectorRadius)
		{
			val = endPoint - midPoint;
			return val.normalized * _selectorRadius + midPoint;
		}
		return endPoint;
	}

	private float Angle(Vector2 angle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (angle.x < 0f)
		{
			return 360f - Mathf.Atan2(angle.x, angle.y) * 57.29578f * -1f;
		}
		return Mathf.Atan2(angle.x, angle.y) * 57.29578f;
	}

	protected RadialMenuPresenterBase()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
	}
}
