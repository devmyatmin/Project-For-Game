using System;
using System.Collections;
using Beam;
using Beam.UI;
using Beam.Utilities;
using UnityEngine;

public class WatchLoader : MonoBehaviour, IIdleUntilInitialized
{
	public string BUNDLE_NAME = "watches/beepi.unity3d";

	[SerializeField]
	private WatchController _watchController;

	private Transform _watchPosition;

	private Coroutine _loadSequence;

	private GameObject _2dView;

	private GameObject _3dView;

	private bool _assetIsLoaded;

	private bool _initialized;

	public bool IsInitialized => _initialized;

	private void Awake()
	{
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
		}
	}

	private IEnumerator Start()
	{
		while (LevelLoader.IsLoading() || !_initialized)
		{
			yield return null;
		}
		((Component)this).transform.parent = null;
		((Component)this).transform.position = Vector3.zero;
		((Component)this).transform.rotation = Quaternion.Euler(Vector3.zero);
		_watchPosition = _watchController?.Player?.Character?.WatchPosition;
		if ((Object)(object)_watchPosition == (Object)null)
		{
			Debug.LogError((object)"WatchLoader:: No watch position found!");
			yield break;
		}
		_loadSequence = ((MonoBehaviour)this).StartCoroutine(Load_Sequence());
		yield return _loadSequence;
		if ((Object)(object)_2dView == (Object)null || (Object)(object)_3dView == (Object)null)
		{
			LoadingFailed();
			yield break;
		}
		WatchViewAdapterBase componentInChildren = _2dView.GetComponentInChildren<WatchViewAdapterBase>();
		WatchScreenViewAdapterBase componentInChildren2 = _3dView.GetComponentInChildren<WatchScreenViewAdapterBase>();
		if ((Object)(object)componentInChildren == (Object)null || (Object)(object)componentInChildren2 == (Object)null)
		{
			Debug.LogError((object)"WatchLoader:: Failed to find watch components in loaded assets! Unloading and destroying any loaded assets.");
			UnloadAndDestroyAssets();
		}
		else
		{
			_watchController.Initialize(componentInChildren, componentInChildren2);
			_assetIsLoaded = true;
			_loadSequence = null;
		}
	}

	private IEnumerator Load_Sequence()
	{
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("UI - Watch", BUNDLE_NAME, (Action<GameObject>)Create2dView, (Action)LoadingFailed);
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("View - 3D Watch", BUNDLE_NAME, (Action<GameObject>)Create3dView, (Action)LoadingFailed);
	}

	private void Create2dView(GameObject prefab)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			_2dView = Object.Instantiate<GameObject>(prefab);
		}
	}

	private void Create3dView(GameObject prefab)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)prefab == (Object)null))
		{
			_3dView = Object.Instantiate<GameObject>(prefab);
			_3dView.transform.SetParent(((Component)_watchPosition).transform, false);
			_3dView.transform.localPosition = Vector3.zero;
		}
	}

	private void LoadingFailed()
	{
		Debug.LogError((object)"WatchLoader::LoadingFailed:: Failed to load watch assets! Unloading and destroying any loaded assets.");
		UnloadAndDestroyAssets();
	}

	private void UnloadAndDestroyAssets()
	{
		if (_loadSequence != null)
		{
			((MonoBehaviour)this).StopCoroutine(_loadSequence);
			_loadSequence = null;
		}
		BeamTeam.Destroy<GameObject>(_2dView);
		BeamTeam.Destroy<GameObject>(_3dView);
		if (_assetIsLoaded)
		{
			Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(BUNDLE_NAME);
			Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(BUNDLE_NAME);
			_assetIsLoaded = false;
		}
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
	}
}
