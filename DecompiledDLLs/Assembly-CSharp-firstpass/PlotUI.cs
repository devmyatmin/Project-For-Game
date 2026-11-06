using UnityEngine;
using UnityEngine.UI;

public class PlotUI : MonoBehaviour
{
	[SerializeField]
	private Image[] _dropSprites;

	public void Refresh(int value)
	{
		for (int i = 0; i < _dropSprites.Length; i++)
		{
			((Behaviour)_dropSprites[i]).enabled = value - 1 >= i;
		}
	}
}
