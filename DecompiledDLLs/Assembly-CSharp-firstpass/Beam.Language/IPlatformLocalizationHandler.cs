namespace Beam.Language;

public interface IPlatformLocalizationHandler
{
	string SubstituteControllerGlyphs(string text, int playerId, params int[] actions);
}
