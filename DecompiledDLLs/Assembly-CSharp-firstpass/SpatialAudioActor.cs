using Beam;
using UnityEngine;

public class SpatialAudioActor : AudioActor
{
	public Transform Target { get; set; }

	public SpatialAudioActor(IAudioSource source)
		: base(source)
	{
	}

	public virtual void Update(int listener, float distanceFromListener)
	{
	}
}
