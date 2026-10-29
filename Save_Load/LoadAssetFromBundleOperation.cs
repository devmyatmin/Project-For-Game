using UnityEngine;

namespace Beam;

public class LoadAssetFromBundleOperation : LoadAssetFromBundleOperationBase
{
	private AssetBundleRequest _request;

	public override bool IsDone => _request?.isDone ?? base.Aborted;

	public override bool Loaded
	{
		get
		{
			if (_request != null)
			{
				return _request.isDone;
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
			AssetBundle assetBundle = Singleton<AssetBundleManager>.Instance.GetAssetBundle(base.AssetBundleName);
			if (assetBundle != null)
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
		if (_request != null && _request.isDone)
		{
			return _request.asset as TObject;
		}
		return null;
	}
}
