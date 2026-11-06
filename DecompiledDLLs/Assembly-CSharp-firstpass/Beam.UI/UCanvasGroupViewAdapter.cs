using UnityEngine;

namespace Beam.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UCanvasGroupViewAdapter : MonoBehaviour, ICanvasGroupViewAdapter
{
	private CanvasGroup _canvasGroup;

	protected CanvasGroup CanvasGroup
	{
		get
		{
			if ((Object)(object)_canvasGroup == (Object)null)
			{
				_canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
	}

	public bool Interactable
	{
		get
		{
			return CanvasGroup.interactable;
		}
		set
		{
			CanvasGroup.interactable = value;
		}
	}

	public float Alpha
	{
		get
		{
			return CanvasGroup.alpha;
		}
		set
		{
			CanvasGroup.alpha = value;
		}
	}
}
