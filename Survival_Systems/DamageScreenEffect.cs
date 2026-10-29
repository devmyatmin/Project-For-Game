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
		if (_view == null)
		{
			Debug.LogError("DamageScreenEffect:: No view component assigned!");
			_view = NullRawTextureViewAdapter.Instance;
		}
		_view.Enabled = false;
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as UTextureViewAdapter;
		if (_view == null)
		{
			Debug.LogError("DamageScreenEffect:: No view component assigned!");
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
		LeanTween.cancel(base.gameObject, callOnComplete: false);
		_view.Enabled = true;
		_view.Color = Color.white;
		LeanTween.value(base.gameObject, 1f, 0f, 5f).setOnUpdate(Fade_OnUpdate).setOnComplete(Fade_OnComplete);
	}

	private void Fade_OnUpdate(float value)
	{
		Color color = _view.Color;
		color.a = value;
		_view.Color = color;
	}

	private void Fade_OnComplete()
	{
		_view.Enabled = false;
	}
}
