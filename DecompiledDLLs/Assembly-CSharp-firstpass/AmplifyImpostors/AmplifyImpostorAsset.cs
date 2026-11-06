using System.Collections.Generic;
using UnityEngine;

namespace AmplifyImpostors;

[CreateAssetMenu(fileName = "New Impostor", order = 85)]
public class AmplifyImpostorAsset : ScriptableObject
{
	[SerializeField]
	public Material Material;

	[SerializeField]
	public Mesh Mesh;

	[HideInInspector]
	[SerializeField]
	public int Version;

	[SerializeField]
	public ImpostorType ImpostorType = ImpostorType.Octahedron;

	[HideInInspector]
	[SerializeField]
	public bool LockedSizes = true;

	[HideInInspector]
	[SerializeField]
	public int SelectedSize = 2048;

	[SerializeField]
	public Vector2 TexSize = new Vector2(2048f, 2048f);

	[HideInInspector]
	[SerializeField]
	public bool DecoupleAxisFrames;

	[SerializeField]
	[Range(1f, 32f)]
	public int HorizontalFrames = 16;

	[SerializeField]
	[Range(1f, 33f)]
	public int VerticalFrames = 16;

	[SerializeField]
	[Range(0f, 64f)]
	public int PixelPadding = 32;

	[SerializeField]
	[Range(4f, 16f)]
	public int MaxVertices = 8;

	[SerializeField]
	[Range(0f, 0.2f)]
	public float Tolerance = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	public float NormalScale = 0.01f;

	[SerializeField]
	public Vector2[] ShapePoints = new Vector2[8]
	{
		new Vector2(0.15f, 0f),
		new Vector2(0.85f, 0f),
		new Vector2(1f, 0.15f),
		new Vector2(1f, 0.85f),
		new Vector2(0.85f, 1f),
		new Vector2(0.15f, 1f),
		new Vector2(0f, 0.85f),
		new Vector2(0f, 0.15f)
	};

	[SerializeField]
	public AmplifyImpostorBakePreset Preset;

	[SerializeField]
	public List<TextureOutput> OverrideOutput = new List<TextureOutput>();

	public AmplifyImpostorAsset()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
	}
}
