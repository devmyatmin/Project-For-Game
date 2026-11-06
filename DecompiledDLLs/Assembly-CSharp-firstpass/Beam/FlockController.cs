using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class FlockController : MonoBehaviour
{
	private class ReplicatedRepositionOriginMsg : MultiplayerMessage
	{
		[Replicate]
		public Vector3 Position;

		public override void OnPeer()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Object.FindObjectOfType<FlockController>().RepositionOrigin(Position);
		}
	}

	private static FlockController _Instance;

	private LandingSpotController _landingSpotController;

	[SerializeField]
	private Transform _activeContainer;

	[SerializeField]
	private Transform _deactivatedContainer;

	[SerializeField]
	private LayerMask _avoidanceMask;

	private Vector3 _origin;

	[Header("Flock Bounds")]
	[SerializeField]
	private Vector3 _center;

	[SerializeField]
	private Vector2 _size;

	[SerializeField]
	private Vector2 _spawnBuffer;

	[Header("Waypoints")]
	[SerializeField]
	private float _waypointDistance = 10f;

	[Tooltip("Bias for new waypoint direction between flock's forward direction and random direction.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _waypointForwardBias = 0.3f;

	[Tooltip("Bias for new waypoint height between flock's height and random height.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float _waypointHeightBias = 0.5f;

	[Header("Flocks")]
	[SerializeField]
	private FlockProvider _flockProvider;

	public FlockAudio _flockAudioPrefab;

	private IList<FlockData> _flockData;

	private Dictionary<FlockSpecies, FlockData> _availableFlocks = new Dictionary<FlockSpecies, FlockData>();

	private string _currentZoneName;

	private string _nmlZoneName = "";

	private string _endGameZoneName = "";

	public static FlockController Instance => _Instance;

	public Transform ActiveContainer => _activeContainer;

	public Transform DeactivatedContainer => _deactivatedContainer;

	public LayerMask AvoidanceMask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _avoidanceMask;
		}
	}

	public IDictionary<FlockSpecies, FlockData> AvailableFlocks => _availableFlocks;

	public Vector3 ControllerPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return _origin + _center;
		}
	}

	public float MaxLandingSpotDistance => 30f;

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			_Instance = this;
		}
	}

	private IEnumerator Start()
	{
		_landingSpotController = Singleton<LandingSpotController>.Instance;
		if (!MPExtensions.IsMaster(Game.Mode))
		{
			yield break;
		}
		while (LevelLoader.IsLoadingScene())
		{
			yield return null;
		}
		_flockData = _flockProvider.GetFlocks();
		CreateFlocks();
		EventManager.AddListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
		if (Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance))
		{
			_nmlZoneName = ((Object)Singleton<StrandedWorld>.Instance.NmlZone).name;
			Zone zone = Singleton<StrandedWorld>.Instance.FindEndGameZone();
			if (Object.op_Implicit((Object)(object)zone))
			{
				_endGameZoneName = ((Object)zone).name;
			}
		}
		((MonoBehaviour)this).InvokeRepeating("PollZone", 1f, 1f);
		((MonoBehaviour)this).InvokeRepeating("PollFlockEvents", 1f, 1f);
	}

	private void OnDestroy()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_Instance != (Object)(object)this))
		{
			_Instance = null;
			if (MPExtensions.IsMaster(Game.Mode))
			{
				EventManager.RemoveListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
			}
		}
	}

	private void EventManager_EndGameSequenceEvent(EndGameSequenceEvent e)
	{
		StopAllFlockAudio();
	}

	private void PollZone()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		IPlayer player = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(PlayerRegistry.AllPlayers);
		if (BeamTeam.IsNullOrDestroyed(player))
		{
			return;
		}
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		if (Object.op_Implicit((Object)(object)zone) && !(((Object)zone).name == _currentZoneName))
		{
			_currentZoneName = ((Object)zone).name;
			DeactivateFlocks();
			if (!(_currentZoneName == _nmlZoneName) && !(_currentZoneName == _endGameZoneName))
			{
				ReplicatedRepositionOrigin(((Component)zone).transform.position);
				RefreshAvailableFlocks();
			}
		}
	}

	private void RepositionOrigin(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 origin = position;
		origin.y = 0f;
		_origin = origin;
	}

	private void CreateFlocks()
	{
		foreach (FlockData flockDatum in _flockData)
		{
			_availableFlocks.Add(flockDatum.Species, flockDatum);
			FlockAudio audio = CreateFlockAudio(flockDatum);
			_availableFlocks[flockDatum.Species].Audio = audio;
			for (int i = 0; i < flockDatum.Population; i++)
			{
				CreateFlockChild(flockDatum);
			}
		}
	}

	public void StopAllFlockAudio()
	{
		foreach (FlockData flockDatum in _flockData)
		{
			_availableFlocks[flockDatum.Species].Audio.Stop();
		}
	}

	private FlockChild CreateFlockChild(FlockData flockData)
	{
		FlockChild component = ((Component)MultiplayerMng.Instantiate<SaveablePrefab>(((Component)flockData.Prefab).GetComponent<SaveablePrefab>().PrefabId, MiniGuid.New())).GetComponent<FlockChild>();
		((Component)component).transform.SetParent(_deactivatedContainer, false);
		component.Initialize(this, _landingSpotController);
		flockData.Children.Add(component);
		return component;
	}

	private FlockAudio CreateFlockAudio(FlockData flockData)
	{
		FlockAudio flockAudio = Object.Instantiate<FlockAudio>(_flockAudioPrefab);
		((Component)flockAudio).transform.SetParent(_deactivatedContainer, false);
		flockAudio.AmbientClip = flockData.FlockAudioClip;
		return flockAudio;
	}

	private void InitializeFlockChild(FlockChild flockChild, Vector3 startingPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		((Component)flockChild).transform.position = startingPosition + Random.onUnitSphere * flockChild.LocalWaypointSpreadRadius;
		flockChild.Reset();
		flockChild.SetLocalWaypoint();
		flockChild.ReplicatedChangeState(FlockChildState.Flying, Vector3.zero);
	}

	private void PollFlockEvents()
	{
		if (_currentZoneName == _nmlZoneName || _currentZoneName == _endGameZoneName)
		{
			return;
		}
		foreach (KeyValuePair<FlockSpecies, FlockData> availableFlock in _availableFlocks)
		{
			FlockData value = availableFlock.Value;
			if ((!value.Active || value.Migrating) && CheckTime(value.StartTime, value.EndTime))
			{
				if (Random.value * 100f <= (float)value.Percentage)
				{
					StartFlockEvent(value);
				}
			}
			else if (value.Active && !CheckTime(value.StartTime, value.EndTime))
			{
				StopFlockEvent(value);
			}
			if (value.Migrating && Beam.Utilities.ExtensionMethods.All_NonAlloc(value.Children, (FlockChild x) => (FlockChildState)x.CurrentState == FlockChildState.Migrated))
			{
				DeactivateFlock(value);
			}
		}
	}

	private void StartFlockEvent(FlockData flockData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 startingPosition = GetStartingPosition();
		flockData.Migrating = false;
		flockData.Waypoint = GetRandomPositionInBounds();
		SetNewWaypointForSpecies(flockData.Species, Random.onUnitSphere);
		((Component)flockData.Audio).transform.position = startingPosition;
		ActivateFlock(flockData);
		foreach (FlockChild child in flockData.Children)
		{
			InitializeFlockChild(child, startingPosition);
		}
	}

	private void StopFlockEvent(FlockData flockData)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (flockData.Migrating)
		{
			return;
		}
		Vector3 startingPosition = GetStartingPosition();
		flockData.Migrating = true;
		flockData.Waypoint = startingPosition;
		flockData.Audio.TargetPosition = startingPosition;
		foreach (FlockChild child in flockData.Children)
		{
			child.SpeciesWaypoint = flockData.Waypoint;
			child.SetLocalWaypoint();
			child.Migrate();
		}
	}

	private void ActivateFlock(FlockData flockData)
	{
		foreach (FlockChild child in flockData.Children)
		{
			((Component)child).transform.SetParent(_activeContainer);
		}
		((Component)flockData.Audio).transform.SetParent(_activeContainer);
		flockData.Active = true;
	}

	private void DeactivateFlock(FlockData flockData)
	{
		if (!flockData.Active)
		{
			return;
		}
		foreach (FlockChild child in flockData.Children)
		{
			DeactivateFlockChild(child);
		}
		((Component)flockData.Audio).transform.SetParent(_deactivatedContainer);
		flockData.Active = false;
	}

	private void DeactivateFlocks()
	{
		foreach (KeyValuePair<FlockSpecies, FlockData> availableFlock in _availableFlocks)
		{
			FlockData value = availableFlock.Value;
			DeactivateFlock(value);
		}
	}

	public void DeactivateFlockChild(FlockChild flockChild)
	{
		((Component)flockChild).transform.SetParent(_deactivatedContainer);
	}

	private void RefreshAvailableFlocks()
	{
		_flockData = _flockProvider.GetFlocks();
	}

	public void SetNewWaypointForSpecies(FlockSpecies species, Vector3 forward)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3 newWaypoint = GetNewWaypoint(_availableFlocks[species].Waypoint, forward);
		foreach (FlockChild child in _availableFlocks[species].Children)
		{
			child.SpeciesWaypoint = newWaypoint;
		}
		_availableFlocks[species].Waypoint = newWaypoint;
		_availableFlocks[species].Audio.TargetPosition = newWaypoint;
	}

	private Vector3 GetNewWaypoint(Vector3 previousWaypoint, Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Lerp(forward, Random.onUnitSphere, _waypointForwardBias) * _waypointDistance;
		if (Vector3Tools.DistanceOnAxis(ControllerPosition, previousWaypoint + val, Vector3Tools.xz) > _size.x)
		{
			val.x *= -1f;
			val.z *= -1f;
		}
		Vector3 val2 = previousWaypoint + val;
		val2.y = Mathf.Lerp(previousWaypoint.y, val2.y, _waypointHeightBias);
		val2.y = Mathf.Clamp(val2.y, ControllerPosition.y, ControllerPosition.y + _size.y);
		return val2;
	}

	private Vector3 GetStartingPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ControllerPosition - PlayerRegistry.AllPlayers[0].transform.position;
		val.y = 0f;
		val.Normalize();
		return ControllerPosition + val * (_size.x + _spawnBuffer.x);
	}

	public void RemoveChildFromFlock(FlockChild flockChild)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_availableFlocks.TryGetValue(flockChild.Species, out var value))
		{
			value.Children.Remove(flockChild);
			FlockChild flockChild2 = CreateFlockChild(value);
			if (value.Active)
			{
				((Component)flockChild2).transform.SetParent(_activeContainer);
			}
			flockChild2.SpeciesWaypoint = value.Waypoint;
			InitializeFlockChild(flockChild2, GetStartingPosition());
			if (value.Migrating)
			{
				flockChild2.Migrate();
			}
		}
	}

	private Vector3 GetRandomPositionInBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = ControllerPosition + Random.insideUnitSphere * _size.x;
		result.y = ControllerPosition.y + Random.Range(0f, _size.y);
		return result;
	}

	private Vector3 GetRandomPositionOnBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = ControllerPosition + Random.onUnitSphere * _size.x;
		result.y = ControllerPosition.y + Random.Range(0f, _size.y);
		return result;
	}

	private void ReplicatedRepositionOrigin(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicatedRepositionOriginMsg replicatedRepositionOriginMsg = new ReplicatedRepositionOriginMsg();
			replicatedRepositionOriginMsg.Position = position;
			replicatedRepositionOriginMsg.Post();
		}
		RepositionOrigin(position);
	}

	private bool CheckTime(float startTime, float endTime)
	{
		return IsTimeBetween(GameTime.Now.TimeOfDay, TimeSpan.FromHours(startTime), TimeSpan.FromHours(endTime));
	}

	private bool IsTimeBetween(TimeSpan time, TimeSpan startTime, TimeSpan endTime)
	{
		if (endTime == startTime)
		{
			return true;
		}
		if (endTime < startTime)
		{
			if (!(time <= endTime))
			{
				return time >= startTime;
			}
			return true;
		}
		if (time >= startTime)
		{
			return time <= endTime;
		}
		return false;
	}
}
