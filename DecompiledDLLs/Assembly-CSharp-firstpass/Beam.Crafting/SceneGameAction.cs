using System;
using Beam.Language;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

[Serializable]
public class SceneGameAction : IGameAction
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _playerActionRequiredDescription;

	[SerializeField]
	[RewiredActionDropdown]
	private int _elementRequiredId;

	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _resultingActionDescription;

	[SerializeField]
	private GameActionEvent _action;

	public string PlayerActionRequiredDescription => _playerActionRequiredDescription;

	public int ElementRequiredId => _elementRequiredId;

	public string ResultingActionDescription => _resultingActionDescription;

	public void DoAction(IBase obj)
	{
		((UnityEvent<IBase>)_action)?.Invoke(obj);
	}
}
