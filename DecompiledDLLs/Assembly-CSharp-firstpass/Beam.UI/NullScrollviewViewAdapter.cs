namespace Beam.UI;

public class NullScrollviewViewAdapter : ViewAdapterBase, IScrollviewViewAdapter, IViewAdapter
{
	private static readonly NullScrollviewViewAdapter _Instance = new NullScrollviewViewAdapter();

	public static NullScrollviewViewAdapter Instance => _Instance;

	public ITransformViewAdapter Content => NullObject<NullTransformViewAdapter>.Instance;

	public float NormalizedHorizontalPosition { get; set; }

	public float NormalizedVerticalPosition { get; set; }
}
