using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu]
public class CreditsAsset : ScriptableObject
{
	[SerializeField]
	private TextAsset _creditsTextAsset;

	[SerializeField]
	private TextAsset _copyrightTextAsset;

	[SerializeField]
	[TextArea(5, 500)]
	private string _copyrightText;

	[SerializeField]
	[TextArea(5, 500)]
	private string _creditsText;

	public string CopyrightText => _copyrightText;

	public string CreditsText => _creditsText;

	private void ParseMarkdown()
	{
		if (Object.op_Implicit((Object)(object)_copyrightTextAsset))
		{
			_copyrightText = ConvertMarkdownToTextMeshProTags(_copyrightTextAsset.text);
		}
		if (Object.op_Implicit((Object)(object)_creditsTextAsset))
		{
			_creditsText = ConvertMarkdownToTextMeshProTags(_creditsTextAsset.text);
			_creditsText = FormatCredits(_creditsText);
		}
	}

	private static string ConvertMarkdownToTextMeshProTags(string markdown)
	{
		markdown = Regex.Replace(markdown, "^> (.+)", "<style=blockquote>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "^# (.+)$", "<style=h1>$1></style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "^## (.+)$", "<style=h2>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "^### (.+)$", "<style=h3>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "^#### (.+)$", "<style=h4>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "\\*\\*(.+?)\\*\\*", "<b>$1</b>");
		markdown = Regex.Replace(markdown, "\\*(.+?)\\*", "<i>$1</i>");
		markdown = Regex.Replace(markdown, "~~(.+?)~~", "<s>$1</s>");
		markdown = Regex.Replace(markdown, "^[-*] (.+)$", "<style=list-style>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "^[\\d]+[.)] (.+)$", "<style=list-style>$1</style>", RegexOptions.Multiline);
		markdown = Regex.Replace(markdown, "\\[(.+?)\\]\\((.+?)\\)", "<link=$2>$1</link>");
		markdown = Regex.Replace(markdown, "`(.+?)`", "<#ff0000>$1</color>");
		return markdown;
	}

	public static string FormatCredits(string tmpMarkup)
	{
		bool pingPongJustify = false;
		return Regex.Replace(tmpMarkup, "^<style=h2>(.*</style>)", (Match match) => $"<style=h2-align-horizontal-{PingPongHorizontalAlign()}>{match.Groups[1]}", RegexOptions.Multiline);
		string PingPongHorizontalAlign()
		{
			pingPongJustify = !pingPongJustify;
			if (!pingPongJustify)
			{
				return "right";
			}
			return "left";
		}
	}
}
