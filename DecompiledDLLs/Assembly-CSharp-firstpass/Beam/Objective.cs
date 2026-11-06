using System;
using UnityEngine;

namespace Beam;

public class Objective : ObjectiveBase
{
	[SerializeField]
	private int _id;

	[SerializeField]
	private string _description;

	private bool _isCompleted;

	public override int Id => _id;

	public override string Description => _description;

	public override bool IsCompleted => _isCompleted;

	public override event Action<ObjectiveBase> Completed;

	private Color GetColor()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (_id != 0)
		{
			return Color.white;
		}
		return Color.red;
	}

	public Objective(int id)
	{
		_id = id;
	}

	public override void Complete()
	{
		if (!_isCompleted)
		{
			_isCompleted = true;
			Completed?.Invoke(this);
			Completed = null;
		}
	}
}
