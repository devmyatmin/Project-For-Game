using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class MovingPlatform : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private BaseObject _baseObject;

	private RigidbodyInterpolation _interpolation;

	public IDraggable Platform { get; private set; }

	public bool Calibrated { get; private set; }

	private void Awake()
	{
		if ((Object)(object)_baseObject == (Object)null)
		{
			_baseObject = ((Component)this).GetComponent<BaseObject>();
		}
		Platform = _baseObject as InteractiveObject;
	}

	public void OnEnter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(Platform))
		{
			_interpolation = Platform.rigidbody.interpolation;
			Platform.rigidbody.interpolation = (RigidbodyInterpolation)2;
			Calibrated = true;
		}
	}

	public void OnExit()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(Platform))
		{
			Platform.rigidbody.interpolation = _interpolation;
			Calibrated = false;
		}
	}
}
