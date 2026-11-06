namespace Beam;

public interface IDisplayableStorable : IStorable
{
	string GetDisplayName();

	string GetDescription();
}
