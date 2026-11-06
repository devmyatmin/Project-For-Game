using System;
using System.Collections;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Intro;

public class AircraftAssetLoader : SaveablePrefab
{
	[SerializeField]
	private string _assetBundleName;

	[SerializeField]
	private string _assetName;

	private Coroutine _loadSequence;

	private AircraftManager _aircraftManager;

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		LoadAssets(null);
	}

	public Coroutine LoadAssets(JObject data)
	{
		_loadSequence = ((MonoBehaviour)this).StartCoroutine(LoadAssets_Sequence(data));
		return _loadSequence;
	}

	private void LoadingAssetsFailed()
	{
		Debug.LogError((object)("EndGameAssetLoader::LoadingAssetsFailed:: [" + ((Object)((Component)this).gameObject).name + "] failed to load! Unloading and destroying any loaded assets."));
		UnloadAndDestroyAssets();
	}

	private void CreateAsset(GameObject prefab)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			GameObject val = Object.Instantiate<GameObject>(prefab);
			_aircraftManager = val.GetComponent<AircraftManager>();
			((Component)_aircraftManager).transform.SetParent(((Component)this).transform, false);
		}
	}

	public void UnloadAndDestroyAssets()
	{
		BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, _loadSequence);
		_loadSequence = null;
		if (!BeamTeam.IsNullOrDestroyed(_aircraftManager))
		{
			BeamTeam.Destroy<GameObject>(((Component)_aircraftManager).gameObject);
		}
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(_assetBundleName);
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Transform", JSerializer.Serialize((Component)(object)((Component)this).transform));
		JObject data = SavingUtilities.Save(_aircraftManager);
		jObject.AddField("EndGameManager", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		((Component)this).transform.localPosition = Vector3.zero;
		CompleteTransformSerialization();
		JObject field = data.GetField("EndGameManager");
		if (field != null && !field.IsNull())
		{
			LoadAssets(field);
		}
	}

	private IEnumerator LoadAssets_Sequence(JObject data = null)
	{
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>(_assetName, _assetBundleName, (Action<GameObject>)CreateAsset, (Action)LoadingAssetsFailed);
		if (data != null)
		{
			yield return 0f;
			yield return 0f;
			SimpleLoader simpleLoader = new SimpleLoader();
			simpleLoader.IsLoading = true;
			LevelLoader.CurrentLoader = simpleLoader;
			_aircraftManager.Load(data);
			for (int i = 0; i < simpleLoader.DeferredLoading.Count; i++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(simpleLoader.DeferredLoading[i]);
			}
			for (int j = 0; j < simpleLoader.PostLoading.Count; j++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(simpleLoader.PostLoading[j]);
			}
			for (int k = 0; k < simpleLoader.PhysicalLoading.Count; k++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(simpleLoader.PhysicalLoading[k]);
			}
			simpleLoader.IsLoading = false;
			LevelLoader.CurrentLoader = null;
		}
	}
}
