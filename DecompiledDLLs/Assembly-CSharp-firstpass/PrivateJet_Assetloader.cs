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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_loadSequence = Timing.RunCoroutine(Load_Sequence());
		return Timing.WaitUntilDone(_loadSequence);
	}

	private IEnumerator<float> Load_Sequence()
	{
		IEnumerator e = Singleton<AssetBundleManager>.Instance.LoadAsset_Sequence<GameObject>("Intro_Assets", BUNDLE_NAME, (Action<GameObject>)CreateObjects, (Action)LoadingFailed);
		while (e.MoveNext())
		{
			yield return 0f;
		}
		((IDisposable)e).Dispose();
		yield return Timing.WaitForSeconds(2f);
	}

	private void CreateObjects(GameObject prefab)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			_introAssets = Object.Instantiate<GameObject>(prefab);
			_introAssets.transform.parent = ((Component)this).transform.parent;
			Jet = _introAssets.GetComponent<PrivateJet>();
			if ((Object)(object)_props != (Object)null)
			{
				_props.SetActive(true);
			}
		}
	}

	private void LoadingFailed()
	{
		Debug.LogError((object)"WatchLoader::LoadingFailed:: Failed to load intro assets! Unloading and destroying any loaded assets.");
		UnloadAndDestroyAssets();
	}

	private void UnloadAndDestroyAssets()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (_loadSequence.IsRunning)
		{
			Timing.KillCoroutines(_loadSequence);
		}
		BeamTeam.Destroy<GameObject>(_introAssets);
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(BUNDLE_NAME);
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
	}
}
