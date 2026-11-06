using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

[RequireComponent(typeof(Text))]
public class ULabelViewAdapter : LabelViewAdapterBase<Text>
{
	public override string Text
	{
		get
		{
			return TextField.text;
		}
		set
		{
			TextField.text = value;
		}
	}

	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Graphic)TextField).color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)TextField).color = value;
		}
	}

	public override bool Enabled
	{
		get
		{
			return ((Behaviour)TextField).enabled;
		}
		set
		{
			((Behaviour)TextField).enabled = value;
		}
	}
}
