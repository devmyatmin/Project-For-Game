using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beam;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
	private static readonly string DEBUG_HEADER = "<color=yellow>AssetBundleManager::</color> ";

	private Dictionary<string, Bundle> _bundles = new Dictionary<string, Bundle>();

	private List<LoadOperationBase> _operations = new List<LoadOperationBase>();

	public static bool IsLoading
	{
		get
		{
			AssetBundleManager instance = Singleton<AssetBundleManager>.Instance;
			if (instance == null)
			{
				return false;
			}
			return instance._operations.Count > 0;
		}
	}

	protected override void OnDestroy()
	{
		UnloadAllAssetBundles();
		base.OnDestroy();
	}

	private void Update()
	{
		int num = 0;
		while (num < _operations.Count)
		{
			LoadOperationBase loadOperationBase = _operations[num];
			loadOperationBase.DoUpdate();
			if (loadOperationBase.IsDone)
			{
				_operations.RemoveAt(num);
				ProcessFinishedOperation(loadOperationBase);
			}
			else
			{
				num++;
			}
		}
	}

	private void ProcessFinishedOperation(LoadOperationBase operation)
	{
		if (operation is LoadAssetBundleOperation loadAssetBundleOperation)
		{
			_bundles.TryGetValue(loadAssetBundleOperation.AssetBundleName, out var value);
			if (value == null)
			{
				return;
			}
			value.AssetBundle = loadAssetBundleOperation.AssetBundle;
		}
		if (operation is LoadAssetFromBundleOperation && operation.Aborted)
		{
			UnloadAssetBundle(operation.AssetBundleName);
		}
	}

	private void LoadAssetBundle(string assetBundleName)
	{
		_bundles.TryGetValue(assetBundleName, out var value);
		if (value != null)
		{
			value.ReferenceCount++;
			return;
		}
		_bundles.Add(assetBundleName, new Bundle());
		_operations.Add(new LoadAssetBundleOperation(assetBundleName));
	}

	public void UnloadAssetBundle(string assetBundleName)
	{
		_bundles.TryGetValue(assetBundleName, out var value);
		if (value == null)
		{
			return;
		}
		if (value.ReferenceCount - 1 == 0)
		{
			foreach (LoadOperationBase operation in _operations)
			{
				if (operation.AssetBundleName.Equals(assetBundleName, StringComparison.InvariantCultureIgnoreCase))
				{
					operation.Abort();
					if (operation is LoadAssetFromBundleOperation)
					{
						return;
					}
				}
			}
		}
		if (--value.ReferenceCount == 0)
		{
			value.Unload();
			_bundles.Remove(assetBundleName);
			Resources.UnloadUnusedAssets();
		}
	}

	private void UnloadAllAssetBundles()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		foreach (LoadOperationBase operation in _operations)
		{
			operation.Abort();
		}
		foreach (Bundle item in _bundles.Values.Reverse())
		{
			item.Unload();
		}
		_bundles.Clear();
		Resources.UnloadUnusedAssets();
		Debug.Log((object)(DEBUG_HEADER + "Unloaded all asset bundles."));
	}

	public AssetBundle GetAssetBundle(string assetBundleName)
	{
		_bundles.TryGetValue(assetBundleName, out var value);
		return value?.AssetBundle;
	}

	private LoadAssetFromBundleOperationBase LoadAsset(string assetName, string assetBundleName)
	{
		LoadAssetBundle(assetBundleName);
		LoadAssetFromBundleOperation loadAssetFromBundleOperation = new LoadAssetFromBundleOperation(assetName, assetBundleName);
		_operations.Add(loadAssetFromBundleOperation);
		return loadAssetFromBundleOperation;
	}

	public Coroutine LoadAssetAsync<TObject>(string assetName, string assetBundleName, Action<TObject> completed, Action failed) where TObject : Object
	{
		return ((MonoBehaviour)this).StartCoroutine(LoadAsset_Sequence(assetName, assetBundleName, completed, failed));
	}

	public IEnumerator LoadAsset_Sequence<TObject>(string assetName, string assetBundleName, Action<TObject> completed, Action failed) where TObject : Object
	{
		float startTime = Time.realtimeSinceStartup;
		int id = Time.frameCount;
		LoadAssetFromBundleOperationBase operation = LoadAsset(assetName, assetBundleName);
		while (!operation.IsDone)
		{
			yield return null;
		}
		if (operation.Aborted)
		{
			Debug.Log((object)(DEBUG_HEADER + $"Aborted loading asset [{assetName}] from asset bundle [{operation.AssetBundleName}], Id: {id}"));
			yield break;
		}
		TObject asset = operation.GetAsset<TObject>();
		if ((Object)(object)asset != (Object)null)
		{
			Debug.Log((object)(DEBUG_HEADER + $"<color=green>Loaded asset</color> [{assetName}] in {Time.realtimeSinceStartup - startTime} seconds, Id: {id}"));
			completed?.Invoke(asset);
		}
		else
		{
			Debug.LogError((object)(DEBUG_HEADER + $"<color=red>Failed to load asset</color> [{assetName}] from asset bundle [{assetBundleName}]! Id: {id}"));
			failed?.Invoke();
		}
	}
}
