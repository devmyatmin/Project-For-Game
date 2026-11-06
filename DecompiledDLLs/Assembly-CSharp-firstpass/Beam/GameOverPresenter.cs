using System.Collections.Generic;
using System.Linq;
using Beam.AccountServices;
using Beam.Events;
using Beam.Language;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Rewired;
using UnityEngine;

namespace Beam;

public class GameOverPresenter : MonoBehaviour
{
	private IGameOverView _view;

	private IInputProvider _input;

	private void Awake()
	{
		_input = new UnityInputAdapter();
	}

	private void Start()
	{
		_view.Hide();
		EventManager.AddListener<GameOverEvent>(EventManager_GameOverEvent);
	}

	public void Initialize(IGameOverView view)
	{
		_view = view;
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<GameOverEvent>(EventManager_GameOverEvent);
	}

	private void EventManager_GameOverEvent(GameOverEvent e)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<string> causesOfDeath = e.Causes.SelectMany((PlayerDeathEventData eff) => eff.StatusEffects);
		Timing.RunCoroutine(GameOver_Sequence(causesOfDeath));
	}

	private IEnumerator<float> GameOver_Sequence(IEnumerable<string> causesOfDeath)
	{
		yield return Timing.WaitForSeconds(5f);
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: true, 16);
		yield return Singleton<Fader>.Instance.FadeOut_Async(1f);
		string text = Localization.GetLanguageHandler().LocalizeMany(causesOfDeath, ", ");
		if (!string.IsNullOrEmpty(text))
		{
			_view.CauseOfDeathLabel.Text = string_safemethods.Format(Localization.GetLanguageHandler().Localize("GAME_OVER_SCREEN_LABEL_YOU_DIED_FROM_DESC", text));
		}
		else
		{
			_view.CauseOfDeathLabel.Text = Localization.GetLanguageHandler().Localize("GAME_OVER_SCREEN_LABEL_YOU_DIED");
		}
		int daysSurvived = GAccountService.Get().LocalUser.UserStats.SessionStats.DaysSurvived;
		string term = ((daysSurvived == 1) ? "GAME_OVER_SCREEN_DAY" : "GAME_OVER_SCREEN_DAYS");
		string format = Localization.GetLanguageHandler().Localize("GAME_OVER_SCREEN_LABEL_YOU_SURVIVED_DAYS_DESC");
		format = string.Format(format, Localization.GetLanguageHandler().Localize(term, daysSurvived.ToString()));
		_view.DaysSurvivedLabel.Text = format;
		bool permadeath = Options.CustomSettings.Permadeath == EGamePermadeathMode.On && MPExtensions.IsMaster(Game.Mode);
		((Component)_view.PermadeathLabel.transform).gameObject.SetActive(permadeath);
		_view.Show();
		yield return Singleton<Fader>.Instance.FadeIn_Async(1f);
		while (!_input.AnyKey)
		{
			yield return 0f;
		}
		if (permadeath)
		{
			SaveManager.DeleteSaveFile(Game.Mode);
		}
		Object.FindObjectOfType<MainMenuPresenter>()?.QuitToMenu();
	}

	private string FormatEffects(IList<PlayerDeathEventData> deathsEventData)
	{
		string text = string.Empty;
		for (int i = 0; i < deathsEventData.Count; i++)
		{
			PlayerDeathEventData playerDeathEventData = deathsEventData[i];
			for (int j = 0; j < playerDeathEventData.StatusEffects.Count; j++)
			{
				string text2 = playerDeathEventData.StatusEffects[j];
				text = text + text2 + ((j == playerDeathEventData.StatusEffects.Count - 1) ? "." : ", ");
			}
		}
		return text;
	}
}
