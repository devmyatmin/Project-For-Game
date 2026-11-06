using System.Collections;
using Beam.Events;
using Beam.UI.Crafting;
using Funlabs;
using Photon.Bolt;
using UdpKit;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class PermissionPromptPresenter : MonoBehaviour
{
	[SerializeField]
	private TMPLabelViewAdapter _title;

	[SerializeField]
	private TMPLabelViewAdapter _message;

	[SerializeField]
	private Image _timer;

	[SerializeField]
	private Button _button;

	private bool _reduceTime;

	private float _acceptTimeout = 8f;

	private float _helpTimer = 10f;

	private Coroutine _hideCoroutine;

	public UdpEndPoint endpoint;

	public IProtocolToken joinToken;

	private string _currentClientId;

	private bool DialogueMapState;

	private void Start()
	{
		EventManager.AddListener<EndGameSequenceEvent>(AbortConnection);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<EndGameSequenceEvent>(AbortConnection);
	}

	public void Update()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		if (_reduceTime)
		{
			_timer.fillAmount = (_helpTimer - Time.time) / _acceptTimeout;
			if (Time.time >= _helpTimer)
			{
				_reduceTime = false;
			}
		}
		if ((int)CursorUtils.LockState == 1)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: false, RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input));
			RewiredUtilities.SetCursorEnabled(PlayerRegistry.LocalPlayer.Input, enabled: true);
			PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Clear);
		}
	}

	public void Show(string nameClient, string requestClientId = "")
	{
		_currentClientId = requestClientId;
		_timer.fillAmount = 1f;
		_reduceTime = true;
		_helpTimer = Time.time + _acceptTimeout;
		_title.Text = "PERMISSION_PROMPT_TITLE";
		_message.Text = nameClient + " PERMISSION_PROMPT_MESSAGE";
		((Component)this).gameObject.SetActive(true);
		DialogueMapState = RewiredUtilities.isMapEnabled(PlayerRegistry.LocalPlayer.Input, 16);
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: false, RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input));
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: true, 16);
		RewiredUtilities.SetCursorEnabled(PlayerRegistry.LocalPlayer.Input, enabled: true);
		((Selectable)_button).Select();
		PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Refresh);
		_hideCoroutine = ((MonoBehaviour)this).StartCoroutine(hideAfterSeconds());
	}

	private IEnumerator hideAfterSeconds()
	{
		yield return (object)new WaitForSeconds(_acceptTimeout);
		Hide(admited: false);
	}

	public void Hide(bool admited)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (admited)
		{
			((MonoBehaviour)this).StopCoroutine(_hideCoroutine);
			BoltNetwork.Accept(endpoint, (IProtocolToken)(object)new MultiplayerTokens.Accept
			{
				ServerGameState = Game.State
			});
			MultiplayerMng.ListenForSuccesfulClientConnection();
			MultiplayerMng.RemoveClientOnHold(_currentClientId);
		}
		else
		{
			BoltNetwork.Refuse(endpoint);
			MultiplayerMng.UpdateClientOnHold(_currentClientId);
		}
		((Component)this).gameObject.SetActive(false);
		_reduceTime = false;
		if (RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input) == 0)
		{
			RewiredUtilities.SetCursorEnabled(PlayerRegistry.LocalPlayer.Input, enabled: false);
		}
		else if (RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input) == 4)
		{
			PlayerRegistry.LocalPlayer.PlayerUI.GetController<CrafterMenuPresenter>().GetSubcategoryButton().Select();
		}
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, DialogueMapState, 16);
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: true, RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input));
		PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Refresh);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		if (RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input) == 0)
		{
			RewiredUtilities.SetCursorEnabled(PlayerRegistry.LocalPlayer.Input, enabled: false);
		}
		else if (RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input) == 4)
		{
			PlayerRegistry.LocalPlayer.PlayerUI.GetController<CrafterMenuPresenter>().GetSubcategoryButton().Select();
		}
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, DialogueMapState, 16);
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: true, RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input));
		PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Refresh);
	}

	public void AbortConnection(EndGameSequenceEvent e)
	{
		Hide(admited: false);
	}
}
