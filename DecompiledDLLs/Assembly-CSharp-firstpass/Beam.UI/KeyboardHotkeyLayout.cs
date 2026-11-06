using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public class KeyboardHotkeyLayout : HotkeyLayout
{
	public override void DoLayout(IList<HotkeyElementView> hotkeyElements)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min(Elements, hotkeyElements.Count);
		for (int i = 0; i < num; i++)
		{
			HotkeyElementView hotkeyElementView = hotkeyElements[i];
			((Component)hotkeyElementView).transform.localPosition = Vector3.zero;
			((Component)hotkeyElementView).transform.localRotation = Quaternion.identity;
		}
	}
}
