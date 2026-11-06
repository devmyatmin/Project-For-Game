using System;

namespace Beam;

public class NullObject<TNull>
{
	public static TNull Instance { get; } = Activator.CreateInstance<TNull>();
}
