using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public interface IRadialMenuElement<TModel> where TModel : class, IRadialMenuModel
{
	Transform transform { get; }

	TModel SelectedValue { get; }

	void Initialize(IList<TModel> data);

	void Enter();

	void Exit();

	bool NavigateLeft();

	bool NavigateRight();
}
