using System.Collections;
using Beam;
using UnityEngine;

public class EnvironmentAudio : MonoBehaviour
{
	[SerializeField]
	private AudioSource _oceanCalm;

	[SerializeField]
	private AudioSource _oceanDeep;

	private AudioLowPassFilter _oceanLowPass;

	private AudioLowPassFilter _oceanDeepLowPass;

	private bool _update;

	private IEnumerator Start()
	{
		_oceanCalm.priority = 64;
		_oceanLowPass = ((Component)_oceanCalm).GetComponent<AudioLowPassFilter>();
		_oceanLowPass.lowpassResonanceQ = 2f;
		_oceanLowPass.cutoffFrequency = 250f;
		_oceanDeep.priority = 64;
		_oceanDeepLowPass = ((Component)_oceanDeep).GetComponent<AudioLowPassFilter>();
		_oceanDeepLowPass.lowpassResonanceQ = 2f;
		_oceanDeepLowPass.cutoffFrequency = 80f;
		while (!LevelLoader.IsPlayable())
		{
			yield return null;
		}
		if ((int)Game.State != 4)
		{
			PlayerRegistry.LocalPlayer.PlayerCamera.ToggleUnderwater += Cameras_ToggleUnderwater;
		}
		_update = true;
	}

	private void OnDestroy()
	{
		if (PlayerRegistry.LocalPlayer != null && (Object)(object)PlayerRegistry.LocalPlayer.PlayerCamera != (Object)null)
		{
			PlayerRegistry.LocalPlayer.PlayerCamera.ToggleUnderwater -= Cameras_ToggleUnderwater;
		}
	}

	private void Cameras_ToggleUnderwater(bool underwater)
	{
		if (underwater)
		{
			((Behaviour)_oceanLowPass).enabled = true;
			((Behaviour)_oceanDeepLowPass).enabled = true;
		}
		else
		{
			((Behaviour)_oceanLowPass).enabled = false;
			((Behaviour)_oceanDeepLowPass).enabled = false;
		}
	}

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!_update)
		{
			_oceanCalm.volume = 0f;
			_oceanDeep.volume = 0f;
		}
		else if ((int)Game.State == 4)
		{
			float y = ((Component)Camera.main).transform.position.y;
			_oceanCalm.volume = Mathf.Clamp01(1f - Mathf.Clamp01(y * 0.1f)) * 0.5f;
			_oceanDeep.volume = Mathf.Clamp01(1f - Mathf.Clamp01(y * 0.015f)) * 0.1f;
		}
		else if (PlayerRegistry.LocalPlayer != null)
		{
			float y2 = ((Component)PlayerRegistry.LocalPlayer.PlayerCamera.Camera).transform.position.y;
			if (Cameras.IsUnderwater(((Component)PlayerRegistry.LocalPlayer.PlayerCamera.Camera).transform))
			{
				_oceanCalm.volume = Mathf.Clamp01(1f - Mathf.Clamp01((0f - y2) * 0.05f)) * 0.5f;
				_oceanDeep.volume = Mathf.Clamp01(1f - Mathf.Clamp01((0f - y2) * 0.1f)) * 0.5f;
			}
			else
			{
				_oceanCalm.volume = Mathf.Clamp01(1f - Mathf.Clamp01(y2 * 0.1f)) * 0.5f;
				_oceanDeep.volume = Mathf.Clamp01(1f - Mathf.Clamp01(y2 * 0.015f)) * 0.1f;
			}
		}
	}
}
