using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Beam;

public class Tutorial : SerializedMonoBehaviour, IPersistentSaveable, ISaveable
{
	[Header("TUTORIAL OBJECTIVES")]
	[OdinSerialize]
	private List<ObjectiveBase> _objectives;

	[Header("TUTORIAL SEQUENCE")]
	[SerializeField]
	private List<SequenceNodeBase> _sequenceNodes = new List<SequenceNodeBase>();

	private bool _enabled = true;

	private bool _previousState;

	private HashSet<int> _completedObjectives = new HashSet<int>();

	public static Tutorial Instance { get; private set; }

	public event Action Completed;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private IEnumerator Start()
	{
		InitializeSequenceNodes();
		_objectives.Last().Completed += LastObjective_Completed;
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		EventManager.AddListener<TutorialObjectiveCompletedEvent>(EventManager_TutorialObjectiveCompletedEvent);
		while (LevelLoader.IsLoading())
		{
			yield return null;
		}
		yield return (object)new WaitForSeconds(3f);
		LoadOptions();
	}

	private void OnDestroy()
	{
		_objectives.Last().Completed -= LastObjective_Completed;
		Completed = null;
		UnsubscribeEvents();
		Instance = null;
	}

	private void UnsubscribeEvents()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		EventManager.RemoveListener<TutorialObjectiveCompletedEvent>(EventManager_TutorialObjectiveCompletedEvent);
	}

	private void LoadOptions()
	{
		if (LevelLoader.IsLoading())
		{
			return;
		}
		if (_sequenceNodes.Last().Completed)
		{
			if (_previousState)
			{
				KillSequenceNodes();
			}
			UnsubscribeEvents();
			return;
		}
		_enabled = Options.GeneralSettings.Tutorial == Quality_OFF_ON.ON;
		if (_previousState != _enabled)
		{
			if (_enabled)
			{
				BeginSequence();
			}
			else
			{
				KillSequenceNodes();
			}
			_previousState = _enabled;
		}
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void EventManager_TutorialObjectiveCompletedEvent(TutorialObjectiveCompletedEvent e)
	{
		_completedObjectives.Add(e.Id);
		if (_enabled)
		{
			GetObjective(e.Id)?.Complete();
		}
	}

	private void InitializeSequenceNodes()
	{
		for (int i = 0; i < _sequenceNodes.Count; i++)
		{
			SequenceNodeBase sequenceNodeBase = _sequenceNodes[i];
			sequenceNodeBase.Initialize();
			int num = i + 1;
			if (num < _sequenceNodes.Count)
			{
				sequenceNodeBase.NextSequenceNode = _sequenceNodes[num];
			}
		}
	}

	private void BeginSequence()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!_enabled || MPExtensions.IsCoop(Game.Mode))
		{
			return;
		}
		foreach (int completedObjective in _completedObjectives)
		{
			GetObjective(completedObjective)?.Complete();
		}
		SequenceNodeBase sequenceNodeBase = null;
		for (int i = 0; i < _sequenceNodes.Count; i++)
		{
			SequenceNodeBase sequenceNodeBase2 = _sequenceNodes[i];
			if (!sequenceNodeBase2.Completed)
			{
				if (sequenceNodeBase == null)
				{
					sequenceNodeBase = sequenceNodeBase2;
				}
				if (sequenceNodeBase2.NextSequenceNode == null && i < _sequenceNodes.Count - 1)
				{
					Debug.LogError((object)("TUTORIAL :: " + sequenceNodeBase2?.ToString() + " does NOT have a NextSequenceNode set!"));
				}
				sequenceNodeBase2.Reset();
			}
		}
		sequenceNodeBase?.Execute();
	}

	private void KillSequenceNodes()
	{
		for (int i = 0; i < _sequenceNodes.Count; i++)
		{
			_sequenceNodes[i].Kill();
		}
	}

	private ObjectiveBase GetObjective(int id)
	{
		foreach (ObjectiveBase objective in _objectives)
		{
			if (objective.Id == id)
			{
				return objective;
			}
		}
		return null;
	}

	private void LastObjective_Completed(ObjectiveBase objective)
	{
		Completed?.Invoke();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		for (int i = 0; i < _sequenceNodes.Count; i++)
		{
			jObject.AddField("SN_Completed_" + i, _sequenceNodes[i].Completed);
		}
		for (int j = 0; j < _objectives.Count; j++)
		{
			jObject.AddField("OBJ_Completed_" + j, _completedObjectives.Contains(_objectives[j].Id));
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		for (int i = 0; i < _sequenceNodes.Count; i++)
		{
			JObject field = data.GetField("SN_Completed_" + i);
			if (field != null && field.GetValue<bool>())
			{
				_sequenceNodes[i].Complete();
			}
		}
		for (int j = 0; j < _objectives.Count; j++)
		{
			JObject field2 = data.GetField("OBJ_Completed_" + j);
			if (field2 != null && field2.GetValue<bool>())
			{
				_completedObjectives.Add(_objectives[j].Id);
			}
		}
	}
}
