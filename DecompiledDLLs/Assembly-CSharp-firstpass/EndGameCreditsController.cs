using Beam;
using Beam.Events;
using Beam.UI;
using UnityEngine;

public class EndGameCreditsController : MonoBehaviour
{
	[SerializeField]
	private EndGameCreditsView _view;

	[SerializeField]
	private CreditsAsset _creditsAsset;

	private void Awake()
	{
		_view.SetCreditsText(_creditsAsset.CreditsText);
		_view.SetCopyrightText(_creditsAsset.CopyrightText);
		_view.Finished += View_Finished;
	}

	private void OnDestroy()
	{
		_view.Finished -= View_Finished;
	}

	public void Show()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Singleton<Fader>.Instance.FadeIn(0f);
		PlayTrackInstantly(TrackType.MainMenu);
		_view.Show();
	}

	private void View_Finished()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, () =>
		{
			_view.Hide();
			StopTrack(TrackType.MainMenu);
			Object.FindObjectOfType<MainMenuPresenter>()?.QuitToMenu();
		});
	}

	public void PlayTrackInstantly(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Play_Instantly));
	}

	public void StopTrack(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Stop));
	}
}
