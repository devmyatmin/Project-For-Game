using System;

namespace Beam;

[Serializable]
public abstract class ObjectiveBase
{
	public abstract int Id { get; }

	public abstract string Description { get; }

	public abstract bool IsCompleted { get; }

	public abstract event Action<ObjectiveBase> Completed;

	public abstract void Complete();
}
