using System;
using System.Collections;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.UI;
using Beam.Utilities;
using UnityEngine;

namespace Funlabs;

public class SpyGlass : MonoBehaviour, INeedPlayerComponent, IGameInputHandler
{
	public const float ZOOM_DURATION = 0.3f;

	private const int NORMAL_ZOOM_LEVEL = 1;

	private const int X2_ZOOM_LEVEL = 2;

	private const int MAX_ZOOM_LEVEL = 8;

	private const float HIDE_DELAY = 0.8f;

	private const float MIN_AXIS_DIFFERENCE = 0.05f;

	private IGameInputController _controller;

	private InteractiveObject_SPYGLASS _spyglass;

	private bool _isUsing;

	private Coroutine _spyglassCoroutine;

	private Coroutine _fpvCoroutine;

	private float _currentNormalFOV;

	private float _currentNormalZoom = 1f;

	private float _currentFOV;

	private float _targetFOV;

	private float _currentZoomLevel = 1f;

	private IPlayer _player;

	private float FULL_ZOOM_FOV => Mathf.Atan(Mathf.Tan(Cameras.DefaultFOV * ((float)Math.PI / 180f)) / 8f) * 57.29578f;

	private float X2_ZOOM_FOV => Mathf.Atan(Mathf.Tan(Cameras.DefaultFOV * ((float)Math.PI / 180f)) / 2f) * 57.29578f;

	public float ZoomLevel => _currentZoomLevel;

	IPlayer INeedPlayerComponent.Player => _player;

	public event Action<bool> Toggled;

	protected virtual void OnToggled(bool activated)
	{
		Toggled?.Invoke(activated);
	}

	void IGameInputHandler.Initialize(IGameInputController controller)
	{
		_controller = controller;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Statistics.KnockedOut += PlayerStatistics_KnockedOut;
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_player) && Object.op_Implicit((Object)(object)_player.Statistics))
		{
			_player.Statistics.KnockedOut -= PlayerStatistics_KnockedOut;
		}
	}

	private void PlayerStatistics_KnockedOut(bool state)
	{
		Abort();
	}

	void IGameInputHandler.CheckInput()
	{
		if (_player.IsOwner && !_isUsing && _player.Input.GetButton(8))
		{
			_spyglass = _player.Holder.CurrentObject as InteractiveObject_SPYGLASS;
			if (Object.op_Implicit((Object)(object)_spyglass) && !_player.Movement.ClimbingLadder)
			{
				_spyglassCoroutine = ((MonoBehaviour)this).StartCoroutine(SpyglassSequence());
				_fpvCoroutine = ((MonoBehaviour)this).StartCoroutine(ForceFirstPersonSequence());
			}
		}
	}

	private void Setup(bool activate)
	{
		if (!activate)
		{
			_currentNormalZoom = 1f;
			_currentNormalFOV = Cameras.DefaultFOV;
		}
		else
		{
			_currentNormalZoom = 2f;
			_currentNormalFOV = X2_ZOOM_FOV;
		}
	}

	private void SetZoomPercent(float percent)
	{
		_currentZoomLevel = Mathf.Lerp(_currentNormalZoom, 8f, percent);
		_targetFOV = Mathf.Lerp(FULL_ZOOM_FOV, _currentNormalFOV, 1f - percent);
		_currentFOV = _player.PlayerCamera.Camera.fieldOfView;
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Zoom, _currentFOV, _targetFOV, 0.3f);
		EventManager.RaiseEvent(new SpyGlassZoomChangedEvent(_currentZoomLevel));
	}

	private void Zoom(float fov)
	{
		_player.PlayerCamera.Camera.fieldOfView = fov;
	}

	private IEnumerator ForceFirstPersonSequence()
	{
		while (_isUsing)
		{
			_player.PlayerCamera.ForceFirstPersonView();
			yield return null;
		}
	}

	private IEnumerator SpyglassSequence()
	{
		_isUsing = true;
		_controller.CurrentHandler = this;
		_player.PlayerUI.Suspend(suspend: true);
		_player.Character.Animater.Play(_spyglass.Primary);
		float waitingTime = 0.7f;
		float timer = 0f;
		while (_player.Input.GetButton(8) && timer <= waitingTime)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if (timer < waitingTime)
		{
			FinishUsing();
			yield break;
		}
		Setup(activate: true);
		SetZoomPercent(0f);
		Singleton<Fader>.Instance.SetFadeColor(Color.black);
		yield return Singleton<Fader>.Instance.FadeOut(0.5f);
		_player.Character.PlayIdleAnimation(AnimationType.SPYGLASS_IDLE);
		_player.Character.SpyGlassRendering(value: true);
		_spyglass.ToggleShadowsOnly(value: true);
		OnToggled(activated: true);
		yield return Singleton<Fader>.Instance.FadeIn(0.5f);
		float lastPercent = 0f;
		while (_player.Input.GetButton(8))
		{
			float axis = _player.Input.GetAxis(7);
			if (Mathf.Abs(lastPercent - axis) > 0.05f)
			{
				SetZoomPercent(axis);
				lastPercent = axis;
			}
			yield return null;
		}
		FinishUsing();
	}

	private void FinishUsing()
	{
		_player.Character.Animater.Play(AnimationType.SPYGLASS_OUT);
		_player.Character.PlayIdleAnimation(_spyglass.Idle);
		CleanUp();
	}

	private void Abort()
	{
		if (_isUsing)
		{
			if (_spyglassCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_spyglassCoroutine);
			}
			if (_fpvCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_fpvCoroutine);
			}
			CleanUp();
		}
	}

	private void CleanUp()
	{
		OnToggled(activated: false);
		Setup(activate: false);
		SetZoomPercent(0f);
		_player.Character.SpyGlassRendering(value: false);
		_spyglass.ToggleShadowsOnly(value: false);
		_spyglass = null;
		_controller.CurrentHandler = null;
		_player.PlayerUI.Suspend(suspend: false);
		_isUsing = false;
	}
}
