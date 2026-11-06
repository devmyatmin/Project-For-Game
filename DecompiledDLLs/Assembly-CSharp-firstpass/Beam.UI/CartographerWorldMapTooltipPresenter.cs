using System;
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
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	public void Show(IWorldMapSlot mapsSlot, PointerEventData eventData)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Show(mapsSlot, Vector2.op_Implicit(eventData.pointerCurrentRaycast.screenPosition));
	}

	public void Show(IWorldMapSlot mapSlot, Vector3 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_view.SetPosition(Vector2.op_Implicit(screenPosition));
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
		LeanTween.cancel(((Component)this).gameObject);
		_view.CanvasGroup.Alpha = 0f;
		LeanTween.value(((Component)this).gameObject, 0f, 1f, 0.7f).setOnComplete((Action)ShowTooltip).setIgnoreTimeScale(true);
	}

	public void Hide()
	{
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_OnUpdate, _view.CanvasGroup.Alpha, 0f, 0.2f).setOnComplete((Action)Fade_OnComplete).setIgnoreTimeScale(true);
	}

	private void ShowTooltip()
	{
		_view.Show();
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_OnUpdate, 0f, 1f, 0.2f).setIgnoreTimeScale(true);
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
