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
		int otherPlayerId = (Game.Mode.IsServer() ? 1 : 0);
		GetOtherPlayer(otherPlayerId);
	}

	private void GetOtherPlayer(int otherPlayerId)
	{
		if (PlayerRegistry.AllPlayers.Count >= 2 && otherPlayerId != _player.Id)
		{
			_otherPlayer = PlayerUtilities.GetPlayerFromId(otherPlayerId);
			if (_otherPlayer.IsValid())
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
		if (Label.transform.gameObject.activeSelf != active)
		{
			Label.transform.gameObject.SetActive(active);
		}
	}

	private void Update()
	{
		if (_player.IsNullOrDestroyed() || _otherPlayer.IsNullOrDestroyed())
		{
			SetLabelActive(active: false);
			return;
		}
		Vector3 position = _otherPlayer.Movement.transform.position;
		Vector3 vector = position - _player.PlayerCamera.Camera.transform.position;
		float num = Vector3.Dot(_player.PlayerCamera.Camera.transform.forward, vector.normalized);
		if (vector.sqrMagnitude < SqrShowDist && num > 0.1f)
		{
			SetLabelActive(active: true);
			Vector3 vector2 = _player.PlayerCamera.Camera.WorldToScreenPoint(position + _labelOffset);
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Label.transform.parent, vector2, _player.PlayerUI.UiCamera, out var localPoint);
			((RectTransform)Label.transform).anchoredPosition = localPoint;
		}
		else
		{
			SetLabelActive(active: false);
		}
	}
}
