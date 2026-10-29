using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomSerializerFor : Attribute
{
	internal readonly Type type;

	public CustomSerializerFor(Type type)
	{
		this.type = type;
	}
}
