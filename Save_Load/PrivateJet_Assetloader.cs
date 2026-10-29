using System;
using System.Collections;
using System.Collections.Generic;
using Beam;
using Beam.Intro;
using Beam.Utilities;
using MEC;
using UnityEngine;

public class PrivateJet_Assetloader : MonoBehaviour
{
	[SerializeField]
	private GameObject _props;

	private CoroutineHandle _loadSequence;

	private GameObject _introAssets;

	[HideInInspector]
	public PrivateJet Jet;

	private string BUNDLE_NAME = "scenes/introscene.unity3d";

	public GameObject IntroAssets => _introAssets;

	public float Load_Async()
	{
		_loadSequence = Timing.RunCoroutine(Load_Sequence());
		return Timing.WaitUntilDone(_loadSequence);
	}

	private IEnumerator<float> Load_Sequence()
	{
		IEnumerator e = Singleton<AssetBundleManager>.Instance.LoadAsset_Sequence<GameObject>("Intro_Assets", BUNDLE_NAME, CreateObjects, LoadingFailed);
		while (e.MoveNext())
		{
			yield return 0f;
		}
		((IDisposable)e).Dispose();
		yield return Timing.WaitForSeconds(2f);
	}

	private void CreateObjects(GameObject prefab)
	{
		if (!(prefab == null))
		{
			_introAssets = UnityEngine.Object.Instantiate(prefab);
			_introAssets.transform.parent = base.transform.parent;
			Jet = _introAssets.GetComponent<PrivateJet>();
			if (_props != null)
			{
				_props.SetActive(value: true);
			}
		}
	}

	private void LoadingFailed()
	{
		Debug.LogError("WatchLoader::LoadingFailed:: Failed to load intro assets! Unloading and destroying any loaded assets.");
		UnloadAndDestroyAssets();
	}

	private void UnloadAndDestroyAssets()
	{
		if (_loadSequence.IsRunning)
		{
			Timing.KillCoroutines(_loadSequence);
		}
		BeamTeam.Destroy(_introAssets);
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(BUNDLE_NAME);
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
	}
}
