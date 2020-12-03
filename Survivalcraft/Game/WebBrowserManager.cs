using Android.Content;
using Android.Net;
using Engine;
using System;

namespace Game
{
	public static class WebBrowserManager
	{
		public static void LaunchBrowser(string url)
		{
			AnalyticsManager.LogEvent("[WebBrowserManager] Launching browser", new AnalyticsParameter("Url", url));
			if (!url.Contains("://"))
			{
				url = "http://" + url;
			}
			try
			{
				Android.Net.Uri uri = Android.Net.Uri.Parse(url);
				Intent intent = new Intent("android.intent.action.VIEW", uri);
				Window.Activity.StartActivity(intent);
			}
			catch (Exception ex)
			{
				Log.Error($"Error launching web browser with URL \"{url}\". Reason: {ex.Message}");
			}
		}
	}
}
