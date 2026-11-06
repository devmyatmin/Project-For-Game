namespace Beam.Utilities;

public struct Condition
{
	private int _value;

	public int Value => _value;

	public static Condition operator ++(Condition condition)
	{
		condition._value++;
		return condition;
	}

	public static Condition operator --(Condition condition)
	{
		condition._value--;
		if (condition._value < 0)
		{
			condition._value = 0;
		}
		return condition;
	}

	public static implicit operator bool(Condition condition)
	{
		return condition._value > 0;
	}
}
