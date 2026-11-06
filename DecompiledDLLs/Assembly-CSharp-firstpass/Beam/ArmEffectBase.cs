using UnityEngine;

namespace Beam;

public abstract class ArmEffectBase : MonoBehaviour
{
	[SerializeField]
	private Renderer _view;

	private Statistics _statistics;

	private bool _active;

	public void SetPlayer(IPlayer player)
	{
		_statistics = player.Statistics;
	}

	private void Start()
	{
		if ((Object)(object)_statistics != (Object)null)
		{
			_statistics.PlayerEffectApplied += PlayerStatistics_EffectApplied;
			_statistics.PlayerEffectRemoved += PlayerStatistics_EffectRemoved;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_statistics != (Object)null)
		{
			_statistics.PlayerEffectApplied += PlayerStatistics_EffectApplied;
			_statistics.PlayerEffectRemoved += PlayerStatistics_EffectRemoved;
		}
	}

	private void PlayerStatistics_EffectApplied(PlayerEffect effect)
	{
		if (CheckPlayerEffect(effect))
		{
			_active = true;
			SetRenderer(_active);
		}
	}

	private void PlayerStatistics_EffectRemoved(PlayerEffect effect)
	{
		if (CheckPlayerEffect(effect))
		{
			_active = false;
			SetRenderer(_active);
		}
	}

	public void SetActive(bool enabled)
	{
		((Component)_view).gameObject.SetActive(enabled);
	}

	private void SetRenderer(bool enabled)
	{
		_view.enabled = enabled;
		if (enabled && !_active)
		{
			_view.enabled = false;
		}
	}

	protected abstract bool CheckPlayerEffect(PlayerEffect effect);
}
