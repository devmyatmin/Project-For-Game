using UnityEngine;

namespace Beam;

public interface IBase : IHasCraftingType
{
	GameObject gameObject { get; }

	Transform transform { get; }

	Transform TransformParent { get; set; }

	string DisplayName { get; }

	RaycastMode RaycastMode { get; }

	bool CanParent { get; set; }

	bool IsDamageable { get; }

	void Damage(float damage, IBase damager);

	void Destroy();

	void Parent();
}
