using System;
using UnityEngine;

namespace Beam;

[Serializable]
public class CellObjectSettings<TCellObject> : IPlacementSettings where TCellObject : MonoBehaviour, ICellObject
{
	[SerializeField]
	private TCellObject _prefab;

	[SerializeField]
	private DistributionType _distributionType;

	[SerializeField]
	private int _maximumInstances;

	[SerializeField]
	[Range(0f, 100f)]
	private float _rarity;

	[SerializeField]
	private float _minimumWorldHeight;

	[SerializeField]
	private float _maximumWorldHeight;

	[SerializeField]
	private float _xPositionJitter;

	[SerializeField]
	private float _zPositionJitter;

	[SerializeField]
	private PlacementModifier[] _placementModifiers;

	[SerializeField]
	private PlacementValidator[] _placementValidators;

	public TCellObject Prefab => _prefab;

	public DistributionType DistributionType => _distributionType;

	public int MaximumInstances => _maximumInstances;

	public float Rarity => _rarity;

	public float MinimumWorldHeight => _minimumWorldHeight;

	public float MaximumWorldHeight => _maximumWorldHeight;

	public float XPositionJitter => _xPositionJitter;

	public float ZPositionJitter => _zPositionJitter;

	public PlacementModifier[] PlacementModifiers => _placementModifiers;

	public PlacementValidator[] PlacementValidators => _placementValidators;

	Vector3 IPlacementSettings.Size
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Bounds bounds = _prefab.GetBounds();
			return bounds.size;
		}
	}
}
