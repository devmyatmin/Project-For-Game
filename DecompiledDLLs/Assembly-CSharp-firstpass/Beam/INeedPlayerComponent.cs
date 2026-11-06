namespace Beam;

public interface INeedPlayerComponent
{
	IPlayer Player { get; }

	void SetPlayer(IPlayer player);
}
