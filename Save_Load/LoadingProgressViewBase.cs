namespace Beam.UI;

public abstract class LoadingProgressViewBase : ViewAdapterBase, ILoadingProgressView
{
	public abstract void UpdateProgress(float progress);
}
