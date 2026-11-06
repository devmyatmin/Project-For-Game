using UnityEngine;

namespace Beam.UI;

public class DialogueViewAdapter : ViewAdapterBase
{
	[SerializeField]
	private GameObject _dialoguePanel;

	public GameObject DialogueContainer;

	public GameObject DialoguePrefab;

	public override void Show()
	{
		if (!_dialoguePanel.activeSelf)
		{
			_dialoguePanel.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (!_dialoguePanel.activeSelf)
		{
			_dialoguePanel.SetActive(true);
			base.Hide();
		}
	}
}
