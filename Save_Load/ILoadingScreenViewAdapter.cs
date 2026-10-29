namespace Beam.UI;

public interface ILoadingScreenViewAdapter : IViewAdapter
{
	ILabelViewAdapter LoadingStatusLabel { get; }

	ILabelViewAdapter HintLabel { get; }
}
