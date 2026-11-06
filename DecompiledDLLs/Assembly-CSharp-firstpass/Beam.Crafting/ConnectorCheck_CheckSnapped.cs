using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class ConnectorCheck_CheckSnapped : ConnectorCheck
{
	[SerializeField]
	private PlacementModifier_SnapBase _placementModifier;

	[SerializeField]
	private float _maxSnapAngle = 15f;

	[SerializeField]
	[Tooltip("If during check it's connected to a connector than the check is valid")]
	private bool _ignoredSnappedObjectIfConnected;

	[SerializeField]
	private LayerMask _layerMask;

	public override bool Check(Placement placement, Connector connector)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (_ignoredSnappedObjectIfConnected && BeamTeam.IsValid(connector))
		{
			return true;
		}
		if ((Object)(object)_placementModifier.SnappedObject != (Object)null && _layerMask.value > 0 && (_layerMask.value & (1 << _placementModifier.SnappedObject.layer)) == 0)
		{
			return false;
		}
		if (_maxSnapAngle > 0f && Mathf.Acos(Vector3.Dot(placement.Rotation * Vector3.up, Vector3.up)) * 57.29578f > _maxSnapAngle)
		{
			return false;
		}
		return _placementModifier.Snapped;
	}
}
