using Rewired;

namespace Beam.UI;

public class InputFieldButtonInfo
{
	public ControllerMap ControllerMap;

	public int ActionId;

	public int ActionElementMapId = -1;

	public AxisRange ActionRange;

	public bool AllowOnlyAxesMapping;
}
