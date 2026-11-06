using System.Collections;
using Beam.Events;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class OnlinePlayerNameWidget : MonoBehaviour, INeedPlayerComponent
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private Vector3 _labelOffset;

	[SerializeField]
	private float _showDistance = 50f;

	private IPlayer _player;

	private IPlayer _otherPlayer;

	public ILabelViewAdapter Label => _label;

	protected float SqrShowDist => _showDistance * _showDistance;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Awake()
	{
		EventManager.AddListener<PlayerConnectedEvent>(OnPlayerConnected);
		EventManager.AddListener<PlayerDisconnectedEvent>(OnPlayerDisconnected);
		EventManager.AddListener<ServerJoinInProgressEnded>(OnServerJoinInProgressEnded);
	}

	private IEnumerator Start()
	{
		while (LevelLoader.IsLoadingScene())
		{
			yield return null;
		}
		InitOtherPlayer();
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerConnectedEvent>(OnPlayerConnected);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnPlayerDisconnected);
		EventManager.RemoveListener<ServerJoinInProgressEnded>(OnServerJoinInProgressEnded);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void OnPlayerConnected(PlayerConnectedEvent ev)
	{
		InitOtherPlayer();
	}

	private void OnPlayerDisconnected(PlayerDisconnectedEvent ev)
	{
		if (ev.PlayerId != _player.Id)
		{
			_otherPlayer = null;
		}
	}

	private void OnServerJoinInProgressEnded(ServerJoinInProgressEnded ev)
	{
		if (ev.Success)
		{
			InitOtherPlayer();
		}
	}

	private void InitOtherPlayer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		int otherPlayerId = (MPExtensions.IsServer(Game.Mode) ? 1 : 0);
		GetOtherPlayer(otherPlayerId);
	}

	private void GetOtherPlayer(int otherPlayerId)
	{
		if (PlayerRegistry.AllPlayers.Count >= 2 && otherPlayerId != _player.Id)
		{
			_otherPlayer = PlayerUtilities.GetPlayerFromId(otherPlayerId);
			if (BeamTeam.IsValid(_otherPlayer))
			{
				SetPlayerName(_otherPlayer.Peer.OnlineName);
			}
		}
	}

	public void SetPlayerName(string username)
	{
		Label.Text = username;
	}

	private void SetLabelActive(bool active)
	{
		if (((Component)Label.transform).gameObject.activeSelf != active)
		{
			((Component)Label.transform).gameObject.SetActive(active);
		}
	}

	private void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected Obj, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(_player) || BeamTeam.IsNullOrDestroyed(_otherPlayer))
		{
			SetLabelActive(active: false);
			return;
		}
		Vector3 position = ((Component)_otherPlayer.Movement).transform.position;
		Vector3 val = position - ((Component)_player.PlayerCamera.Camera).transform.position;
		float num = Vector3.Dot(((Component)_player.PlayerCamera.Camera).transform.forward, val.normalized);
		if (val.sqrMagnitude < SqrShowDist && num > 0.1f)
		{
			SetLabelActive(active: true);
			Vector3 val2 = _player.PlayerCamera.Camera.WorldToScreenPoint(position + _labelOffset);
			Vector2 anchoredPosition = default;
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Label.transform.parent, Vector2.op_Implicit(val2), _player.PlayerUI.UiCamera, ref anchoredPosition);
			((RectTransform)Label.transform).anchoredPosition = anchoredPosition;
		}
		else
		{
			SetLabelActive(active: false);
		}
	}
}
