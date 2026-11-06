using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public class DPadHotkeyLayout : HotkeyLayout
{
	[SerializeField]
	private float _elementRadius;

	public override void DoLayout(IList<HotkeyElementView> hotkeyElements)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min(Elements, hotkeyElements.Count);
		for (int i = 0; i < num; i++)
		{
			HotkeyElementView hotkeyElementView = hotkeyElements[i];
			float num2 = (float)Math.PI / 2f * (float)i;
			float num3 = Mathf.Sin(num2);
			float num4 = Mathf.Cos(num2);
			((Component)hotkeyElementView).transform.localPosition = new Vector3(num3, num4, 0f) * _elementRadius;
		}
	}
}
