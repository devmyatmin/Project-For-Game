namespace Beam.UI;

public interface IScrollviewViewAdapter : IViewAdapter
{
	ITransformViewAdapter Content { get; }

	float NormalizedHorizontalPosition { get; set; }

	float NormalizedVerticalPosition { get; set; }
}
