using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Beam;

public class LoadAssetBundleOperation : LoadOperationBase
{
	private readonly UnityWebRequest _uwr;

	private readonly UnityWebRequestAsyncOperation _request;

	public override bool IsDone
	{
		get
		{
			if (_request == null || !((AsyncOperation)_request).isDone)
			{
				return Aborted;
			}
			return true;
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

	public AssetBundle AssetBundle { get; private set; }

	public LoadAssetBundleOperation(string assetBundleName)
		: base(assetBundleName)
	{
		string text = Path.Combine(FilePath.ASSET_BUNDLES_FOLDER, AssetBundleName);
		_uwr = UnityWebRequestAssetBundle.GetAssetBundle("file://" + text);
		_request = _uwr.SendWebRequest();
	}

	public override void DoUpdate()
	{
		if (!Aborted && ((AsyncOperation)_request).isDone)
		{
			if (_uwr.isNetworkError || _uwr.isHttpError)
			{
				Debug.LogError((object)_uwr.error);
			}
			AssetBundle = DownloadHandlerAssetBundle.GetContent(_uwr);
			_ = (Object)(object)AssetBundle == (Object)null;
			_uwr.Dispose();
		}
	}

	protected override void OnAborted()
	{
		_uwr.Abort();
		_uwr.Dispose();
		base.OnAborted();
	}
}
