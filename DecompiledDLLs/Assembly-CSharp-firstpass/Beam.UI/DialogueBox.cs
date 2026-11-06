using System;
using Beam.Utilities;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class DialogueBox : MonoBehaviour
{
	private static DialogueBox _Instance;

	[SerializeField]
	private DialogueBoxViewAdapter _dialoguePrefab;

	[SerializeField]
	private GameObject _dialogueContainer;

	public static bool Notifying
	{
		get
		{
			if ((Object)(object)_Instance != (Object)null)
			{
				return _Instance._dialogueContainer.transform.childCount > 0;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)_Instance))
		{
			BeamTeam.Destroy<DialogueBox>(this);
		}
		else
		{
			_Instance = this;
		}
	}

	private DialogueBoxViewAdapter _Show(Player input, string title, string message, DialogueButton buttons, DialogueIcon icon, DialogueType type, bool keepCurrent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		UserInterfaceContext val = new UserInterfaceContext(RewiredUtilities.GetUserInterfaceFromInput(input));
		try
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = Object.Instantiate<DialogueBoxViewAdapter>(_dialoguePrefab);
			((Component)dialogueBoxViewAdapter).transform.SetParent(_dialogueContainer.transform, false);
			dialogueBoxViewAdapter.Title.Text = title;
			dialogueBoxViewAdapter.Message.Text = message;
			dialogueBoxViewAdapter.SetIcon(icon);
			dialogueBoxViewAdapter.SetType(type);
			dialogueBoxViewAdapter.SetButtons(buttons);
			dialogueBoxViewAdapter.SetInput(input);
			return dialogueBoxViewAdapter;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private DialogueBoxViewAdapter _Show(Player input, string title, string message, string action1, string action2, string action3, DialogueButton buttons, DialogueIcon icon, DialogueType type)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		BeamTeam.DestroyAllChildren(_dialogueContainer.transform);
		UserInterfaceContext val = new UserInterfaceContext(RewiredUtilities.GetUserInterfaceFromInput(input));
		try
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = Object.Instantiate<DialogueBoxViewAdapter>(_dialoguePrefab);
			((Component)dialogueBoxViewAdapter).transform.SetParent(_dialogueContainer.transform, false);
			dialogueBoxViewAdapter.Title.Text = title;
			dialogueBoxViewAdapter.Message.Text = message;
			dialogueBoxViewAdapter.Action1 = action1;
			dialogueBoxViewAdapter.Action2 = action2;
			dialogueBoxViewAdapter.Action3 = action3;
			dialogueBoxViewAdapter.SetIcon(icon);
			dialogueBoxViewAdapter.SetType(type);
			dialogueBoxViewAdapter.SetButtons(buttons);
			dialogueBoxViewAdapter.SetInput(input);
			return dialogueBoxViewAdapter;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static DialogueBoxViewAdapter Show(Player input, string title, string message, DialogueButton buttons, DialogueIcon icon = DialogueIcon.None, DialogueType type = DialogueType.Base)
	{
		return _Instance?._Show(input, title, message, buttons, icon, type, keepCurrent: false);
	}

	public static DialogueBoxViewAdapter Show(Player input, string title, string message, string action1, string action2, string action3, DialogueIcon icon = DialogueIcon.None, DialogueType type = DialogueType.Base)
	{
		return _Instance?._Show(input, title, message, action1, action2, action3, DialogueButton.Custom, icon, type);
	}
}
