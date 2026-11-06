using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI;

public class NullFilledImageViewAdapter : NullViewAdapter<NullFilledImageViewAdapter>, IFilledImageViewAdapter, IImageViewAdapter, IViewAdapter
{
	public Color Color
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Sprite Sprite { get; set; }

	public float Fill { get; set; }
}
