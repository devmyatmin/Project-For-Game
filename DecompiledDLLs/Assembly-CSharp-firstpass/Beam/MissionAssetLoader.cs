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
			MissionAssetLoader component = ((Component)MissionLoader).GetComponent<MissionAssetLoader>();
			MissionGoalBase component2 = ((Component)Goal).GetComponent<MissionGoalBase>();
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
		Loaded?.Invoke();
	}

	protected virtual void OnLoadedGoal(IMissionGoal goal)
	{
		LoadedGoal?.Invoke(goal);
	}

	protected virtual void OnLoadFailed()
	{
		LoadFailed?.Invoke();
	}

	public void Load()
	{
		_loadSequence = ((MonoBehaviour)this).StartCoroutine(Load_Sequence());
	}

	private IEnumerator Load_Sequence()
	{
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>("ENVIRONMENT", _missionAssetBundleName, (Action<GameObject>)CreateEnvironment, (Action)LoadingFailed);
		yield return Singleton<AssetBundleManager>.Instance.LoadAssetAsync<GameObject>(GOAL_ASSET_NAME, _missionAssetBundleName, (Action<GameObject>)CreateGoal, (Action)LoadingFailed);
		OnLoaded();
	}

	private void LoadingFailed()
	{
		Debug.LogError((object)("MissionAssetLoader::LoadingFailed:: [" + ((Object)((Component)this).gameObject).name + "] failed to load! Unloading and destroying any loaded assets."));
		UnloadAndDestroyAssets();
		OnLoadFailed();
	}

	private void CreateEnvironment(GameObject prefab)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)prefab == (Object)null)
		{
			return;
		}
		if (_bossType == MissionType.GiantSquid)
		{
			if (MPExtensions.IsMaster(Game.Mode))
			{
				MiniGuid id = MiniGuid.New();
				_environment = ((Component)MultiplayerMng.Instantiate<BuoyMultiplayerSync>(BoltPrefabs.ENVIRONMENT, (IProtocolToken)(object)new MultiplayerTokens.SaveablePrefab(id))).gameObject;
				_environment.transform.SetParent(((Component)this).transform, false);
				_environment.transform.position = _environmentSpawnPosition.position;
			}
		}
		else
		{
			_environment = Object.Instantiate<GameObject>(prefab);
			_environment.transform.SetParent(((Component)this).transform, false);
			_environment.transform.position = _environmentSpawnPosition.position;
		}
	}

	private void CreateGoal(GameObject prefab)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(CreateGoal_Sequence(prefab));
	}

	private IEnumerator<float> CreateGoal_Sequence(GameObject prefab)
	{
		yield return Timing.WaitUntilDone(MultiplayerFence.Sync(_bossType.ToFence()));
		if (!((Object)(object)prefab == (Object)null) && !((Object)(object)this == (Object)null) && MPExtensions.IsMaster(Game.Mode))
		{
			MiniGuid id = MiniGuid.New();
			MissionGoalBase component = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(_bossType switch
			{
				MissionType.Eel => BoltPrefabs.GOAL_EEL, 
				MissionType.Shark => BoltPrefabs.GOAL_SHARK, 
				MissionType.GiantSquid => BoltPrefabs.GOAL_SQUID, 
				_ => BoltPrefabs.GOAL_EEL, 
			}, (IProtocolToken)(object)new MultiplayerTokens.SaveablePrefab(id))).GetComponent<MissionGoalBase>();
			if (!((Object)(object)component == (Object)null))
			{
				ReplicatedPrepareGoal(component);
			}
		}
	}

	private void PrepareGoal(MissionGoalBase goal)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		_goal = ((Component)goal).gameObject;
		_goal.transform.SetParent(((Component)this).transform, false);
		_goal.transform.position = _goalSpawnPosition.position;
		_goal.transform.rotation = _goalSpawnPosition.rotation;
		OnLoadedGoal(goal);
	}

	public void UnloadAndDestroyAssets()
	{
		if (_loadSequence != null)
		{
			((MonoBehaviour)this).StopCoroutine(_loadSequence);
			_loadSequence = null;
		}
		if (!BeamTeam.IsNullOrDestroyed(_environment) && _bossType == MissionType.GiantSquid)
		{
			MultiplayerMng.DestroyMultiplayerEntity(_environment);
		}
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(_missionAssetBundleName);
		Singleton<AssetBundleManager>.Instance?.UnloadAssetBundle(_missionAssetBundleName);
	}

	private void OnDestroy()
	{
		UnloadAndDestroyAssets();
		Loaded = null;
		LoadedGoal = null;
		LoadFailed = null;
	}

	public void ReplicatedPrepareGoal(MissionGoalBase goal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicatePrepareGoal replicatePrepareGoal = new ReplicatePrepareGoal();
			replicatePrepareGoal.MissionLoader = ((Component)this).GetComponent<SaveablePrefab>();
			replicatePrepareGoal.Goal = ((Component)goal).GetComponent<SaveablePrefab>();
			replicatePrepareGoal.Post();
		}
		PrepareGoal(goal);
	}
}
