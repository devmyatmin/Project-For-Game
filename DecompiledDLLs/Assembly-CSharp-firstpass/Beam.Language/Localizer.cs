using System;
using Beam.UI;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.Language;

[RequireComponent(typeof(TMPLabelViewAdapter))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class Localizer : MonoBehaviour
{
	[Header("Localization Term")]
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _term = "NONE";

	[Header("{PLATFORM} Term")]
	[SerializeField]
	[RewiredActionDropdown]
	private int _rewiredAction = -1;

	private TMPLabelViewAdapter _label;

	private TextMeshProUGUI _text;

	private bool _dynamic;

	public string Term => _term;

	public int RewiredAction => _rewiredAction;

	private void Awake()
	{
		_dynamic = _term.Equals("DYNAMIC");
		ScenePlatformLocalization.Instance.AddLocalizer(this);
	}

	private void OnDestroy()
	{
		ScenePlatformLocalization.Instance?.RemoveLocalizer(this);
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		if ((Object)(object)_label == (Object)null)
		{
			_label = ((Component)this).GetComponent<TMPLabelViewAdapter>();
			_label.TextChanged += Label_TextChanged;
		}
		if ((Object)(object)_text == (Object)null)
		{
			_text = ((Component)this).GetComponent<TextMeshProUGUI>();
		}
		Label_TextChanged(_label, EventArgs.Empty);
	}

	private void Label_TextChanged(ILabelViewAdapter sender, EventArgs args)
	{
		if (_dynamic)
		{
			_term = sender.Text;
		}
		((TMP_Text)_text).text = Format(_term);
	}

	private string Format(string term)
	{
		return LocalizePlatform(Localize(term));
	}

	private string Localize(string term)
	{
		if (string.IsNullOrWhiteSpace(term))
		{
			return string.Empty;
		}
		return Localization.GetLanguageHandler().LocalizeMany(term, ' ');
	}

	private string LocalizePlatform(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		if (_rewiredAction != -1)
		{
			Player playerFromCanvas = RewiredUtilities.GetPlayerFromCanvas(((Graphic)_text).canvas);
			return PlatformLocalization.GetHandler().SubstituteControllerGlyphs(text, playerFromCanvas.id, _rewiredAction);
		}
		return text;
	}
}
