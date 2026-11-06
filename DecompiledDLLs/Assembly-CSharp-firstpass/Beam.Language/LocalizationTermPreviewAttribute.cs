using System;

namespace Beam.Language;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LocalizationTermPreviewAttribute : Attribute
{
	public bool Preview;
}
