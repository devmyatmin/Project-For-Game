using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class Interactive_FISH : Interactive_SKINNABLE, ISpearable, ISpearedInstance, IKillable
{
	private bool _isAlive = true;

	[Header("SPEARING")]
	[SerializeField]
	private bool _canSpear = true;

	bool IKillable.IsAlive => _isAlive;

	InteractiveType ISpearable.Type => CraftingType.InteractiveType;

	bool ISpearable.CanSpear => _canSpear;

	bool ISpearable.IsLocalFauna => false;

	uint ISpearable.LocalFaunaSpearedInstancePrefabId => 0u;

	IPhysical ISpearedInstance.PhysicsObject => this;

	IPickupable ISpearedInstance.PickupableObject => this;

	bool ISpearable.LocalFaunaCheckHit(Vector3 postion)
	{
		return false;
	}

	void ISpearable.LocalFaunaRelease()
	{
	}

	ISpearedInstance ISpearable.GetSpearedInstance(Vector3 position)
	{
		return this;
	}

	public void Kill(int killerId = -1)
	{
		if (_isAlive)
		{
			_isAlive = false;
			if (killerId != -1)
			{
				EventManager.RaiseEvent(new ExperienceEvent(killerId, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
				EventManager.RaiseEvent(new AddStatsEvent(killerId, new Stats
				{
					FishFished = 1
				}));
			}
		}
	}

	public override void EnablePhysics()
	{
		base.EnablePhysics();
	}

	public override void DisablePhysics()
	{
		base.DisablePhysics();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		_isAlive = false;
	}
}
