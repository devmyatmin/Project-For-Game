using System.Runtime.CompilerServices;
using Beam.UI;
using UnityEngine;

public class NullImageViewAdapter : NullViewAdapter<NullImageViewAdapter>, IImageViewAdapter, IViewAdapter
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
}
