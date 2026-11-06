using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public abstract class HotkeyLayout : ScriptableObject
{
	[SerializeField]
	private int _elements;

	public int Elements => _elements;

	public abstract void DoLayout(IList<HotkeyElementView> hotkeyElements);
}
