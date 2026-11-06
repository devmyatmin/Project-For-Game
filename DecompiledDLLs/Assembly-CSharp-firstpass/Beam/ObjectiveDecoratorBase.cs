using System;
using UnityEngine;

namespace Beam;

public abstract class ObjectiveDecoratorBase : ObjectiveBase
{
	[SerializeField]
	private ObjectiveBase _objective;

	public override int Id => _objective.Id;

	public override string Description => _objective.Description;

	public override bool IsCompleted => _objective.IsCompleted;

	public override event Action<ObjectiveBase> Completed;

	protected ObjectiveDecoratorBase(ObjectiveBase objective)
	{
		_objective = objective;
	}

	public override void Complete()
	{
		if (!IsCompleted)
		{
			_objective.Complete();
			Completed?.Invoke(this);
			Completed = null;
		}
	}
}
