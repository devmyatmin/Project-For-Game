using System;
using UnityEngine;

namespace Beam;

public interface IMissionGoal
{
	string Name { get; }

	Vector3 Location { get; }

	event Action UpdateCompletion;

	event Action Completed;

	float GetCompletionPercent();

	void Reset();
}
