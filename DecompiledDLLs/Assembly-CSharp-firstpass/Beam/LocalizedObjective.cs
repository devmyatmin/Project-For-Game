using Beam.Language;
using UnityEngine;

namespace Beam;

public class LocalizedObjective : ObjectiveDecoratorBase
{
	[SerializeField]
	private LocalizedText _descriptionText = new LocalizedText();

	public LocalizedText DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			_descriptionText = value;
		}
	}

	public override string Description => DescriptionText.ToString();

	public LocalizedObjective(ObjectiveBase objective)
		: base(objective)
	{
	}
}
