using System.Collections;
using System.Linq;
using Beam.Serialization.Json;
using Beam.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Intro;

public class AircraftObjectives : SerializedMonoBehaviour, ISaveable
{
	[SerializeField]
	private ObjectiveBase[] _objectives;

	private ChecklistNotification _checklistNotification;

	private bool _loaded;

	[Header("EVENTS")]
	public UnityEvent ObjectiveCompleted;

	public UnityEvent<AircraftObjectivesCompletedArgs> ObjectivesCompleted;

	private IEnumerator Start()
	{
		while (LevelLoader.IsLoading())
		{
			yield return null;
		}
		Initialize();
	}

	private void Initialize()
	{
		if (_loaded && IsAllObjectivesCompleted())
		{
			_checklistNotification = GetFinalObjectiveNotification();
			ObjectivesCompleted?.Invoke(new AircraftObjectivesCompletedArgs
			{
				IsFirstTime = false
			});
			return;
		}
		LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
		localizedNotification.Priority = NotificationPriority.Immediate;
		localizedNotification.Duration = -1f;
		localizedNotification.MessageText.SetTerm("AIRCRAFT_NTF_THIS_COULD_BE_MY_ESCAPE");
		_checklistNotification = new ChecklistNotification(localizedNotification);
		_checklistNotification.SetObjectives(_objectives);
		ObjectiveBase[] objectives = _objectives;
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Completed += Objective_Completed;
		}
	}

	private void OnDestroy()
	{
		ObjectiveBase[] objectives = _objectives;
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Completed -= Objective_Completed;
		}
	}

	public void EndGameVfx_ObjectivesCompleted()
	{
		_checklistNotification.Raise();
	}

	public void EndGame_EnteredProximity()
	{
		_checklistNotification.PlayerId = PlayerRegistry.LocalPlayer.Id;
		_checklistNotification.Disposed = false;
		_checklistNotification.Raise();
	}

	public void EndGame_LeftProximity()
	{
		_checklistNotification.Dispose();
	}

	public void CompleteObjective(int id)
	{
		ObjectiveBase objectiveBase = _objectives.FirstOrDefault((ObjectiveBase o) => o.Id == id);
		if (objectiveBase != null && !objectiveBase.IsCompleted)
		{
			objectiveBase.Complete();
			UnityEvent objectiveCompleted = ObjectiveCompleted;
			if (objectiveCompleted != null)
			{
				objectiveCompleted.Invoke();
			}
		}
	}

	public void CompleteFinalObjective()
	{
		_checklistNotification.Dispose();
	}

	private void Objective_Completed(ObjectiveBase objective)
	{
		if (IsAllObjectivesCompleted())
		{
			_checklistNotification?.Dispose();
			_checklistNotification = GetFinalObjectiveNotification();
			ObjectivesCompleted?.Invoke(new AircraftObjectivesCompletedArgs
			{
				IsFirstTime = true
			});
		}
	}

	private bool IsAllObjectivesCompleted()
	{
		return _objectives.All((ObjectiveBase o) => o.IsCompleted);
	}

	private bool CheckObjectiveIds()
	{
		return _objectives.Select((ObjectiveBase o) => o.Id).Distinct().Count() != _objectives.Length;
	}

	private ChecklistNotification GetFinalObjectiveNotification()
	{
		LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
		localizedNotification.Priority = NotificationPriority.Immediate;
		localizedNotification.Duration = -1f;
		localizedNotification.MessageText.SetTerm("AIRCRAFT_OBJECTIVE_GO_HOME");
		LocalizedObjective localizedObjective = new LocalizedObjective(new Objective(1));
		localizedObjective.DescriptionText.SetTerm("AIRCRAFT_OBJECTIVE_GO_HOME");
		ChecklistNotification checklistNotification = new ChecklistNotification(localizedNotification);
		checklistNotification.SetObjectives(localizedObjective);
		return checklistNotification;
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject jObject2 = new JObject();
		ObjectiveBase[] objectives = _objectives;
		foreach (ObjectiveBase objectiveBase in objectives)
		{
			JObject jObject3 = new JObject();
			jObject3.AddField("Id", objectiveBase.Id);
			jObject3.AddField("IsCompleted", objectiveBase.IsCompleted);
			jObject2.AddField("Objective", jObject3);
		}
		jObject.AddField("Objectives", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Objectives");
		if (field != null && !field.IsNull())
		{
			for (int i = 0; i < field.Children.Count; i++)
			{
				JObject jObject = field.Children[i];
				if (jObject.GetField("IsCompleted").GetValue<bool>())
				{
					int id = jObject.GetField("Id").GetValue<int>();
					_objectives.FirstOrDefault((ObjectiveBase o) => o.Id == id)?.Complete();
				}
			}
		}
		_loaded = true;
	}
}
