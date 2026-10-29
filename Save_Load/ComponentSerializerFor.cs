using System;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentSerializerFor : Attribute
{
	internal readonly Type type;

	public ComponentSerializerFor(Type type)
	{
		this.type = type;
	}
}
