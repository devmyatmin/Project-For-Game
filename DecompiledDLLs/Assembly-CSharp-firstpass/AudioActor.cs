using System.Runtime.CompilerServices;
using Beam;
using UnityEngine;

public class AudioActor
{
	private static int _actors;

	private static int _i;

	private IAudioSource _source;

	public IAudioSource Source
	{
		get
		{
			if (!Handle.IsValid())
			{
				return NullAudioSource.Instance;
			}
			return _source;
		}
		private set
		{
			_source = value;
		}
	}

	public AudioActorHandle Handle { get; private set; }

	public Vector3 Position
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

	public AudioActor(IAudioSource source)
	{
		Source = source;
		Source.SpatialBlend = 0f;
		Source.Pan = 0f;
		Handle = new AudioActorHandle(++_i);
		_actors++;
	}

	public void Dispose()
	{
		if (Handle.IsValid())
		{
			_actors--;
			if (_actors == 0)
			{
				_i = 0;
			}
			Source.Stop();
			Source.Clip = null;
			Source = null;
			Handle = AudioActorHandle.Empty;
		}
	}
}
