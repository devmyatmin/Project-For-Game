namespace Beam.UI;

public class UFilledImageViewAdapter : UImageViewAdapter, IFilledImageViewAdapter, IImageViewAdapter, IViewAdapter
{
	public float Fill
	{
		get
		{
			return Image.fillAmount;
		}
		set
		{
			Image.fillAmount = value;
		}
	}
}
