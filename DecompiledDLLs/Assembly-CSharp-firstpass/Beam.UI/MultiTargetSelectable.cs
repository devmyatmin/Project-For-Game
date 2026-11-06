using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class MultiTargetSelectable : Selectable
{
	private Graphic[] _graphics;

	protected Graphic[] Graphics
	{
		get
		{
			if (_graphics == null)
			{
				_graphics = ((Component)((Component)((Selectable)this).targetGraphic).transform.parent).GetComponentsInChildren<Graphic>();
			}
			return _graphics;
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		ColorBlock colors;
		Color val;
		switch ((int)state)
		{
		case 0:
			colors = ((Selectable)this).colors;
			val = colors.normalColor;
			break;
		case 1:
			colors = ((Selectable)this).colors;
			val = colors.highlightedColor;
			break;
		case 2:
			colors = ((Selectable)this).colors;
			val = colors.pressedColor;
			break;
		case 4:
			colors = ((Selectable)this).colors;
			val = colors.disabledColor;
			break;
		default:
			val = Color.black;
			break;
		}
		if (((Component)this).gameObject.activeInHierarchy)
		{
			if ((int)((Selectable)this).transition != 1)
			{
				throw new NotSupportedException();
			}
			Color val2 = val;
			colors = ((Selectable)this).colors;
			ColorTween(val2 * colors.colorMultiplier, instant);
		}
	}

	private void ColorTween(Color targetColor, bool instant)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Selectable)this).targetGraphic == (Object)null)
		{
			return;
		}
		Graphic[] graphics = Graphics;
		foreach (Graphic obj in graphics)
		{
			float num;
			if (instant)
			{
				num = 0f;
			}
			else
			{
				ColorBlock colors = ((Selectable)this).colors;
				num = colors.fadeDuration;
			}
			obj.CrossFadeColor(targetColor, num, true, true);
		}
	}
}
