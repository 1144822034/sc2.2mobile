using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Game
{
	public class CommunityContentScreen : Screen
	{
		public enum Order
		{
			ByRank,
			ByTime
		}

		public ListPanelWidget m_listPanel;

		public LinkWidget m_moreLink;

		public LabelWidget m_orderLabel;

		public ButtonWidget m_changeOrderButton;

		public LabelWidget m_filterLabel;

		public ButtonWidget m_changeFilterButton;

		public ButtonWidget m_downloadButton;

		public ButtonWidget m_deleteButton;

		public ButtonWidget m_moreOptionsButton;

		public object m_filter;

		public Order m_order;

		public double m_contentExpiryTime;

		public static string fName= "CommunityContentScreen";

		public Dictionary<string, IEnumerable<object>> m_itemsCache = new Dictionary<string, IEnumerable<object>>();

		public CommunityContentScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/CommunityContentScreen");
			LoadContents(this, node);
			m_listPanel = Children.Find<ListPanelWidget>("List");
			m_orderLabel = Children.Find<LabelWidget>("Order");
			m_changeOrderButton = Children.Find<ButtonWidget>("ChangeOrder");
			m_filterLabel = Children.Find<LabelWidget>("Filter");
			m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
			m_downloadButton = Children.Find<ButtonWidget>("Download");
			m_deleteButton = Children.Find<ButtonWidget>("Delete");
			m_moreOptionsButton = Children.Find<ButtonWidget>("MoreOptions");
			m_listPanel.ItemWidgetFactory = delegate(object item)
			{
				CommunityContentEntry communityContentEntry = item as CommunityContentEntry;
				if (communityContentEntry != null)
				{
					XElement node2 = ContentManager.Get<XElement>("Widgets/CommunityContentItem");
					ContainerWidget obj = (ContainerWidget)Widget.LoadWidget(this, node2, null);
					obj.Children.Find<RectangleWidget>("CommunityContentItem.Icon").Subtexture = ExternalContentManager.GetEntryTypeIcon(communityContentEntry.Type);
					obj.Children.Find<LabelWidget>("CommunityContentItem.Text").Text = communityContentEntry.Name;
					obj.Children.Find<LabelWidget>("CommunityContentItem.Details").Text = $"{ExternalContentManager.GetEntryTypeDescription(communityContentEntry.Type)} {DataSizeFormatter.Format(communityContentEntry.Size)}";
					obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").Rating = communityContentEntry.RatingsAverage;
					obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").IsVisible = (communityContentEntry.RatingsAverage > 0f);
					obj.Children.Find<LabelWidget>("CommunityContentItem.ExtraText").Text = communityContentEntry.ExtraText;
					return obj;
				}
				XElement node3 = ContentManager.Get<XElement>("Widgets/CommunityContentItemMore");
				ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(this, node3, null);
				m_moreLink = containerWidget.Children.Find<LinkWidget>("CommunityContentItemMore.Link");
				m_moreLink.Tag = (item as string);
				return containerWidget;
			};
			m_listPanel.SelectionChanged += delegate
			{
				if (m_listPanel.SelectedItem != null && !(m_listPanel.SelectedItem is CommunityContentEntry))
				{
					m_listPanel.SelectedItem = null;
				}
			};
		}

		public override void Enter(object[] parameters)
		{
			m_filter = string.Empty;
			m_order = Order.ByRank;
			PopulateList(null);
		}

		public override void Update()
		{
			CommunityContentEntry communityContentEntry = m_listPanel.SelectedItem as CommunityContentEntry;
			m_downloadButton.IsEnabled = (communityContentEntry != null);
			m_deleteButton.IsEnabled = (UserManager.ActiveUser != null && communityContentEntry != null && communityContentEntry.UserId == UserManager.ActiveUser.UniqueId);
			m_orderLabel.Text = GetOrderDisplayName(m_order);
			m_filterLabel.Text = GetFilterDisplayName(m_filter);
			if (m_changeOrderButton.IsClicked)
			{
				List<Order> items = EnumUtils.GetEnumValues(typeof(Order)).Cast<Order>().ToList();
				DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(fName, "Order Type"), items, 60f, (object item) => GetOrderDisplayName((Order)item), delegate(object item)
				{
					m_order = (Order)item;
					PopulateList(null);
				}));
			}
			if (m_changeFilterButton.IsClicked)
			{
				List<object> list = new List<object>();
				list.Add(string.Empty);
				foreach (ExternalContentType item in from ExternalContentType t in EnumUtils.GetEnumValues(typeof(ExternalContentType))
					where ExternalContentManager.IsEntryTypeDownloadSupported(t)
					select t)
				{
					list.Add(item);
				}
				if (UserManager.ActiveUser != null)
				{
					list.Add(UserManager.ActiveUser.UniqueId);
				}
				DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(fName, "Filter"), list, 60f, (object item) => GetFilterDisplayName(item), delegate(object item)
				{
					m_filter = item;
					PopulateList(null);
				}));
			}
			if (m_downloadButton.IsClicked && communityContentEntry != null)
			{
				DownloadEntry(communityContentEntry);
			}
			if (m_deleteButton.IsClicked && communityContentEntry != null)
			{
				DeleteEntry(communityContentEntry);
			}
			if (m_moreOptionsButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new MoreCommunityLinkDialog());
			}
			if (m_moreLink != null && m_moreLink.IsClicked)
			{
				PopulateList((string)m_moreLink.Tag);
			}
			if (base.Input.Back || Children.Find<BevelledButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen("Content");
			}
			if (base.Input.Hold.HasValue && base.Input.HoldTime > 2f && base.Input.Hold.Value.Y < 20f)
			{
				m_contentExpiryTime = 0.0;
				Task.Delay(250).Wait();
			}
		}

		public void PopulateList(string cursor)
		{
			string text = string.Empty;
			if (SettingsManager.CommunityContentMode == CommunityContentMode.Strict)
			{
				text = "1";
			}
			if (SettingsManager.CommunityContentMode == CommunityContentMode.Normal)
			{
				text = "0";
			}
			string text2 = (m_filter is string) ? ((string)m_filter) : string.Empty;
			string text3 = (m_filter is ExternalContentType) ? LanguageControl.Get(fName, m_filter.ToString()) : string.Empty;
			string text4 = LanguageControl.Get(fName, m_order.ToString());
			string cacheKey = text2 + "\n" + text3 + "\n" + text4 + "\n" + text;
			m_moreLink = null;
			if (string.IsNullOrEmpty(cursor))
			{
				m_listPanel.ClearItems();
				m_listPanel.ScrollPosition = 0f;
				if (m_contentExpiryTime != 0.0 && Time.RealTime < m_contentExpiryTime && m_itemsCache.TryGetValue(cacheKey, out IEnumerable<object> value))
				{
					foreach (object item in value)
					{
						m_listPanel.AddItem(item);
					}
					return;
				}
			}
			CancellableBusyDialog busyDialog = new CancellableBusyDialog(LanguageControl.Get(fName,2), autoHideOnCancel: false);
			DialogsManager.ShowDialog(null, busyDialog);
			CommunityContentManager.List(cursor, text2, text3, text, text4, busyDialog.Progress, delegate(List<CommunityContentEntry> list, string nextCursor)
			{
				DialogsManager.HideDialog(busyDialog);
				m_contentExpiryTime = Time.RealTime + 300.0;
				while (m_listPanel.Items.Count > 0 && !(m_listPanel.Items[m_listPanel.Items.Count - 1] is CommunityContentEntry))
				{
					m_listPanel.RemoveItemAt(m_listPanel.Items.Count - 1);
				}
				foreach (CommunityContentEntry item2 in list)
				{
					m_listPanel.AddItem(item2);
				}
				if (list.Count > 0 && !string.IsNullOrEmpty(nextCursor))
				{
					m_listPanel.AddItem(nextCursor);
				}
				m_itemsCache[cacheKey] = new List<object>(m_listPanel.Items);
			}, delegate(Exception error)
			{
				DialogsManager.HideDialog(busyDialog);
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), error.Message, LanguageControl.Get("Usual","ok"), null, null));
			});
		}

		public void DownloadEntry(CommunityContentEntry entry)
		{
			string userId = (UserManager.ActiveUser != null) ? UserManager.ActiveUser.UniqueId : string.Empty;
			CancellableBusyDialog busyDialog = new CancellableBusyDialog(string.Format(LanguageControl.Get(fName, 1), entry.Name), autoHideOnCancel: false);
			DialogsManager.ShowDialog(null, busyDialog);
			CommunityContentManager.Download(entry.Address, entry.Name, entry.Type, userId, busyDialog.Progress, delegate
			{
				DialogsManager.HideDialog(busyDialog);
			}, delegate(Exception error)
			{
				DialogsManager.HideDialog(busyDialog);
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), error.Message, LanguageControl.Get("Usual", "ok"), null, null));
			});
		}

		public void DeleteEntry(CommunityContentEntry entry)
		{
			if (UserManager.ActiveUser != null)
			{
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 5), LanguageControl.Get("Usual", "yes"), LanguageControl.Get("Usual", "no"), delegate(MessageDialogButton button)
				{
					if (button == MessageDialogButton.Button1)
					{
						CancellableBusyDialog busyDialog = new CancellableBusyDialog(string.Format(LanguageControl.Get(fName,3), entry.Name), autoHideOnCancel: false);
						DialogsManager.ShowDialog(null, busyDialog);
						CommunityContentManager.Delete(entry.Address, UserManager.ActiveUser.UniqueId, busyDialog.Progress, delegate
						{
							DialogsManager.HideDialog(busyDialog);
							DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 6), LanguageControl.Get(fName, 7), LanguageControl.Get("Usual", "ok"), null, null));
						}, delegate(Exception error)
						{
							DialogsManager.HideDialog(busyDialog);
							DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get("Usual", "error"), error.Message, LanguageControl.Get("Usual", "ok"), null, null));
						});
					}
				}));
			}
		}

		public static string GetFilterDisplayName(object filter)
		{
			if (filter is string)
			{
				if (!string.IsNullOrEmpty((string)filter))
				{
					return LanguageControl.Get(fName, 8);
				}
				return LanguageControl.Get(fName, 9);
			}
			if (filter is ExternalContentType)
			{
				return ExternalContentManager.GetEntryTypeDescription((ExternalContentType)filter);
			}
			throw new InvalidOperationException(LanguageControl.Get(fName, 10));
		}

		public static string GetOrderDisplayName(Order order)
		{
			switch (order)
			{
			case Order.ByRank:
				return LanguageControl.Get(fName, 11);
			case Order.ByTime:
				return LanguageControl.Get(fName, 12);
			default:
				throw new InvalidOperationException(LanguageControl.Get(fName, 13));
			}
		}
	}
}
