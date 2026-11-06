using System.Collections.Generic;
using System.Linq;
using Beam.Developer.UI;
using Beam.Serialization;
using Funlabs;
using Rewired;
using UnityEngine;

namespace Beam.Developer;

public class TestingConsole : Singleton<TestingConsole>
{
	private ITestingConsoleViewAdapter _view;

	private GameObject _selectedPrefab;

	private bool _open;

	private CursorLockMode _cursorLockMode;

	private bool _cursorVisibility;

	private void Awake()
	{
		((Behaviour)this).enabled = false;
	}

	public void Initialize(ITestingConsoleViewAdapter view)
	{
		_view = view;
		_view.CreatePrefabButton.Click += CreatePrefabButton_Click;
		_view.FlyModeButton.Click += FlyModeButton_Click;
		Prefabs.Initialize();
		foreach (PrefabReference item in Prefabs.PrefabReferences.OrderBy((PrefabReference p) => ((Object)p.Prefab).name))
		{
			UPrefabListElement uPrefabListElement = Object.Instantiate<UPrefabListElement>(_view.PrefabListElementPrefab);
			((Component)uPrefabListElement).transform.SetParent(_view.PrefabListContainer.transform, false);
			uPrefabListElement.Text.text = ((Object)item.Prefab).name;
			uPrefabListElement.Prefab = ((Component)item.Prefab).gameObject;
			uPrefabListElement.Click += PrefabElementButton_Click;
		}
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode) && Input.GetKeyDown((KeyCode)47))
		{
			if (!_open)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	private void Show()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_open = true;
		_view.Show();
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: true, 16);
		_cursorLockMode = CursorUtils.LockState;
		_cursorVisibility = CursorUtils.Visible;
		CursorUtils.LockState = (CursorLockMode)2;
		CursorUtils.Visible = true;
	}

	private void Hide()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_open)
		{
			_open = false;
			_view.Hide();
			RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: false, 16);
			RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
			CursorUtils.LockState = _cursorLockMode;
			CursorUtils.Visible = _cursorVisibility;
		}
	}

	public static void Close()
	{
		if (Object.op_Implicit((Object)(object)Singleton<TestingConsole>.Instance) && ((Behaviour)Singleton<TestingConsole>.Instance).enabled)
		{
			Singleton<TestingConsole>.Instance.Hide();
		}
	}

	public void PrefabElementButton_Click(GameObject prefab)
	{
		_selectedPrefab = prefab;
	}

	public void CreatePrefabButton_Click(object sender)
	{
		if ((Object)(object)_selectedPrefab != (Object)null)
		{
			PlayerRegistry.LocalPlayer.DeveloperMode.Create(_selectedPrefab);
		}
	}

	private void FlyModeButton_Click(object sender)
	{
		_view.FlyModeButton.Text = "FLY MODE " + (PlayerRegistry.LocalPlayer.DeveloperMode.InFlyMode ? "OFF" : "ON");
		PlayerRegistry.LocalPlayer.DeveloperMode.ReplicatedToggleGodMode();
	}
}
