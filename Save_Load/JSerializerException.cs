using System;

namespace Beam.Serialization.Json;

public class JSerializerException : Exception
{
	public JSerializerException()
	{
	}

	public JSerializerException(string message)
		: base(message)
	{
	}

	public JSerializerException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
