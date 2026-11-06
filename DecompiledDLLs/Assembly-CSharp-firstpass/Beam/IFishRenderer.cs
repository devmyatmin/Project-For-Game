namespace Beam;

public interface IFishRenderer
{
	RenderableWithLods FishTemplate { get; }

	FishRendererBase FishRenderer { get; }
}
