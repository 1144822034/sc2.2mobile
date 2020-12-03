using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Game
{
	public static class ExternalContentManager
	{
		private static List<IExternalContentProvider> m_providers;
		public static string fName = "ExternalContentManager";
		public static IExternalContentProvider DefaultProvider
		{
			get
			{
				if (Providers.Count <= 0)
				{
					return null;
				}
				return Providers[0];
			}
		}

		public static ReadOnlyList<IExternalContentProvider> Providers => new ReadOnlyList<IExternalContentProvider>(m_providers);

		public static void Initialize()
		{
			m_providers = new List<IExternalContentProvider>();
			m_providers.Add(new AndroidSdCardExternalContentProvider());
			m_providers.Add(new SPMBoxExternalContentProvider());
			m_providers.Add(new DropboxExternalContentProvider());
			m_providers.Add(new TransferShExternalContentProvider());
		}

		public static ExternalContentType ExtensionToType(string extension)
		{
			extension = extension.ToLower();
			foreach (ExternalContentType value in Enum.GetValues(typeof(ExternalContentType)))
			{
				if (GetEntryTypeExtensions(value).FirstOrDefault((string e) => e == extension) != null)
				{
					return value;
				}
			}
			return ExternalContentType.Unknown;
		}

		public static IEnumerable<string> GetEntryTypeExtensions(ExternalContentType type)
		{
			switch (type)
			{
				case ExternalContentType.World:
					yield return ".scworld";
					break;
				case ExternalContentType.BlocksTexture:
					yield return ".scbtex";
					yield return ".png";
					break;
				case ExternalContentType.CharacterSkin:
					yield return ".scskin";
					break;
				case ExternalContentType.FurniturePack:
					yield return ".scfpack";
					break;
			}
		}

		public static Subtexture GetEntryTypeIcon(ExternalContentType type)
		{
			switch (type)
			{
				case ExternalContentType.Directory:
					return ContentManager.Get<Subtexture>("Textures/Atlas/FolderIcon");
				case ExternalContentType.World:
					return ContentManager.Get<Subtexture>("Textures/Atlas/WorldIcon");
				case ExternalContentType.BlocksTexture:
					return ContentManager.Get<Subtexture>("Textures/Atlas/TexturePackIcon");
				case ExternalContentType.CharacterSkin:
					return ContentManager.Get<Subtexture>("Textures/Atlas/CharacterSkinIcon");
				case ExternalContentType.FurniturePack:
					return ContentManager.Get<Subtexture>("Textures/Atlas/FurnitureIcon");
				default:
					return ContentManager.Get<Subtexture>("Textures/Atlas/QuestionMarkIcon");
			}
		}

		public static string GetEntryTypeDescription(ExternalContentType type)
		{
			switch (type)
			{
				case ExternalContentType.Directory:
					return LanguageControl.Get(fName, "Directory");
				case ExternalContentType.World:
					return LanguageControl.Get(fName, "World");
				case ExternalContentType.BlocksTexture:
					return LanguageControl.Get(fName, "Blocks Texture");
				case ExternalContentType.CharacterSkin:
					return LanguageControl.Get(fName, "Character Skin");
				case ExternalContentType.FurniturePack:
					return LanguageControl.Get(fName, "Furniture Pack");
				default:
					return string.Empty;
			}
		}

		public static bool IsEntryTypeDownloadSupported(ExternalContentType type)
		{
			switch (type)
			{
				case ExternalContentType.World:
					return true;
				case ExternalContentType.BlocksTexture:
					return true;
				case ExternalContentType.CharacterSkin:
					return true;
				case ExternalContentType.FurniturePack:
					return true;
				default:
					return false;
			}
		}

		public static bool DoesEntryTypeRequireName(ExternalContentType type)
		{
			switch (type)
			{
				case ExternalContentType.BlocksTexture:
					return true;
				case ExternalContentType.CharacterSkin:
					return true;
				case ExternalContentType.FurniturePack:
					return true;
				default:
					return false;
			}
		}

		public static Exception VerifyExternalContentName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return new InvalidOperationException(LanguageControl.Get(fName,1));
			}
			if (name.Length > 50)
			{
				return new InvalidOperationException(LanguageControl.Get(fName,2));
			}
			if (name[0] == ' ' || name[name.Length - 1] == ' ')
			{
				return new InvalidOperationException(LanguageControl.Get(fName,3));
			}
			return null;
		}

		public static void DeleteExternalContent(ExternalContentType type, string name)
		{
			switch (type)
			{
				case ExternalContentType.World:
					WorldsManager.DeleteWorld(name);
					break;
				case ExternalContentType.BlocksTexture:
					BlocksTexturesManager.DeleteBlocksTexture(name);
					break;
				case ExternalContentType.CharacterSkin:
					CharacterSkinsManager.DeleteCharacterSkin(name);
					break;
				case ExternalContentType.FurniturePack:
					FurniturePacksManager.DeleteFurniturePack(name);
					break;
				default:
					throw new InvalidOperationException(LanguageControl.Get(fName,4));
			}
		}

		public static void ImportExternalContent(Stream stream, ExternalContentType type, string name, Action<string> success, Action<Exception> failure)
		{
			Task.Run(delegate
			{
				try
				{
					success(ImportExternalContentSync(stream, type, name));
				}
				catch (Exception obj)
				{
					failure(obj);
				}
			});
		}

		public static string ImportExternalContentSync(Stream stream, ExternalContentType type, string name)
		{
			switch (type)
			{
				case ExternalContentType.World:
					return WorldsManager.ImportWorld(stream);
				case ExternalContentType.BlocksTexture:
					return BlocksTexturesManager.ImportBlocksTexture(name, stream);
				case ExternalContentType.CharacterSkin:
					return CharacterSkinsManager.ImportCharacterSkin(name, stream);
				case ExternalContentType.FurniturePack:
					return FurniturePacksManager.ImportFurniturePack(name, stream);
				default:
					throw new InvalidOperationException(LanguageControl.Get(fName,4));
			}
		}

		public static void ShowLoginUiIfNeeded(IExternalContentProvider provider, bool showWarningDialog, Action handler)
		{
			if (provider.RequiresLogin && !provider.IsLoggedIn)
			{
				Action loginAction = delegate
				{
					CancellableBusyDialog busyDialog = new CancellableBusyDialog(LanguageControl.Get(fName,5), autoHideOnCancel: true);
					DialogsManager.ShowDialog(null, busyDialog);
					provider.Login(busyDialog.Progress, delegate
					{
						DialogsManager.HideDialog(busyDialog);
						handler?.Invoke();
					}, delegate (Exception error)
					{
						DialogsManager.HideDialog(busyDialog);
						if (error != null)
						{
							DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual","error"), error.Message, LanguageControl.Get("Usual", "ok"), null, null));
						}
					});
				};
				if (showWarningDialog)
				{
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 6),string.Format(LanguageControl.Get(fName, 7),provider.DisplayName), LanguageControl.Get(fName, 8), LanguageControl.Get("Usual", "cancel"), delegate (MessageDialogButton b)
					{
						if (b == MessageDialogButton.Button1)
						{
							loginAction();
						}
					}));
				}
				else
				{
					loginAction();
				}
			}
			else
			{
				handler?.Invoke();
			}
		}

		public static void ShowUploadUi(ExternalContentType type, string name)
		{
			DialogsManager.ShowDialog(null, new SelectExternalContentProviderDialog(LanguageControl.Get(fName, 9), listingSupportRequired: false, delegate (IExternalContentProvider provider)
			{
				try
				{
					if (provider != null)
					{
						ShowLoginUiIfNeeded(provider, showWarningDialog: true, delegate
						{
							CancellableBusyDialog busyDialog = new CancellableBusyDialog(LanguageControl.Get(fName, 10), autoHideOnCancel: false);
							DialogsManager.ShowDialog(null, busyDialog);
							Task.Run(delegate
							{
								bool needsDelete = false;
								string sourcePath = null;
								Stream stream = null;
								Action cleanup = delegate
								{
									Utilities.Dispose(ref stream);
									if (needsDelete && sourcePath != null)
									{
										try
										{
											Storage.DeleteFile(sourcePath);
										}
										catch
										{
										}
									}
								};
								try
								{
									string path;
									if (type == ExternalContentType.BlocksTexture)
									{
										sourcePath = BlocksTexturesManager.GetFileName(name);
										if (sourcePath == null)
										{
											throw new InvalidOperationException(LanguageControl.Get(fName, 11));
										}
										path = Storage.GetFileName(sourcePath);
									}
									else if (type == ExternalContentType.CharacterSkin)
									{
										sourcePath = CharacterSkinsManager.GetFileName(name);
										if (sourcePath == null)
										{
											throw new InvalidOperationException(LanguageControl.Get(fName, 11));
										}
										path = Storage.GetFileName(sourcePath);
									}
									else if (type == ExternalContentType.FurniturePack)
									{
										sourcePath = FurniturePacksManager.GetFileName(name);
										if (sourcePath == null)
										{
											throw new InvalidOperationException(LanguageControl.Get(fName, 11));
										}
										path = Storage.GetFileName(sourcePath);
									}
									else
									{
										if (type != ExternalContentType.World)
										{
											throw new InvalidOperationException(LanguageControl.Get(fName, 12));
										}
										busyDialog.LargeMessage = LanguageControl.Get(fName, 13);
										sourcePath = "android:SurvivalCraft2.2/WorldUpload.tmp";
										needsDelete = true;
										string name2 = WorldsManager.GetWorldInfo(name).WorldSettings.Name;
										path = $"{name2}.scworld";
										using (Stream targetStream = Storage.OpenFile(sourcePath, OpenFileMode.Create))
										{
											WorldsManager.ExportWorld(name, targetStream);
										}
									}
									busyDialog.LargeMessage = LanguageControl.Get(fName, 14);
									stream = Storage.OpenFile(sourcePath, OpenFileMode.Read);
									provider.Upload(path, stream, busyDialog.Progress, delegate (string link)
									{
										long length = stream.Length;
										cleanup();
										DialogsManager.HideDialog(busyDialog);
										if (string.IsNullOrEmpty(link))
										{
											DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual","success"),string.Format(LanguageControl.Get(fName, 15), DataSizeFormatter.Format(length)), LanguageControl.Get("Usual", "ok"), null, null));
										}
										else
										{
											DialogsManager.ShowDialog(null, new ExternalContentLinkDialog(link));
										}
									}, delegate (Exception error)
									{
										cleanup();
										DialogsManager.HideDialog(busyDialog);
										DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), error.Message, LanguageControl.Get("Usual", "ok"), null, null));
									});
								}
								catch (Exception ex2)
								{
									cleanup();
									DialogsManager.HideDialog(busyDialog);
									DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), ex2.Message, LanguageControl.Get("Usual", "ok"), null, null));
								}
							});
						});
					}
				}
				catch (Exception ex)
				{
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), ex.Message, LanguageControl.Get("Usual", "ok"), null, null));
				}
			}));
		}
	}
}
