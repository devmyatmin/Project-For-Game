using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI;

public class NullLabelViewAdapter : NullViewAdapter<NullLabelViewAdapter>, ILabelViewAdapter, IViewAdapter
{
	public string Text { get; set; }

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
}
