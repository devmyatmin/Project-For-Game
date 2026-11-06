using UnityEngine.EventSystems;

namespace uTools;

public interface uIPointHandler : IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerExitHandler
{
	void OnPointerEnter(PointerEventData eventData);

	void OnPointerDown(PointerEventData eventData);

	void OnPointerClick(PointerEventData eventData);

	void OnPointerUp(PointerEventData eventData);

	void OnPointerExit(PointerEventData eventData);
}
