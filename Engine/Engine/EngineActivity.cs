using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Engine
{
	[Activity(Label = "生存战争2.2插件版",LaunchMode =LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	//[Activity(Label = "SurvivalCraft2", LaunchMode = LaunchMode.SingleTask, Icon = "@mipmap/icon", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]

	[IntentFilter(new string[] { "android.intent.action.VIEW" }, DataScheme = "com.candy.survivalcraft", Categories = new string[] { "android.intent.category.DEFAULT", "android.intent.category.BROWSABLE" })]

	public class EngineActivity : Activity
	{
		internal static EngineActivity m_activity;

		public event Action Paused;

		public event Action Resumed;

		public event Action Destroyed;

		public event Action<Intent> NewIntent;

		public static string basePath = "";
		public static string configPath = "";
		public EngineActivity()
		{
			m_activity = this;
		}
	
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			while (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
			{
				RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 0);
			}
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			VolumeControlStream = Android.Media.Stream.Music;
			RequestedOrientation = ScreenOrientation.UserLandscape;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			string[] flist= Assets.List("");
			basePath = new StreamReader( Assets.Open("apppath.txt")).ReadToEnd();
			configPath =this.GetExternalFilesDir("").AbsolutePath;
			foreach (string dll in flist) {
				if (dll.EndsWith(".dll")) {
					MemoryStream memoryStream = new MemoryStream();
					Assets.Open(dll).CopyTo(memoryStream);
					AppDomain.CurrentDomain.Load(memoryStream.ToArray()); 
				}
			}
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name == "mscorlib" || assembly.GetName().Name == "Mono.Android")
				{
					continue;
				}
				Type[] types = assembly.GetTypes();
				for (int j = 0; j < types.Length; j++)
				{
					MethodInfo method = types[j].GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(method != null))
					{
						continue;
					}
					List<object> list = new List<object>();
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length == 1)
					{
						if (parameters[0].ParameterType != typeof(string[]))
						{
							continue;
						}
						list.Add(new string[0]);
					}
					else if (parameters.Length > 1)
					{
						continue;
					}
					method.Invoke(null, list.ToArray());
					return;
				}
			}
			throw new Exception("Cannot find static Main method.");			
		}
		protected override void OnPause()
		{
			base.OnPause();
			Paused?.Invoke();
		}
		protected override void OnResume()
		{
			base.OnResume();
			Resumed?.Invoke();
		}
        protected override void OnNewIntent(Intent intent)
        {
			base.OnNewIntent(intent);
			NewIntent?.Invoke(intent);
        }
        protected override void OnDestroy()
		{
			try
			{
				base.OnDestroy();
				Destroyed?.Invoke();
			}
			finally
			{
				Thread.Sleep(250);
				System.Environment.Exit(0);
			}
		}
	}
}
