using System;
using Beam.Developer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class UIFallthrough : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public event Action FellThrough;

	protected virtual void OnFellThrough()
	{
		FellThrough?.Invoke();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		TestingConsole.Close();
		OnFellThrough();
	}
}
