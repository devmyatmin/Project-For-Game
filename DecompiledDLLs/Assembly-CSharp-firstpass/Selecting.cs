using Beam;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Events;

public class Selecting : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Highlighter _highlighter;

	[SerializeField]
	private InteracterUnityEvent _onSelected = new InteracterUnityEvent();

	public UnityEvent Deselected;

	public InteracterUnityEvent OnSelected
	{
		get
		{
			return _onSelected;
		}
		set
		{
			_onSelected = value;
		}
	}

	private void Start()
	{
		if ((Object)(object)_highlighter != (Object)null)
		{
			_highlighter.seeThrough = false;
		}
	}

	public void Select(IPlayer player)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_highlighter != (Object)null)
		{
			_highlighter.OnParams(Color.white);
			_highlighter.On();
		}
		((UnityEvent<IPlayer>)OnSelected).Invoke(player);
	}

	public void Deselect()
	{
		if ((Object)(object)_highlighter != (Object)null)
		{
			_highlighter.Off();
		}
		UnityEvent deselected = Deselected;
		if (deselected != null)
		{
			deselected.Invoke();
		}
	}
}
