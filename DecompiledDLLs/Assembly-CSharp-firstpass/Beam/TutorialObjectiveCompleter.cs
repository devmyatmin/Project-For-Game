using Beam.Events;
using UnityEngine;

namespace Beam;

public class TutorialObjectiveCompleter : MonoBehaviour
{
	[SerializeField]
	private int _tutorialObjectiveId;

	[SerializeField]
	private bool _completeOnStart;

	private bool _completed;

	private void Start()
	{
		if (_completeOnStart)
		{
			Complete();
		}
	}

	public void Complete()
	{
		if (!_completed)
		{
			EventManager.RaiseEvent(new TutorialObjectiveCompletedEvent(_tutorialObjectiveId));
			_completed = true;
		}
	}
}
