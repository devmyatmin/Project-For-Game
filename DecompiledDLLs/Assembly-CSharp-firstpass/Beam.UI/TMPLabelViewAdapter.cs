using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class TMPLabelViewAdapter : LabelViewAdapterBase<TextMeshProUGUI>
{
	public override string Text
	{
		get
		{
			return ((TMP_Text)TextField).text;
		}
		set
		{
			if (((TMP_Text)TextField).text != value)
			{
				((TMP_Text)TextField).text = value;
				OnTextChanged(this, EventArgs.Empty);
			}
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

	public bool EnableAutoSizing
	{
		get
		{
			return ((TMP_Text)TextField).enableAutoSizing;
		}
		set
		{
			((TMP_Text)TextField).enableAutoSizing = value;
		}
	}

	public float FontSizeMax
	{
		get
		{
			return ((TMP_Text)TextField).fontSizeMax;
		}
		set
		{
			if (((TMP_Text)TextField).enableAutoSizing)
			{
				((TMP_Text)TextField).fontSizeMax = value;
			}
		}
	}

	public float FontSizeMin
	{
		get
		{
			return ((TMP_Text)TextField).fontSizeMin;
		}
		set
		{
			if (((TMP_Text)TextField).enableAutoSizing)
			{
				((TMP_Text)TextField).fontSizeMin = value;
			}
		}
	}

	public bool EnableWordWrapping
	{
		get
		{
			return ((TMP_Text)TextField).enableWordWrapping;
		}
		set
		{
			((TMP_Text)TextField).enableWordWrapping = value;
		}
	}

	public event Action<ILabelViewAdapter, EventArgs> TextChanged;

	protected virtual void OnTextChanged(ILabelViewAdapter sender, EventArgs args)
	{
		TextChanged?.Invoke(sender, args);
	}

	protected virtual void OnDestroy()
	{
		TextChanged = null;
	}
}
