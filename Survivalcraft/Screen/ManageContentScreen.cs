using Engine;
using Engine.Graphics;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class ManageContentScreen : Screen
{
	private class ListItem
	{
		public ExternalContentType Type;

		public bool IsBuiltIn;

		public string Name;

		public string DisplayName;

		public DateTime CreationTime;

		public int UseCount;
	}
	public static string fName = "ManageContentScreen";

	private ListPanelWidget m_contentList;

	private ButtonWidget m_deleteButton;

	private ButtonWidget m_uploadButton;

	private LabelWidget m_filterLabel;

	private ButtonWidget m_changeFilterButton;

	private BlocksTexturesCache m_blocksTexturesCache = new BlocksTexturesCache();

	private CharacterSkinsCache m_characterSkinsCache = new CharacterSkinsCache();

	private ExternalContentType m_filter;

	public ManageContentScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/ManageContentScreen");
		LoadContents(this, node);
		m_contentList = Children.Find<ListPanelWidget>("ContentList");
		m_deleteButton = Children.Find<ButtonWidget>("DeleteButton");
		m_uploadButton = Children.Find<ButtonWidget>("UploadButton");
		m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
		m_filterLabel = Children.Find<LabelWidget>("Filter");
		m_contentList.ItemWidgetFactory = delegate (object obj)
		{
			ListItem listItem = (ListItem)obj;
			ContainerWidget containerWidget;
			if (listItem.Type == ExternalContentType.BlocksTexture)
			{
				XElement node2 = ContentManager.Get<XElement>("Widgets/BlocksTextureItem");
				containerWidget = (ContainerWidget)Widget.LoadWidget(this, node2, null);
				RectangleWidget rectangleWidget = containerWidget.Children.Find<RectangleWidget>("BlocksTextureItem.Icon");
				LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Text");
				LabelWidget labelWidget2 = containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Details");
				Texture2D texture = m_blocksTexturesCache.GetTexture(listItem.Name);
				BlocksTexturesManager.GetCreationDate(listItem.Name);
				rectangleWidget.Subtexture = new Subtexture(texture, Vector2.Zero, Vector2.One);
				labelWidget.Text = listItem.DisplayName;
				labelWidget2.Text = string.Format(LanguageControl.Get(fName,1),texture.Width,texture.Height);
				if (!listItem.IsBuiltIn)
				{
					labelWidget2.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
					if (listItem.UseCount > 0)
					{
						labelWidget2.Text += string.Format(LanguageControl.Get(fName,2),listItem.UseCount);
					}
				}
			}
			else
			{
				if (listItem.Type != ExternalContentType.CharacterSkin)
				{
					if (listItem.Type == ExternalContentType.FurniturePack)
					{
						XElement node3 = ContentManager.Get<XElement>("Widgets/FurniturePackItem");
						containerWidget = (ContainerWidget)Widget.LoadWidget(this, node3, null);
						LabelWidget labelWidget3 = containerWidget.Children.Find<LabelWidget>("FurniturePackItem.Text");
						LabelWidget labelWidget4 = containerWidget.Children.Find<LabelWidget>("FurniturePackItem.Details");
						labelWidget3.Text = listItem.DisplayName;
						try
						{
							List<FurnitureDesign> designs = FurniturePacksManager.LoadFurniturePack(null, listItem.Name);
							labelWidget4.Text = string.Format(LanguageControl.Get(fName,3), FurnitureDesign.ListChains(designs).Count);
							if (string.IsNullOrEmpty(listItem.Name))
							{
								return containerWidget;
							}
							labelWidget4.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
							return containerWidget;
						}
						catch (Exception ex)
						{
							labelWidget4.Text = labelWidget4.Text + LanguageControl.Get("Usual","error") + ex.Message;
							return containerWidget;
						}
					}
					throw new InvalidOperationException(LanguageControl.Get(fName,10));
				}
				XElement node4 = ContentManager.Get<XElement>("Widgets/CharacterSkinItem");
				containerWidget = (ContainerWidget)Widget.LoadWidget(this, node4, null);
				PlayerModelWidget playerModelWidget = containerWidget.Children.Find<PlayerModelWidget>("CharacterSkinItem.Model");
				LabelWidget labelWidget5 = containerWidget.Children.Find<LabelWidget>("CharacterSkinItem.Text");
				LabelWidget labelWidget6 = containerWidget.Children.Find<LabelWidget>("CharacterSkinItem.Details");
				Texture2D texture2 = m_characterSkinsCache.GetTexture(listItem.Name);
				playerModelWidget.PlayerClass = PlayerClass.Male;
				playerModelWidget.CharacterSkinTexture = texture2;
				labelWidget5.Text = listItem.DisplayName;
				labelWidget6.Text = string.Format(LanguageControl.Get(fName,4), texture2.Width,texture2.Height);
				if (!listItem.IsBuiltIn)
				{
					labelWidget6.Text += $" | {listItem.CreationTime.ToLocalTime():dd MMM yyyy HH:mm}";
					if (listItem.UseCount > 0)
					{
						labelWidget6.Text += string.Format(LanguageControl.Get(fName, 2), listItem.UseCount);
					}
				}
			}
			return containerWidget;
		};
	}

	public override void Enter(object[] parameters)
	{
		UpdateList();
	}

	public override void Leave()
	{
		m_blocksTexturesCache.Clear();
		m_characterSkinsCache.Clear();
	}

	public override void Update()
	{
		ListItem selectedItem = (ListItem)m_contentList.SelectedItem;
		m_deleteButton.IsEnabled = (selectedItem != null && !selectedItem.IsBuiltIn);
		m_uploadButton.IsEnabled = (selectedItem != null && !selectedItem.IsBuiltIn);
		m_filterLabel.Text = GetFilterDisplayName(m_filter);
		if (m_deleteButton.IsClicked)
		{
			string smallMessage = (selectedItem.UseCount <= 0) ? string.Format(LanguageControl.Get(fName,5), selectedItem.DisplayName) : string.Format(LanguageControl.Get(fName, 6), selectedItem.DisplayName,selectedItem.UseCount);
			DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName,9), smallMessage, LanguageControl.Get("Usual","yes"), LanguageControl.Get("Usual","no"), delegate (MessageDialogButton button)
			{
				if (button == MessageDialogButton.Button1)
				{
					ExternalContentManager.DeleteExternalContent(selectedItem.Type, selectedItem.Name);
					UpdateList();
				}
			}));
		}
		if (m_uploadButton.IsClicked)
		{
			ExternalContentManager.ShowUploadUi(selectedItem.Type, selectedItem.Name);
		}
		if (m_changeFilterButton.IsClicked)
		{
			List<ExternalContentType> list = new List<ExternalContentType>();
			list.Add(ExternalContentType.Unknown);
			list.Add(ExternalContentType.BlocksTexture);
			list.Add(ExternalContentType.CharacterSkin);
			list.Add(ExternalContentType.FurniturePack);
			DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(fName,7), list, 60f, (object item) => GetFilterDisplayName((ExternalContentType)item), delegate (object item)
			{
				if ((ExternalContentType)item != m_filter)
				{
					m_filter = (ExternalContentType)item;
					UpdateList();
				}
			}));
		}
		if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
		}
	}

	private void UpdateList()
	{
		WorldsManager.UpdateWorldsList();
		List<ListItem> list = new List<ListItem>();
		if (m_filter == ExternalContentType.BlocksTexture || m_filter == ExternalContentType.Unknown)
		{
			BlocksTexturesManager.UpdateBlocksTexturesList();
			foreach (string name2 in BlocksTexturesManager.BlockTexturesNames)
			{
				list.Add(new ListItem
				{
					Name = name2,
					IsBuiltIn = BlocksTexturesManager.IsBuiltIn(name2),
					Type = ExternalContentType.BlocksTexture,
					DisplayName = BlocksTexturesManager.GetDisplayName(name2),
					CreationTime = BlocksTexturesManager.GetCreationDate(name2),
					UseCount = WorldsManager.WorldInfos.Count((WorldInfo wi) => wi.WorldSettings.BlocksTextureName == name2)
				});
			}
		}
		if (m_filter == ExternalContentType.CharacterSkin || m_filter == ExternalContentType.Unknown)
		{
			CharacterSkinsManager.UpdateCharacterSkinsList();
			foreach (string name in CharacterSkinsManager.CharacterSkinsNames)
			{
				list.Add(new ListItem
				{
					Name = name,
					IsBuiltIn = CharacterSkinsManager.IsBuiltIn(name),
					Type = ExternalContentType.CharacterSkin,
					DisplayName = CharacterSkinsManager.GetDisplayName(name),
					CreationTime = CharacterSkinsManager.GetCreationDate(name),
					UseCount = WorldsManager.WorldInfos.Count((WorldInfo wi) => wi.PlayerInfos.Any((PlayerInfo pi) => pi.CharacterSkinName == name))
				});
			}
		}
		if (m_filter == ExternalContentType.FurniturePack || m_filter == ExternalContentType.Unknown)
		{
			FurniturePacksManager.UpdateFurniturePacksList();
			foreach (string furniturePackName in FurniturePacksManager.FurniturePackNames)
			{
				list.Add(new ListItem
				{
					Name = furniturePackName,
					IsBuiltIn = false,
					Type = ExternalContentType.FurniturePack,
					DisplayName = FurniturePacksManager.GetDisplayName(furniturePackName),
					CreationTime = FurniturePacksManager.GetCreationDate(furniturePackName)
				});
			}
		}
		list.Sort(delegate (ListItem o1, ListItem o2)
		{
			if (o1.IsBuiltIn && !o2.IsBuiltIn)
			{
				return -1;
			}
			if (o2.IsBuiltIn && !o1.IsBuiltIn)
			{
				return 1;
			}
			if (string.IsNullOrEmpty(o1.Name) && !string.IsNullOrEmpty(o2.Name))
			{
				return -1;
			}
			return (!string.IsNullOrEmpty(o1.Name) && string.IsNullOrEmpty(o2.Name)) ? 1 : string.Compare(o1.DisplayName, o2.DisplayName);
		});
		m_contentList.ClearItems();
		foreach (ListItem item in list)
		{
			m_contentList.AddItem(item);
		}
	}

	private static string GetFilterDisplayName(ExternalContentType filter)
	{
		if (filter == ExternalContentType.Unknown)
		{
			return LanguageControl.Get(fName,8);
		}
		return ExternalContentManager.GetEntryTypeDescription(filter);
	}
}
