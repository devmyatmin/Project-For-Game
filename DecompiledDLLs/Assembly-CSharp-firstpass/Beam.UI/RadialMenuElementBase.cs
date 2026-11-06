using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beam.UI;

public abstract class RadialMenuElementBase<TModel, TView> : MonoBehaviour, IRadialMenuElement<TModel> where TModel : class, IRadialMenuModel where TView : IRadialMenuElementViewAdapter
{
	[SerializeField]
	private TView _view;

	private TModel _selectedValue;

	private IList<TModel> _data;

	private int _selectedIndex;

	protected TView View => _view;

	public TModel SelectedValue => _selectedValue;

	protected IList<TModel> Data => _data;

	protected int SelectedIndex => _selectedIndex;

	public virtual void Initialize(IList<TModel> data)
	{
		_data = data;
		_selectedIndex = 0;
		IList<TModel> data2 = _data;
		_selectedValue = ((data2 != null && data2.Count > 0) ? _data.First() : null);
	}

	public void Enter()
	{
		_view.Selectable.Select();
	}

	public void Exit()
	{
		_view.Selectable.Deselect();
	}

	public virtual bool NavigateLeft()
	{
		if (_data == null || _data.Count == 0)
		{
			return false;
		}
		int num = _selectedIndex - 1;
		if (num < 0)
		{
			num = _data.Count - 1;
		}
		if (_selectedIndex != num)
		{
			_selectedIndex = num;
			_selectedValue = _data[_selectedIndex];
			return true;
		}
		return false;
	}

	public virtual bool NavigateRight()
	{
		if (_data == null || _data.Count == 0)
		{
			return false;
		}
		int num = _selectedIndex + 1;
		if (num > _data.Count - 1)
		{
			num = 0;
		}
		if (_selectedIndex != num)
		{
			_selectedIndex = num;
			_selectedValue = _data[_selectedIndex];
			return true;
		}
		return false;
	}
}
