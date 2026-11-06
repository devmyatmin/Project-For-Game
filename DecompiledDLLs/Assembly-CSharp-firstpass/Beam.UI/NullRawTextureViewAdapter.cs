using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI;

public class NullRawTextureViewAdapter : UTextureViewAdapter
{
	public static NullRawTextureViewAdapter Instance { get; } = new NullRawTextureViewAdapter();

	public override Texture Texture { get; set; }

	public override Color Color
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
}
