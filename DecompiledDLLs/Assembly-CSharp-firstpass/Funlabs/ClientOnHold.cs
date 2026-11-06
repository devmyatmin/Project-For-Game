using UnityEngine;

namespace Funlabs;

public class ClientOnHold
{
	private string _requestClientId;

	private float _lastRequestTime;

	private int _requests;

	public string RequestClientId
	{
		get
		{
			return _requestClientId;
		}
		set
		{
			_requestClientId = value;
		}
	}

	public float LastRequestTime
	{
		get
		{
			return _lastRequestTime;
		}
		set
		{
			_lastRequestTime = value;
		}
	}

	public int Requests
	{
		get
		{
			return _requests;
		}
		set
		{
			_requests = value;
		}
	}

	public ClientOnHold(string requestClientId)
	{
		RequestClientId = requestClientId;
		LastRequestTime = Time.unscaledTime;
		Requests = 1;
	}
}
