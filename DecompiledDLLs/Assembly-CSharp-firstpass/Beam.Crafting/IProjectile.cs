namespace Beam.Crafting;

public interface IProjectile
{
	ProjectileMovement ProjectileMovement { get; }

	BaseObjectProxy Proxy { get; }

	float HealthPoints { get; set; }

	void Pierce(ProjectileHit args);

	void LocalFaunaPierce();

	void Reset();
}
