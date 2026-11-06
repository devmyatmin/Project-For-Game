namespace Ceto.Common.Unity.Utility;

public class NotifyOnWillRender : NotifyOnEvent
{
	private void OnWillRenderObject()
	{
		OnEvent();
	}
}
