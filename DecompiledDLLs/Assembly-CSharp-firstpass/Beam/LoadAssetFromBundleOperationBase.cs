using UnityEngine;

namespace Beam;

public abstract class LoadAssetFromBundleOperationBase : LoadOperationBase
{
	protected LoadAssetFromBundleOperationBase(string assetBundleName)
		: base(assetBundleName)
	{
	}

	public abstract TObject GetAsset<TObject>() where TObject : Object;
}
