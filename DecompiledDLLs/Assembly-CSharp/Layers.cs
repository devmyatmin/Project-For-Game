using UnityEngine;

public static class Layers
{
	public static readonly int PLAYER = LayerMask.NameToLayer("Player");

	public static readonly int WATER = LayerMask.NameToLayer("Water");

	public static readonly int TERRAIN = LayerMask.NameToLayer("Terrain");

	public static readonly int TERRAIN_OBJECTS = LayerMask.NameToLayer("TerrainObjects");

	public static readonly int TERRAIN_DETAILS = LayerMask.NameToLayer("TerrainDetails");

	public static readonly int PROJECTILES = LayerMask.NameToLayer("Projectiles");

	public static readonly int PROJECTILE_HITBOXES = LayerMask.NameToLayer("ProjectileHitboxes");

	public static readonly int IGNORE_RAYCAST = LayerMask.NameToLayer("Ignore Raycast");

	public static readonly int INTERACTIVE_OBJECTS = LayerMask.NameToLayer("InteractiveObjects");

	public static readonly int CONSTRUCTIONS = LayerMask.NameToLayer("Constructions");

	public static readonly int CONSTRUCTIONS_SMALL = LayerMask.NameToLayer("Constructions_Small");

	public static readonly int CONSTRUCTIONS_RAFTS = LayerMask.NameToLayer("ConstructionsRafts");

	public static readonly int CONNECTOR_PANEL = LayerMask.NameToLayer("Connector_Panel");

	public static readonly int CONNECTOR_FOUNDATION = LayerMask.NameToLayer("Connector_Foundation");

	public static readonly int INTERACTIVE_TREES = LayerMask.NameToLayer("InteractiveTrees");

	public static readonly int PARTICLES = LayerMask.NameToLayer("Particles");

	public static readonly int UI_MAIN_MENU = LayerMask.NameToLayer("MainMenuUI");
}
