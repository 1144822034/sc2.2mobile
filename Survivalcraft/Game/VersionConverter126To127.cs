using Engine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public class VersionConverter126To127 : VersionConverter
	{
		public override string SourceVersion => "1.26";

		public override string TargetVersion => "1.27";

		public override void ConvertProjectXml(XElement projectNode)
		{
			XmlUtils.SetAttributeValue(projectNode, "Version", TargetVersion);
			ConvertTypesToEngine(projectNode);
		}

		public override void ConvertWorld(string directoryName)
		{
			string path = Storage.CombinePaths(directoryName, "Project.xml");
			XElement xElement;
			using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read))
			{
				xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
			}
			ConvertProjectXml(xElement);
			using (Stream stream2 = Storage.OpenFile(path, OpenFileMode.Create))
			{
				XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
			}
		}

		public static void MigrateDataFromIsolatedStorageWithDialog()
		{
			try
			{
				if (Storage.DirectoryExists("data:/.config/.isolated-storage"))
				{
					Log.Information("1.26 data found, starting migration to 1.27.");
					BusyDialog dialog = new BusyDialog("Please wait", "Migrating 1.26 data to 1.27 format...");
					DialogsManager.ShowDialog(null, dialog);
					Task.Run(delegate
					{
						string empty = string.Empty;
						string empty2 = string.Empty;
						try
						{
							int num = MigrateFolder("data:/.config/.isolated-storage", "data:");
							empty = "Migration Successful";
							empty2 = $"{num} file(s) were migrated from 1.26 to 1.27.";
							AnalyticsManager.LogEvent("[Migration to 1.27]", new AnalyticsParameter("Details", empty2));
						}
						catch (Exception ex2)
						{
							empty = "Migration Failed";
							empty2 = ex2.Message;
							Log.Error("Migration to 1.27 failed, reason: {0}", ex2.Message);
							AnalyticsManager.LogError("Migration to 1.27 failed", ex2);
						}
						DialogsManager.HideDialog(dialog);
						DialogsManager.ShowDialog(null, new MessageDialog(empty, empty2, LanguageControl.Get("Usual","ok"), null, null));
						Dispatcher.Dispatch(delegate
						{
							SettingsManager.LoadSettings();
						});
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("Failed to migrate data. Reason: {0}", ex.Message);
				AnalyticsManager.LogError("Migration to 1.27 failed", ex);
			}
		}

		public void ConvertTypesToEngine(XElement node)
		{
			foreach (XElement item in node.DescendantsAndSelf("Value"))
			{
				XAttribute xAttribute = item.Attribute("Type");
				if (xAttribute != null)
				{
					if (xAttribute.Value == "Microsoft.Xna.Framework.Vector2")
					{
						xAttribute.Value = "Engine.Vector2";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Vector3")
					{
						xAttribute.Value = "Engine.Vector3";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Vector4")
					{
						xAttribute.Value = "Engine.Vector4";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Quaternion")
					{
						xAttribute.Value = "Engine.Quaternion";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Matrix")
					{
						xAttribute.Value = "Engine.Matrix";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Point")
					{
						xAttribute.Value = "Engine.Point2";
					}
					else if (xAttribute.Value == "Microsoft.Xna.Framework.Color")
					{
						xAttribute.Value = "Engine.Color";
					}
					else if (xAttribute.Value == "Game.Point3")
					{
						xAttribute.Value = "Engine.Point3";
					}
				}
			}
		}

		public static int MigrateFolder(string sourceFolderName, string targetFolderName)
		{
			int num = 0;
			Storage.CreateDirectory(targetFolderName);
			foreach (string item in Storage.ListDirectoryNames(sourceFolderName))
			{
				num += MigrateFolder(Storage.CombinePaths(sourceFolderName, item), Storage.CombinePaths(targetFolderName, item));
			}
			foreach (string item2 in Storage.ListFileNames(sourceFolderName))
			{
				MigrateFile(Storage.CombinePaths(sourceFolderName, item2), targetFolderName);
				num++;
			}
			Storage.DeleteDirectory(sourceFolderName);
			Log.Information("Migrated {0}", sourceFolderName);
			return num;
		}

		public static void MigrateFile(string sourceFileName, string targetFolderName)
		{
			Storage.CopyFile(sourceFileName, Storage.CombinePaths(targetFolderName, Storage.GetFileName(sourceFileName)));
			Storage.DeleteFile(sourceFileName);
			Log.Information("Migrated {0}", sourceFileName);
		}
	}
}
