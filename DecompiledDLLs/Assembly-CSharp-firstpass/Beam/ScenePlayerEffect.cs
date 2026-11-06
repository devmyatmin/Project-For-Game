namespace Beam;

public abstract class ScenePlayerEffect : SaveableBehaviour, IPlayerEffect
{
	public abstract string Name { get; }
}
