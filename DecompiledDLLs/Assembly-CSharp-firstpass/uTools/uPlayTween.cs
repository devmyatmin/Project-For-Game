using UnityEngine;
using UnityEngine.EventSystems;

namespace uTools;

[AddComponentMenu("uTools/Internal/Play Tween(uTools)")]
public class uPlayTween : MonoBehaviour, uIPointHandler, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerExitHandler
{
	public uTweener tweenTarget;

	public PlayDirection playDirection = PlayDirection.Forward;

	public Trigger trigger = Trigger.OnPointerClick;

	private void Start()
	{
		if ((Object)(object)tweenTarget == (Object)null)
		{
			tweenTarget = ((Component)this).GetComponent<uTweener>();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TriggerPlay(Trigger.OnPointerEnter);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		TriggerPlay(Trigger.OnPointerDown);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		TriggerPlay(Trigger.OnPointerClick);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		TriggerPlay(Trigger.OnPointerUp);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TriggerPlay(Trigger.OnPointerExit);
	}

	private void TriggerPlay(Trigger _trigger)
	{
		if (_trigger == trigger)
		{
			Play();
		}
	}

	private void Play()
	{
		if (playDirection == PlayDirection.Toggle)
		{
			tweenTarget.Toggle();
		}
		else
		{
			tweenTarget.Play(playDirection);
		}
	}
}
