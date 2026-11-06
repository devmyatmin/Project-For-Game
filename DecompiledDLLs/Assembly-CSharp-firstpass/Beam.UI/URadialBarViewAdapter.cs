using UnityEngine;

namespace Beam.UI;

public class URadialBarViewAdapter : RadialBarViewAdapterBase
{
	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UFilledImageViewAdapter _radialBar;

	[SerializeField]
	private UImageViewAdapter _iconImage;

	[SerializeField]
	private Sprite _healthSprite;

	[SerializeField]
	private Sprite _waterSprite;

	[SerializeField]
	private Sprite _fuelSprite;

	[SerializeField]
	private Sprite _completionSprite;

	[SerializeField]
	private Sprite _constructionSprite;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override IFilledImageViewAdapter RadialBar => _radialBar;

	public override void SetIcon(IconType iconType)
	{
		Sprite sprite = iconType switch
		{
			IconType.Water => _waterSprite, 
			IconType.Fuel => _fuelSprite, 
			IconType.Completion => _completionSprite, 
			IconType.Construction => _constructionSprite, 
			_ => _healthSprite, 
		};
		_iconImage.Sprite = sprite;
	}
}
