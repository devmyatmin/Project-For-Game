using System.Globalization;
using System.Threading;
using UnityEngine;

namespace Beam;

public static class FixCultureRuntime
{
	public static bool FIX_DECIMAL_SEPARATORS;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void FixCulture()
	{
		string text = Thread.CurrentThread.CurrentCulture.ToString();
		if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(","))
		{
			FIX_DECIMAL_SEPARATORS = true;
			Debug.Log((object)"CULTURE: System culture uses comma instead of decimal point...");
		}
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		Debug.Log((object)("CULTURE: Changed culture from [" + text + "] to CultureInfo.InvariantCulture."));
	}
}
