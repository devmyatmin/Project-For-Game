namespace Ceto.Common.Unity.Utility;

public class NotifyOnRenderObject : NotifyOnEvent
{
	private void OnRenderObject()
	{
		OnEvent();
	}
}
