using System;

namespace Beam.UI;

public class Property<T>
{
	private T _value;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (!object.Equals(_value, value))
			{
				_value = value;
				ValueChanged?.Invoke(_value);
			}
		}
	}

	public event Action<T> ValueChanged;

	public Property(T value)
	{
		_value = value;
	}

	public void ClearSubscribers()
	{
		ValueChanged = null;
	}
}
