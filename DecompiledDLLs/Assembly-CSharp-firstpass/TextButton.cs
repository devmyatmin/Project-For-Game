using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TextButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private ButtonId _id;

	public Button Button => _button;

	public ButtonId Id => _id;
}
