using System;
using System.Collections.Generic;
using Beam;
using UnityEngine;

public class AudioActorFactory<TAudioActor> : IDisposable where TAudioActor : AudioActor
{
	private sealed class AudioActorFactoryFinalizer
	{
		~AudioActorFactoryFinalizer()
		{
			AudioActorFactory<TAudioActor>.Dispose();
		}
	}

	private readonly AudioActorFactoryFinalizer _finalizer = new AudioActorFactoryFinalizer();

	private readonly Stack<TAudioActor> _actorPool = new Stack<TAudioActor>();

	private readonly List<TAudioActor> _activeActors = new List<TAudioActor>();

	public static AudioActorFactory<TAudioActor> Instance { get; } = new AudioActorFactory<TAudioActor>();

	public static IList<TAudioActor> GetActiveActors()
	{
		return Instance?._activeActors;
	}

	public static TAudioActor Create()
	{
		AudioActorFactory<TAudioActor> instance = Instance;
		if (instance == null)
		{
			return null;
		}
		return instance.Pop();
	}

	public static void Pool(TAudioActor audioActor)
	{
		Instance?.Pool_(audioActor);
	}

	public static void Dispose()
	{
		((IDisposable)Instance)?.Dispose();
	}

	private TAudioActor Create_()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		AudioSource val = new GameObject("Audio Source").AddComponent<AudioSource>();
		val.playOnAwake = false;
		val.dopplerLevel = 0f;
		return (TAudioActor)Activator.CreateInstance(typeof(TAudioActor), new UAudioSource(val));
	}

	private TAudioActor Pop()
	{
		TAudioActor val = ((_actorPool.Count > 0) ? _actorPool.Pop() : Create_());
		val.Source.Pitch = 1f;
		val.Source.Volume = 1f;
		_activeActors.Add(val);
		return val;
	}

	public void Pool_(TAudioActor audioActor)
	{
		audioActor.Source.Stop();
		audioActor.Source.Clip = null;
		_activeActors.Remove(audioActor);
		_actorPool.Push(audioActor);
	}

	void IDisposable.Dispose()
	{
		foreach (TAudioActor activeActor in _activeActors)
		{
			activeActor.Dispose();
		}
		foreach (TAudioActor item in _actorPool)
		{
			item.Dispose();
		}
		_activeActors.Clear();
		_actorPool.Clear();
	}

	public override string ToString()
	{
		return $"AudioActors ({typeof(TAudioActor)}):\nPooled: {_actorPool.Count}\nActive: {_activeActors.Count}\n";
	}
}
