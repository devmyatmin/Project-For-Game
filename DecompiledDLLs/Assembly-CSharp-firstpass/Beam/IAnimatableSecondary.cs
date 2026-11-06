namespace Beam;

public interface IAnimatableSecondary
{
	AnimationType SecondaryIdle { get; }

	AnimationType Secondary { get; }

	bool ValidateSecondary(IBase obj);
}
