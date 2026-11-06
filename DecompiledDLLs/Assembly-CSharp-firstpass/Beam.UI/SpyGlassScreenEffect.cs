using Beam.PostEffects;
using UnityEngine;

namespace Beam.UI;

public class SpyGlassScreenEffect : ControllerBase, INeedPlayerComponent
{
	public const float FADE_DURATION = 0.5f;

	[SerializeField]
	private UImageViewAdapter _view;

	[SerializeField]
	private Texture2D _blurMaskTexture;

	private IPlayer _player;

	private IMenuEffectsController _menuEffects;

	public IPlayer Player => _player;

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		IMenuEffectsController component = ((Component)_player.PlayerCamera.Camera).GetComponent<MainMenuEffectsController>();
		_menuEffects = component ?? NullObject<NullMenuEffectsController>.Instance;
	}

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"SpyGlassScreenEffect:: No view component assigned!");
		}
		_menuEffects = _menuEffects ?? NullObject<NullMenuEffectsController>.Instance;
		_player.GameInput.SpyGlass.Toggled += SpyGlass_Toggled;
		_view.Enabled = false;
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as UImageViewAdapter;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"SpyGlassScreenEffect:: No view component assigned!");
		}
	}

	private void SpyGlass_Toggled(bool activated)
	{
		ToggleEffect(activated);
	}

	private void OnDestroy()
	{
		_player.GameInput.SpyGlass.Toggled -= SpyGlass_Toggled;
	}

	private void ToggleEffect(bool activated)
	{
		if (activated)
		{
			_view.Enabled = activated;
			_menuEffects.SetMaskTexture(_blurMaskTexture);
			_menuEffects.Show();
			Fade(1f);
		}
		else
		{
			FadedOut();
		}
	}

	private void FadedOut()
	{
		_view.Enabled = false;
		_menuEffects?.Hide();
	}

	private void Fade(float value)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_view.Color = new Color(_view.Color.r, _view.Color.g, _view.Color.b, value);
	}
}
