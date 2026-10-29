using Beam.Language;
using Beam.UI.MapEditor;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class CartographerWorldMapTooltipPresenter : MonoBehaviour
{
	private ICartographerWorldMapTooltipViewAdapter _view;

	private void Start()
	{
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	public void Initialize(ICartographerWorldMapTooltipViewAdapter view)
	{
		_view = view;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 1)
		{
			BeamTeam.Destroy(base.gameObject);
		}
	}

	public void Show(IWorldMapSlot mapsSlot, PointerEventData eventData)
	{
		Show(mapsSlot, eventData.pointerCurrentRaycast.screenPosition);
	}

	public void Show(IWorldMapSlot mapSlot, Vector3 screenPosition)
	{
		_view.SetPosition(screenPosition);
		_view.NameLabel.Text = mapSlot.Map.EditorData.Name;
		_view.AuthorLabel.Text = mapSlot.Map.EditorData.Author;
		_view.VersionNumberLabel.Text = mapSlot.Map.EditorData.VersionNumber.ToString("F1");
		_view.SetSource(mapSlot.Map.EditorData.Source);
		_view.SetState(mapSlot.Map.EditorData.State);
		if (mapSlot.Map.EditorData.Id.Contains("INTERNAL_PROCEDURAL") || mapSlot.Map.EditorData.Id.Contains("INTERNAL_MISSION"))
		{
			_view.DescriptionLabel.Text = (mapSlot.Map.EditorData.Description.Equals("A mysterious location...") ? Localization.GetLanguageHandler().Localize("MAP_DESC_MISSION") : Localization.GetLanguageHandler().Localize(mapSlot.Map.EditorData.Description));
		}
		else
		{
			_view.DescriptionLabel.Text = mapSlot.Map.EditorData.Description;
		}
		LeanTween.cancel(base.gameObject);
		_view.CanvasGroup.Alpha = 0f;
		LeanTween.value(base.gameObject, 0f, 1f, 0.7f).setOnComplete(ShowTooltip).setIgnoreTimeScale(useUnScaledTime: true);
	}

	public void Hide()
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, Fade_OnUpdate, _view.CanvasGroup.Alpha, 0f, 0.2f).setOnComplete(Fade_OnComplete).setIgnoreTimeScale(useUnScaledTime: true);
	}

	private void ShowTooltip()
	{
		_view.Show();
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, Fade_OnUpdate, 0f, 1f, 0.2f).setIgnoreTimeScale(useUnScaledTime: true);
	}

	private void Fade_OnUpdate(float value)
	{
		_view.CanvasGroup.Alpha = value;
	}

	private void Fade_OnComplete()
	{
		_view.Hide();
	}
}
