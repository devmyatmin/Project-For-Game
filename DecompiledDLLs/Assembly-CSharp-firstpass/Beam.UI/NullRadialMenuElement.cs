using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public class NullRadialMenuElement : NullObject<NullRadialMenuElement>, IRadialMenuElement<NullRadialMenuModel>
{
	public Transform transform => NullTransform.Instance;

	public NullRadialMenuModel SelectedValue => NullObject<NullRadialMenuModel>.Instance;

	public void Initialize(IList<NullRadialMenuModel> data)
	{
		Debug.LogError((object)"NullRadialMenuElement::Initialize:: Not implemented!");
	}

	public void Enter()
	{
		Debug.LogError((object)"NullRadialMenuElement::Enter:: Not implemented!");
	}

	public void Exit()
	{
		Debug.LogError((object)"NullRadialMenuElement::Exit:: Not implemented!");
	}

	public bool NavigateLeft()
	{
		Debug.LogError((object)"NullRadialMenuElement::NavigateLeft:: Not implemented!");
		return false;
	}

	public bool NavigateRight()
	{
		Debug.LogError((object)"NullRadialMenuElement::NavigateRight:: Not implemented!");
		return false;
	}
}
