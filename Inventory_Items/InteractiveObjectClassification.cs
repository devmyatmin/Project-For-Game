namespace Beam;

public enum InteractiveObjectClassification
{
	NONE,
	[Description("ITEM_DISPLAY_CLASSIFICATION_TOOL", "")]
	Tool,
	[Description("ITEM_DISPLAY_CLASSIFICATION_CRAFTABLE", "")]
	Craftable,
	[Description("ITEM_DISPLAY_CLASSIFICATION_MEDICAL", "")]
	Medical,
	[Description("ITEM_DISPLAY_CLASSIFICATION_FOOD", "")]
	Food,
	[Description("ITEM_DISPLAY_CLASSIFICATION_AWESOME", "")]
	Awesome
}
