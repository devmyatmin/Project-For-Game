using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Raycaster : MonoBehaviour, IGameInputHandler, INeedPlayerComponent
{
	public const float RAYCASTING_DISTANCE = 3f;

	[SerializeField]
	private LayerMask _raycastingMask;

	private IBase _currentObject;

	private BeamRay _ray;

	private RaycastHit _lastHitInfo;

	private RaycastHit[] _hitBuffer = new RaycastHit[2];

	private IRaycastFilter _filter = NullRaycastFilter.Instance;

	private IPlayer _player;

	private IBase _left;

	private bool _suspended;

	public LayerMask RaycastingMask
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _raycastingMask;
		}
	}

	public IBase CurrentObject
	{
		get
		{
			if (!BeamTeam.IsNullOrDestroyed(_currentObject) && !_currentObject.gameObject.activeSelf)
			{
				DeselectCurrentObject();
			}
			return _currentObject;
		}
	}

	public BeamRay Ray => _ray;

	public RaycastHit LastHitInfo
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _lastHitInfo;
		}
	}

	IPlayer INeedPlayerComponent.Player => _player;

	public event RaycasterEventHandler EnterObject;

	public event RaycasterEventHandler LeaveObject;

	public void SetCurrentObject(IBase currentObject)
	{
		_currentObject = currentObject;
	}

	protected virtual void OnEnterObject(IBase obj)
	{
		EnterObject?.Invoke(obj);
	}

	protected virtual void OnLeaveObject(IBase obj)
	{
		if (_left != obj)
		{
			LeaveObject?.Invoke(obj);
			_left = obj;
		}
	}

	public void Initialize(IGameInputController controller)
	{
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Statistics.KnockedOut += PlayerStatistics_KnockedOut;
	}

	public void SetFilter(IRaycastFilter filter)
	{
		_filter = filter ?? NullRaycastFilter.Instance;
	}

	private void OnDestroy()
	{
		EnterObject = null;
		LeaveObject = null;
		if (!BeamTeam.IsNullOrDestroyed(_player) && Object.op_Implicit((Object)(object)_player.Statistics))
		{
			_player.Statistics.KnockedOut -= PlayerStatistics_KnockedOut;
		}
	}

	private void PlayerStatistics_KnockedOut(bool knockedOut)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			Suspend(knockedOut);
		}
	}

	public void Suspend(bool suspend)
	{
		_suspended = suspend;
		DeselectCurrentObject();
	}

	public void CheckInput()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (_suspended)
		{
			return;
		}
		_ray = _player.PlayerCamera.GetRay(3f);
		_hitBuffer[0] = default;
		_hitBuffer[1] = default;
		int num = Physics.SphereCastNonAlloc(_ray.OriginPoint, 0.15f, _ray.Direction, _hitBuffer, _ray.Length, LayerMask.op_Implicit(_raycastingMask));
		RaycastHit hit;
		if (num == 0)
		{
			DeselectCurrentObject();
		}
		else if (GetPrecisionHit(out hit) || GetHits(_hitBuffer, num, out hit, _ray.Length))
		{
			IBase obj = BeamTeam.GetInterface<IBase>((Component)(object)hit.collider);
			IBase obj2 = obj;
			if ((BeamTeam.IsNullOrDestroyed(obj2) || obj2.RaycastMode == RaycastMode.Rigidbody) && (Object)(object)hit.rigidbody != (Object)null)
			{
				obj2 = BeamTeam.GetInterface<IBase>((Component)(object)hit.rigidbody);
				if (BeamTeam.IsNullOrDestroyed(obj2))
				{
					RagdollChild component = ((Component)hit.rigidbody).GetComponent<RagdollChild>();
					if ((Object)(object)component != (Object)null)
					{
						obj2 = component.InteractiveObject;
					}
				}
				if (BeamTeam.IsValid(obj2) && BeamTeam.IsValid(obj))
				{
					obj2 = obj;
				}
			}
			if (BeamTeam.IsNullOrDestroyed(obj2))
			{
				DeselectCurrentObject();
				return;
			}
			if (BeamTeam.IsNullOrDestroyed(_currentObject) || ((Object)_currentObject.gameObject).GetInstanceID() != ((Object)obj2.gameObject).GetInstanceID())
			{
				OnLeaveObject(_currentObject);
				_currentObject = obj2;
				OnEnterObject(_currentObject);
			}
			_lastHitInfo = hit;
		}
		else if (!BeamTeam.IsNullOrDestroyed(_currentObject))
		{
			DeselectCurrentObject();
		}
	}

	private void DeselectCurrentObject()
	{
		OnLeaveObject(_currentObject);
		_currentObject = null;
	}

	private bool GetPrecisionHit(out RaycastHit hit)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.Raycast(_ray.OriginPoint, _ray.Direction, ref hit, _ray.Length, LayerMask.op_Implicit(_raycastingMask)))
		{
			return _filter.Validate(hit);
		}
		return false;
	}

	private bool GetHits(RaycastHit[] hitBuffer, int hitCount, out RaycastHit hit, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		hit = default;
		bool result = false;
		float num = distance;
		for (int i = 0; i < hitCount; i++)
		{
			RaycastHit val = hitBuffer[i];
			if (_filter.Validate(val) && val.distance < num)
			{
				hit = val;
				num = val.distance;
				result = true;
			}
		}
		return result;
	}

	public void Reset()
	{
		LeaveObject?.Invoke(_currentObject);
		_currentObject = null;
	}
}
