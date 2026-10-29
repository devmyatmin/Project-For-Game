namespace Beam;

public abstract class LoadOperationBase
{
	public string AssetBundleName { get; }

	public bool Aborted { get; private set; }

	public abstract bool IsDone { get; }

	public abstract bool Loaded { get; }

	protected LoadOperationBase(string assetBundleName)
	{
		AssetBundleName = assetBundleName;
	}

	public abstract void DoUpdate();

	public void Abort()
	{
		if (!Aborted)
		{
			Aborted = true;
			OnAborted();
		}
	}

	protected virtual void OnAborted()
	{
	}
}
