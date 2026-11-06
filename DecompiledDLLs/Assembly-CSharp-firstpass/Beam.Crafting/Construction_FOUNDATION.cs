using UnityEngine;

namespace Beam.Crafting;

public class Construction_FOUNDATION : Constructing, IHaveAttribute
{
	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type = IconType.Construction;

	[SerializeField]
	private Transform _geometryCenter;

	IconType IHaveAttribute.Type => _type;

	public Transform GeometryCenter
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_geometryCenter))
			{
				_geometryCenter = transform;
			}
			return _geometryCenter;
		}
	}

	float IHaveAttribute.GetValue()
	{
		return HealthPoints / MaximumHealthPoints;
	}

	protected override void Awake()
	{
		base.Awake();
		ProximityDistance += 0.3f;
	}

	public override void Damage(float damage, IBase damager)
	{
		base.Damage(damage, damager);
	}
}
