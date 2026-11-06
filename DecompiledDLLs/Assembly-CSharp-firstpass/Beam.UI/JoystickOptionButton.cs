namespace Beam.UI;

public class JoystickOptionButton : OptionButtonDecoratorBase
{
	public string DeviceInstanceId { get; }

	public JoystickOptionButton(IOptionButtonViewAdapter button, string deviceInstanceId)
		: base(button)
	{
		DeviceInstanceId = deviceInstanceId;
	}
}
