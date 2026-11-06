using System;
using UnityEngine;

namespace Beam.Terrain;

[Serializable]
public class MapEditorData
{
	public enum EMapSource
	{
		[Description("MAP_SOURCE_LOCAL", "")]
		Local,
		[Description("MAP_SOURCE_COMMUNITY", "")]
		Community
	}

	public enum EMapState
	{
		[Description("MAP_STATUS_CURRENT", "")]
		Current,
		[Description("MAP_STATUS_OLDER", "")]
		Older,
		[Description("MAP_STATUS_UNAVAILABLE", "")]
		Unavailable
	}

	[NonSerialized]
	public bool Valid = true;

	public string Id = Guid.NewGuid().ToString();

	public int VersionNumber;

	public string GameVersionNumber = global::ExtensionMethods.ToPaddedString(BuildInformation.BuildVersion);

	public EMapSource Source;

	public EMapState State;

	public Zone.BiomeType Biome = Zone.BiomeType.ISLAND;

	public string Name = "";

	public string Author = "";

	public string Description = "";

	public string DateCreated = "--";

	public string DateEdited = "--";

	public string DateAdded = "--";

	public byte[] LargePreviewImageBytes;

	[NonSerialized]
	public Texture2D LargePreviewImage;

	[NonSerialized]
	public Texture2D SmallPreviewImage;
}
