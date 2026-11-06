using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class SlackWebhook
{
	private const string HOOK_URL = "https://hooks.slack.com/services/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";

	private static readonly HttpClient _httpClient = new HttpClient
	{
		Timeout = TimeSpan.FromMilliseconds(3000.0)
	};

	public static async Task Post(SlackMessage msg)
	{
		try
		{
			StringContent content = new StringContent(JsonUtility.ToJson((object)msg), Encoding.UTF8, "application/json");
			using HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync("https://hooks.slack.com/services/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", content);
			httpResponseMessage.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}
}
