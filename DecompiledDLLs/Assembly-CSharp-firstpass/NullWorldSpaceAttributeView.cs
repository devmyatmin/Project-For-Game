public class NullWorldSpaceAttributeView : WorldSpaceAttributeViewBase
{
	public static NullWorldSpaceAttributeView Instance { get; } = new NullWorldSpaceAttributeView();

	public override void Show(IHaveAttribute attribute)
	{
	}

	public override void Hide()
	{
	}
}
