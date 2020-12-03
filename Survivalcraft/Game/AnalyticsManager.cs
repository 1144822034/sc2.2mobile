using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Game
{
	public static class AnalyticsManager
	{
		public static double LastSendTime = double.MinValue;

		public static string AnalyticsVersion => string.Empty;

		public static void Initialize()
		{
		}

		public static void LogError(string message, Exception error)
		{
			try
			{
				double realTime = Time.RealTime;
				if (!(realTime - LastSendTime < 15.0))
				{
					LastSendTime = realTime;
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("Platform", VersionsManager.Platform.ToString());
					dictionary.Add("BuildConfiguration", VersionsManager.BuildConfiguration.ToString());
					dictionary.Add("DeviceModel", DeviceManager.DeviceModel);
					dictionary.Add("OSVersion", DeviceManager.OperatingSystemVersion);
					dictionary.Add("Is64bit", (Marshal.SizeOf<IntPtr>() == 8).ToString());
					dictionary.Add("FreeSpace", (Storage.FreeSpace / 1024 / 1024).ToString() + "MB");
					dictionary.Add("TotalAvailableMemory", (Utilities.GetTotalAvailableMemory() / 1024).ToString() + "kB");
					dictionary.Add("RealTime", Time.RealTime.ToString("0.000") + "s");
					dictionary.Add("WindowSize", Window.Size.ToString());
					dictionary.Add("FullErrorMessage", ExceptionManager.MakeFullErrorMessage(message, error));
					dictionary.Add("ExceptionType", error.GetType().ToString());
					dictionary.Add("ExceptionStackTrace", AbbreviateStackTrace(error.StackTrace));
					MemoryStream memoryStream = new MemoryStream();
					DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true);
					BinaryWriter binaryWriter = new BinaryWriter(deflateStream);
					binaryWriter.Write(3735928559u);
					binaryWriter.Write((byte)dictionary.Count);
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						binaryWriter.Write(item.Key);
						binaryWriter.Write(item.Value);
					}
					deflateStream.Dispose();
					memoryStream.Position = 0L;
					WebManager.Post(string.Format("{0}:{1}/{2}/{3}/{4}/{5}", "http://quality.kaalus.com", 30099, 1, "Survivalcraft", VersionsManager.Version, "Error"), null, null, memoryStream, null, null, null);
				}
			}
			catch
			{
			}
		}

		public static void LogEvent(string eventName, params AnalyticsParameter[] parameters)
		{
		}

		public static string AbbreviateStackTrace(string stackTrace)
		{
			stackTrace = stackTrace.Replace("System.Collections.Generic.", "");
			stackTrace = stackTrace.Replace("System.Collections.", "");
			stackTrace = stackTrace.Replace("System.IO.", "");
			stackTrace = stackTrace.Replace("Engine.Audio.", "");
			stackTrace = stackTrace.Replace("Engine.Input.", "");
			stackTrace = stackTrace.Replace("Engine.Graphics.", "");
			stackTrace = stackTrace.Replace("Engine.Media.", "");
			stackTrace = stackTrace.Replace("Engine.Content.", "");
			stackTrace = stackTrace.Replace("Engine.Serialization.", "");
			stackTrace = stackTrace.Replace("Engine.", "");
			stackTrace = stackTrace.Replace("C:\\Sources\\Engine\\Engine\\", "");
			stackTrace = stackTrace.Replace("C:\\Sources\\Survivalcraft\\Game\\", "");
			stackTrace = stackTrace.Replace(":line ", ":");
			return stackTrace;
		}
	}
}
