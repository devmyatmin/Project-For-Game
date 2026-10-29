using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using MEC;
using Photon.Bolt;
using UnityEngine;

namespace Beam;

public class MissionAssetLoader : MonoBehaviour
{
	public enum MissionType
	{
		Shark,
		Eel,
		GiantSquid,
		Ship
	}

	private class ReplicatePrepareGoal : I_MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionLoader;

		[I_Replicate]
		public SaveablePrefab Goal;

		public override void OnPeer()
		{
			MissionAssetLoader component = MissionLoader.GetComponent<MissionAssetLoader>();
			MissionGoalBase component2 = Goal.GetComponent<MissionGoalBase>();
			component.PrepareGoal(component2);
		}
	}

	private const string ENVIRONMENT_ASSET_NAME = "ENVIRONMENT";

	[SerializeField]
	private string GOAL_ASSET_NAME = "GOAL";

	[SerializeField]
	private string _missionAssetBundleName;

	[SerializeField]
	private Transform _environmentSpawnPosition;

	[SerializeField]
	private Transform _goalSpawnPosition;

	private GameObject _environment;

	private GameObject _goal;

	private Coroutine _loadSequence;

	[SerializeField]
	private MissionType _bossType;

	public string MissionAssetBundleName => _missionAssetBundleName;

	public event Action Loaded;

	public event Action<IMissionGoal> LoadedGoal;

	public event Action LoadFailed;

	protected virtual void OnLoaded()
	{
		this.Loaded?.Invoke();
	}

	protected virtual void OnLoadedGoal(IMissionGoal goal)
	{
		this.LoadedGoal?.Invoke(goal);
	}

	protected virtual void OnLoadFailed()
	{
		this.LoadFailed?.Invoke();
	}

	public void Load()
	{
		_loadSequence = StartCoroutine(Load_Sequence());
	}

	private IEnumerator Load_Sequence()
	{
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("ENVIRONMENT", _missionAssetBundleName, CreateEnvironment, LoadingFailed);
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>(GOAL_ASSET_NAME, _missionAssetBundleName, CreateGoal, LoadingFailed);
		OnLoaded();
	}

	private void LoadingFailed()
	{
		Debug.LogError("MissionAssetLoader::LoadingFailed:: [" + base.gameObject.name + "] failed to load! Unloading and destroying any loaded assets.");
		UnloadAndDestroyAssets();
		OnLoadFailed();
	}

	private void CreateEnvironment(GameObject prefab)
	{
		if (prefab == null)
		{
			return;
		}
		if (_bossType == MissionType.GiantSquid)
		{
			if (Game.Mode.IsMaster())
			{
				MiniGuid id = MiniGuid.New();
				_environment = MultiplayerMng.Instantiate<BuoyMultiplayerSync>(BoltPrefabs.ENVIRONMENT, new MultiplayerTokens.SaveablePrefab(id)).gameObject;
				_environment.transform.SetParent(base.transform, worldPositionStays: false);
				_environment.transform.position = _environmentSpawnPosition.position;
			}
		}
		else
		{
			_environment = UnityEngine.Object.Instantiate(prefab);
			_environment.transform.SetParent(base.transform, worldPositionStays: false);
			_environment.transform.position = _environmentSpawnPosition.position;
		}
	}

	private void CreateGoal(GameObject prefab)
	{
		Timing.RunCoroutine(CreateGoal_Sequence(prefab));
	}

	private IEnumerator<float> CreateGoal_Sequence(GameObject prefab)
	{
		yield return Timing.WaitUntilDone(MultiplayerFence.Sync(_bossType.ToFence()));
		if (!(prefab == null) && !(this == null) && Game.Mode.IsMaster())
		{
			MiniGuid id = MiniGuid.New();
			MissionGoalBase component = MultiplayerMng.Instantiate<SaveablePrefab>(_bossType switch
			{
				MissionType.Eel => BoltPrefabs.GOAL_EEL, 
				MissionType.Shark => BoltPrefabs.GOAL_SHARK, 
				MissionType.GiantSquid => BoltPrefabs.GOAL_SQUID, 
				_ => BoltPrefabs.GOAL_EEL, 
			}, new MultiplayerTokens.SaveablePrefab(id)).GetComponent<MissionGoalBase>();
			if (!(component == null))
			{
				ReplicatedPrepareGoal(component);
			}
		}
	}

	private void PrepareGoal(MissionGoalBase goal)
	{
		_goal = goal.gameObject;
		_goal.transform.SetParent(base.transform, worldPositionStays: false);
		_goal.transform.position = _goalSpawnPosition.position;
		_goal.transform.rotation = _goalSpawnPosition.rotation;
		OnLoadedGoal(goal);
	}

	public void UnloadAndDestroyAssets()
	{
		if (_loadSequence != null)
		{
			StopCoroutine(_loadSequence);
			_loadSequence = null;
		}
		if (!_environment.IsNullOrDestroyed() && _bossType == MissionType.GiantSquid)
		{
			MultiplayerMng.DestroyMultiplayerEntity(_environment);
		}
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(_missionAssetBundleName);
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(_missionAssetBundleName);
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
		this.Loaded = null;
		this.LoadedGoal = null;
		this.LoadFailed = null;
	}

	public void ReplicatedPrepareGoal(MissionGoalBase goal)
	{
		if (Game.Mode.IsServer())
		{
			ReplicatePrepareGoal replicatePrepareGoal = new ReplicatePrepareGoal();
			replicatePrepareGoal.MissionLoader = GetComponent<SaveablePrefab>();
			replicatePrepareGoal.Goal = goal.GetComponent<SaveablePrefab>();
			replicatePrepareGoal.Post();
		}
		PrepareGoal(goal);
	}
}
