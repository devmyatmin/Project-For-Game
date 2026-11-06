using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beam.UI;

[RequireComponent(typeof(Button))]
public class UButtonViewAdapter : ButtonViewAdapterBase<Button>
{
	public override Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			ColorBlock colors = ((Selectable)Button).colors;
			return colors.highlightedColor;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			ColorBlock colors = ((Selectable)Button).colors;
			colors.highlightedColor = value;
			((Selectable)Button).colors = colors;
		}
	}

	public override bool Interactable
	{
		get
		{
			return ((Selectable)Button).interactable;
		}
		set
		{
			((Selectable)Button).interactable = value;
		}
	}

	public override bool Enabled
	{
		get
		{
			return ((Selectable)Button).interactable;
		}
		set
		{
			((Selectable)Button).interactable = value;
		}
	}

	public override Button GetUnderlyingElement()
	{
		return Button;
	}

	protected virtual void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		((UnityEventBase)Button.onClick).RemoveAllListeners();
		((UnityEvent)Button.onClick).AddListener((UnityAction)PerformClick);
	}

	protected override void OnDestroy()
	{
		((UnityEventBase)Button.onClick).RemoveAllListeners();
		Button.onClick = null;
		base.OnDestroy();
	}

	public override void Select()
	{
		((Selectable)Button).Select();
		((Selectable)Button).OnSelect((BaseEventData)null);
		base.Select();
	}
}
