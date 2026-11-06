using System;

public struct AudioActorHandle : IEquatable<AudioActorHandle>
{
	private readonly int _id;

	public static AudioActorHandle Empty { get; } = new AudioActorHandle(0);

	public AudioActorHandle(int id)
	{
		_id = id;
	}

	public bool IsValid()
	{
		return _id != 0;
	}

	public bool Equals(AudioActorHandle other)
	{
		return _id == other._id;
	}

	public override bool Equals(object other)
	{
		if (other is AudioActorHandle)
		{
			return Equals((AudioActorHandle)other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _id;
	}

	public static bool operator ==(AudioActorHandle a, AudioActorHandle b)
	{
		return a._id == b._id;
	}

	public static bool operator !=(AudioActorHandle a, AudioActorHandle b)
	{
		return a._id != b._id;
	}

	public override string ToString()
	{
		int id = _id;
		return id.ToString();
	}
}
