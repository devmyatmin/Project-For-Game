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
		((MonoBehaviour)this).InvokeRepeating("Update_InvokeRepeating", 0f, 1f);
	}

	private void Update_InvokeRepeating()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (LevelLoader.IsLoading() || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		IPlayer player = PlayerUtilities.FindClosestPlayer(_proximityPosition.position);
		if (!BeamTeam.IsNullOrDestroyed(player))
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
		UnityEvent enteredProximity = EnteredProximity;
		if (enteredProximity != null)
		{
			enteredProximity.Invoke();
		}
		_inProximity = true;
	}

	private void LeaveProximity()
	{
		UnityEvent leftProximity = LeftProximity;
		if (leftProximity != null)
		{
			leftProximity.Invoke();
		}
		_inProximity = false;
	}

	private bool CheckBounds(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Tools.SqrDistanceOnAxis(_proximityPosition.position, position, Vector3Tools.xz) < BeamMath.Sqr(10f) && position.y > _proximityPosition.position.y)
		{
			return position.y < _proximityPosition.position.y + 8f;
		}
		return false;
	}
}
