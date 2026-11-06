using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UnityEngine;

namespace Beam;

public static class NetworkConnectivity
{
	private static readonly HttpClient _httpClient = new HttpClient
	{
		Timeout = TimeSpan.FromMilliseconds(3000.0)
	};

	public static async Task<int> CheckHttpConnectivityAsync()
	{
		Debug.Log((object)"Checking internet connectivity: Requesting https://clients3.google.com/generate_204 ...");
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			using HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("https://clients3.google.com/generate_204");
			Debug.Log((object)$"Checking internet connectivity: Response received: {httpResponseMessage.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");
			return (!httpResponseMessage.IsSuccessStatusCode) ? 1 : 0;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return 1;
		}
	}

	public static async Task<int> CheckPingConnectivityAsync()
	{
		Debug.Log((object)"Checking internet connectivity: Pinging 8.8.8.8 ...");
		using Ping ping = new Ping();
		try
		{
			PingReply pingReply = await ping.SendPingAsync("8.8.8.8", 3000);
			if (pingReply == null)
			{
				Debug.LogError((object)"Checking internet connectivity: Ping reply null.");
				return 1;
			}
			Debug.Log((object)$"Checking internet connectivity: Response received: {pingReply.Status} in {pingReply.RoundtripTime}ms");
			return (pingReply.Status != IPStatus.Success) ? 1 : 0;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return 1;
		}
	}
}
