using Rewired.Dev;

namespace Beam;

public static class RewiredAction
{
	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Vertical Axis")]
	public const int Move_Vertical = 1;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Move Horizontal Axis")]
	public const int Move_Horizontal = 0;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Look Vertical Axis")]
	public const int Look_Vertical = 3;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Look Horizontal Axis")]
	public const int Look_Horizontal = 2;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Jump")]
	public const int Jump = 4;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Crouch")]
	public const int Crouch = 5;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Sprint")]
	public const int Sprint = 6;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Use Item")]
	public const int Use_Item = 7;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Drag Item")]
	public const int Drag_Item = 8;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Pickup Item")]
	public const int Pickup_Item = 9;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Switch Item")]
	public const int Switch_Item = 29;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Drop Item")]
	public const int Drop_Item = 10;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Watch")]
	public const int Watch = 11;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Watch Cycle Forward")]
	public const int Watch_Cycle_Forward = 34;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Watch Cycle Backward")]
	public const int Watch_Cycle_Backward = 40;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Torch")]
	public const int Torch = 15;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Inventory Menu")]
	public const int Inventory_Menu = 12;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Crafting Menu")]
	public const int Crafting_Menu = 13;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Free Hands")]
	public const int Free_Hands = 17;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Radial Menu Horizontal Axis")]
	public const int Radial_Menu_Horizontal = 24;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Radial Menu Vertical Axis")]
	public const int Radial_Menu_Vertical = 25;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Radial Menu Navigation Axis")]
	public const int Radial_Menu_Navigation = 63;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_1")]
	public const int Hotkey_1 = 67;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_2")]
	public const int Hotkey_2 = 69;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_3")]
	public const int Hotkey_3 = 70;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_4")]
	public const int Hotkey_4 = 71;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_5")]
	public const int Hotkey_5 = 72;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_6")]
	public const int Hotkey_6 = 73;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_7")]
	public const int Hotkey_7 = 74;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_8")]
	public const int Hotkey_8 = 75;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_9")]
	public const int Hotkey_9 = 76;

	[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Hotkey_0")]
	public const int Hotkey_0 = 77;

	[ActionIdFieldInfo(categoryName = "Crafting Menu", friendlyName = "Crafting Menu Pin")]
	public const int Crafting_Menu_Pin = 78;

	[ActionIdFieldInfo(categoryName = "Crafting Menu", friendlyName = "Crafting Navigate Left")]
	public const int Crafting_Navigate_Left = 35;

	[ActionIdFieldInfo(categoryName = "Crafting Menu", friendlyName = "Crafting Navigate Right")]
	public const int Crafting_Navigate_Right = 36;

	[ActionIdFieldInfo(categoryName = "Placing", friendlyName = "Rotate Object Axis")]
	public const int Rotate_Object = 16;

	[ActionIdFieldInfo(categoryName = "Placing", friendlyName = "Confirm Placement")]
	public const int Placing_Confirm = 81;

	[ActionIdFieldInfo(categoryName = "Placing", friendlyName = "Cancel Placement")]
	public const int Placing_Cancel = 82;

	[ActionIdFieldInfo(categoryName = "Fire Lighting", friendlyName = "Fire Lighting Action 1")]
	public const int Fire_Lighting_Action_1 = 79;

	[ActionIdFieldInfo(categoryName = "Fire Lighting", friendlyName = "Fire Lighting Action 2")]
	public const int Fire_Lighting_Action_2 = 80;

	[ActionIdFieldInfo(categoryName = "Vehicle", friendlyName = "Vehicle Ascend")]
	public const int Vehicle_Ascend = 57;

	[ActionIdFieldInfo(categoryName = "Vehicle", friendlyName = "Vehicle Descend")]
	public const int Vehicle_Descend = 58;

	[ActionIdFieldInfo(categoryName = "Vehicle", friendlyName = "Vehicle Sail Axis")]
	public const int Vehicle_Sail = 64;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Move X Axis")]
	public const int Editor_Move_X = 43;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Move Y Axis")]
	public const int Editor_Move_Y = 45;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Move Z Axis")]
	public const int Editor_Move_Z = 62;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Delete")]
	public const int Editor_Delete = 48;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Clone")]
	public const int Editor_Clone = 49;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Rotate")]
	public const int Editor_Rotate = 50;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Position")]
	public const int Editor_Position = 51;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Sprint")]
	public const int Editor_Sprint = 52;

	[ActionIdFieldInfo(categoryName = "Map Editor", friendlyName = "Editor Focus")]
	public const int Editor_Focus = 53;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Confirm")]
	public const int UI_Confirm = 30;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Cancel")]
	public const int UI_Cancel = 31;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Delete")]
	public const int UI_Delete = 41;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Escape")]
	public const int UI_Escape = 39;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Horizontal Axis")]
	public const int UI_Horizontal = 32;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Vertical Axis")]
	public const int UI_Vertical = 33;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UI Secondary Button")]
	public const int UI_Secondary = 66;
}
