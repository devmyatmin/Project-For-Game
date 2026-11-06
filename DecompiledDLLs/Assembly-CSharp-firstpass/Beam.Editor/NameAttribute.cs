using UnityEngine;

namespace Beam.Editor;

public class NameAttribute : PropertyAttribute
{
	private readonly string _name;

	public string Name => _name;

	public NameAttribute(string name)
	{
		_name = name;
	}
}
