using Beam.Events;
using UnityEngine;

namespace Beam.UI;

public class ChecklistElementController : MonoBehaviour
{
	[SerializeField]
	private ChecklistElementViewBase _view;

	private ObjectiveBase _objective;

	public ChecklistElementViewBase View => _view;

	public void Initialize(ObjectiveBase objective)
	{
		_objective = objective;
		_objective.Completed += Objective_Completed;
		_view.SetDescription(_objective.Description);
		if (_objective.IsCompleted)
		{
			Objective_Completed(_objective);
		}
	}

	private void Start()
	{
		EventManager.AddListener<RewiredControllerChangedEvent>(EventManager_RewiredControllerChanged);
	}

	private void OnDestroy()
	{
		if (_objective != null)
		{
			_objective.Completed -= Objective_Completed;
			_objective = null;
		}
		EventManager.RemoveListener<RewiredControllerChangedEvent>(EventManager_RewiredControllerChanged);
	}

	private void EventManager_RewiredControllerChanged(RewiredControllerChangedEvent e)
	{
		Refresh();
	}

	public void Refresh()
	{
		if (_objective != null)
		{
			_view.SetDescription(_objective.Description);
		}
	}

	private void Objective_Completed(ObjectiveBase objective)
	{
		_view.SetComplete();
		_objective.Completed -= Objective_Completed;
		_objective = null;
	}
}
