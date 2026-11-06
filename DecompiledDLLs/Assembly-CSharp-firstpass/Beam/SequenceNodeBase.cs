using System;

namespace Beam;

[Serializable]
public abstract class SequenceNodeBase
{
	private bool _completed;

	public bool Completed => _completed;

	public SequenceNodeBase NextSequenceNode { get; set; }

	public virtual void Initialize()
	{
	}

	public virtual void Reset()
	{
		_completed = false;
	}

	public abstract void Execute();

	public virtual void Complete()
	{
		_completed = true;
	}

	public void Continue()
	{
		NextSequenceNode?.Execute();
	}

	public virtual void Kill()
	{
	}
}
