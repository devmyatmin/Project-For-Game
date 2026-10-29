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
		base.transform.parent = null;
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.Euler(Vector3.zero);
		_watchPosition = _watchController?.Player?.Character?.WatchPosition;
		if (_watchPosition == null)
		{
			Debug.LogError("WatchLoader:: No watch position found!");
			yield break;
		}
		_loadSequence = StartCoroutine(Load_Sequence());
		yield return _loadSequence;
		if (_2dView == null || _3dView == null)
		{
			LoadingFailed();
			yield break;
		}
		WatchViewAdapterBase componentInChildren = _2dView.GetComponentInChildren<WatchViewAdapterBase>();
		WatchScreenViewAdapterBase componentInChildren2 = _3dView.GetComponentInChildren<WatchScreenViewAdapterBase>();
		if (componentInChildren == null || componentInChildren2 == null)
		{
			Debug.LogError("WatchLoader:: Failed to find watch components in loaded assets! Unloading and destroying any loaded assets.");
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
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("UI - Watch", BUNDLE_NAME, Create2dView, LoadingFailed);
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("View - 3D Watch", BUNDLE_NAME, Create3dView, LoadingFailed);
	}

	private void Create2dView(GameObject prefab)
	{
		if (!(prefab == null))
		{
			_2dView = Object.Instantiate(prefab);
		}
	}

	private void Create3dView(GameObject prefab)
	{
		if (!(prefab == null))
		{
			_3dView = Object.Instantiate(prefab);
			_3dView.transform.SetParent(_watchPosition.transform, worldPositionStays: false);
			_3dView.transform.localPosition = Vector3.zero;
		}
	}

	private void LoadingFailed()
	{
		Debug.LogError("WatchLoader::LoadingFailed:: Failed to load watch assets! Unloading and destroying any loaded assets.");
		UnloadAndDestroyAssets();
	}

	private void UnloadAndDestroyAssets()
	{
		if (_loadSequence != null)
		{
			StopCoroutine(_loadSequence);
			_loadSequence = null;
		}
		BeamTeam.Destroy(_2dView);
		BeamTeam.Destroy(_3dView);
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
