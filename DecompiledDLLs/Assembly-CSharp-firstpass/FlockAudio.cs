using System.Runtime.CompilerServices;
using UnityEngine;

public class FlockAudio : MonoBehaviour
{
	[SerializeField]
	private AudioClip _ambientClip;

	private AudioActorHandle _ambientHandle;

	public AudioClip AmbientClip
	{
		get
		{
			return _ambientClip;
		}
		set
		{
			_ambientClip = value;
		}
	}

	public Vector3 TargetPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	private void OnEnable()
	{
		_ambientHandle = AudioManager.GetAudioPlayer().Play3D(_ambientClip, ((Component)this).transform, AudioMixerChannels.FX, AudioRollOffDistance.VeryFar, AudioPlayMode.Persistent);
	}

	private void OnDisable()
	{
		Stop();
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		Vector3 position = transform.position;
		Vector3 val = TargetPosition - ((Component)this).transform.position;
		transform.position = position + val.normalized * 4f * Time.deltaTime;
	}

	public void Stop()
	{
		AudioManager.GetAudioPlayer().Stop(_ambientHandle);
		_ambientHandle = AudioActorHandle.Empty;
	}
}
