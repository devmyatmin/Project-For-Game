namespace Beam;

public interface IAnimatablePrimary
{
	AnimationType Idle { get; }

	AnimationType Primary { get; }

	AnimationType PrimaryOnObject { get; }

	bool ValidatePrimary(IBase obj);
}
