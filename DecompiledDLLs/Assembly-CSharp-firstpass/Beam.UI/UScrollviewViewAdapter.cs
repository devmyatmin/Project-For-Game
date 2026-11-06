using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UScrollviewViewAdapter : ScrollviewViewAdapterBase<ScrollRect>
{
	private TransformViewAdapter _content;

	public override ITransformViewAdapter Content
	{
		get
		{
			if (_content == null)
			{
				_content = new TransformViewAdapter((Transform)(object)Scrollview.content);
			}
			return _content;
		}
	}

	public override float NormalizedHorizontalPosition
	{
		get
		{
			return Scrollview.horizontalNormalizedPosition;
		}
		set
		{
			Scrollview.horizontalNormalizedPosition = value;
		}
	}

	public override float NormalizedVerticalPosition
	{
		get
		{
			return Scrollview.verticalNormalizedPosition;
		}
		set
		{
			Scrollview.verticalNormalizedPosition = value;
		}
	}
}
