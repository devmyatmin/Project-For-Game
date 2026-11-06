using UnityEngine;

namespace Beam;

public class LoadAssetFromBundleOperation : LoadAssetFromBundleOperationBase
{
	private AssetBundleRequest _request;

	public override bool IsDone
	{
		get
		{
			AssetBundleRequest request = _request;
			if (request == null)
			{
				return Aborted;
			}
			return ((AsyncOperation)request).isDone;
		}
	}

	public override bool Loaded
	{
		get
		{
			if (_request != null)
			{
				return ((AsyncOperation)_request).isDone;
			}
			return false;
		}
	}

	public string AssetName { get; }

	public LoadAssetFromBundleOperation(string assetName, string assetBundleName)
		: base(assetBundleName)
	{
		AssetName = assetName;
	}

	public override void DoUpdate()
	{
		if (_request == null)
		{
			AssetBundle assetBundle = Singleton<AssetBundleManager>.Instance.GetAssetBundle(AssetBundleName);
			if ((Object)(object)assetBundle != (Object)null)
			{
				_request = assetBundle.LoadAssetAsync<Object>(AssetName);
			}
		}
	}

	protected override void OnAborted()
	{
		base.OnAborted();
	}

	public override TObject GetAsset<TObject>()
	{
		if (_request != null && ((AsyncOperation)_request).isDone)
		{
			Object asset = _request.asset;
			return (TObject)(object)((asset is TObject) ? asset : null);
		}
		return default;
	}
}
