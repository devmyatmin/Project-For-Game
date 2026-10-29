using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam.Intro;

public class AircraftProximity : MonoBehaviour
{
	[SerializeField]
	private bool _debug;

	private const float PROXIMITY_HEIGHT = 8f;

	private const float PROXIMITY_DISTANCE = 10f;

	[FormerlySerializedAs("_position")]
	[SerializeField]
	private Transform _proximityPosition;

	private bool _inProximity;

	[Header("EVENTS")]
	public UnityEvent EnteredProximity;

	public UnityEvent LeftProximity;

	private void Start()
	{
		InvokeRepeating("Update_InvokeRepeating", 0f, 1f);
	}

	private void Update_InvokeRepeating()
	{
		if (LevelLoader.IsLoading() || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		IPlayer player = PlayerUtilities.FindClosestPlayer(_proximityPosition.position);
		if (!player.IsNullOrDestroyed())
		{
			bool flag = CheckBounds(player.transform.position);
			if (flag && !_inProximity)
			{
				EnterProximity();
			}
			else if (!flag && _inProximity)
			{
				LeaveProximity();
			}
		}
	}

	private void EnterProximity()
	{
		EnteredProximity?.Invoke();
		_inProximity = true;
	}

	private void LeaveProximity()
	{
		LeftProximity?.Invoke();
		_inProximity = false;
	}

	private bool CheckBounds(Vector3 position)
	{
		if (Vector3Tools.SqrDistanceOnAxis(_proximityPosition.position, position, Vector3Tools.xz) < BeamMath.Sqr(10f) && position.y > _proximityPosition.position.y)
		{
			return position.y < _proximityPosition.position.y + 8f;
		}
		return false;
	}
}
