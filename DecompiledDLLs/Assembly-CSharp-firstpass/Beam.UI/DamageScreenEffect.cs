using System;
using UnityEngine;

namespace Beam.UI;

public class DamageScreenEffect : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private UTextureViewAdapter _view;

	private IPlayer _player;

	public IPlayer Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"DamageScreenEffect:: No view component assigned!");
			_view = NullRawTextureViewAdapter.Instance;
		}
		_view.Enabled = false;
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as UTextureViewAdapter;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"DamageScreenEffect:: No view component assigned!");
			_view = NullRawTextureViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Statistics.Damaged += PlayerStatistics_Damaged;
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.Statistics.Damaged += PlayerStatistics_Damaged;
		}
	}

	private void PlayerStatistics_Damaged()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject, false);
		_view.Enabled = true;
		_view.Color = Color.white;
		LeanTween.value(((Component)this).gameObject, 1f, 0f, 5f).setOnUpdate((Action<float>)Fade_OnUpdate).setOnComplete((Action)Fade_OnComplete);
	}

	private void Fade_OnUpdate(float value)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Color color = _view.Color;
		color.a = value;
		_view.Color = color;
	}

	private void Fade_OnComplete()
	{
		_view.Enabled = false;
	}
}
