using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public class HelpScreen : Screen
	{
		public ListPanelWidget m_topicsList;

		public ButtonWidget m_recipaediaButton;

		public ButtonWidget m_bestiaryButton;

		public Screen m_previousScreen;

		public Dictionary<string, HelpTopic> m_topics = new Dictionary<string, HelpTopic>();

		public HelpScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/HelpScreen");
			LoadContents(this, node);
			m_topicsList = Children.Find<ListPanelWidget>("TopicsList");
			m_recipaediaButton = Children.Find<ButtonWidget>("RecipaediaButton");
			m_bestiaryButton = Children.Find<ButtonWidget>("BestiaryButton");
			m_topicsList.ItemWidgetFactory = delegate (object item)
			{
				HelpTopic helpTopic3 = (HelpTopic)item;
				XElement node2 = ContentManager.Get<XElement>("Widgets/HelpTopicItem");
				ContainerWidget obj = (ContainerWidget)Widget.LoadWidget(this, node2, null);
				obj.Children.Find<LabelWidget>("HelpTopicItem.Title").Text = helpTopic3.Title;
				return obj;
			};
			m_topicsList.ItemClicked += delegate (object item)
			{
				HelpTopic helpTopic2 = item as HelpTopic;
				if (helpTopic2 != null)
				{
					ShowTopic(helpTopic2);
				}
			};
			foreach (KeyValuePair<string, Dictionary<string, string>> item in LanguageControl.items2["Help"])
			{
				if (item.Value.ContainsKey("DisabledPlatforms"))
				{
					item.Value.TryGetValue("DisabledPlatforms", out string displa);
					if (displa.Split(new string[] { "," }, StringSplitOptions.None).FirstOrDefault((string s) => s.Trim().ToLower() == VersionsManager.Platform.ToString().ToLower()) == null) continue;
				}
				item.Value.TryGetValue("Title", out string Title);
				item.Value.TryGetValue("Name", out string Name);
				item.Value.TryGetValue("value", out string value);
				if (string.IsNullOrEmpty(Title)) Title = string.Empty;
				if (string.IsNullOrEmpty(Name)) Name = string.Empty;
				if (string.IsNullOrEmpty(value)) value = string.Empty;

				string attributeValue = Name;
				string attributeValue2 = Title;
				string text = string.Empty;
				string[] array = value.Split(new string[] { "\n" }, StringSplitOptions.None);
				foreach (string text2 in array)
				{
					text = text + text2.Trim() + " ";
				}
				text = text.Replace("\r", "");
				text = text.Replace("’", "'");
				text = text.Replace("\\n", "\n");
				HelpTopic helpTopic = new HelpTopic
				{
					Name = attributeValue,
					Title = attributeValue2,
					Text = text
				};
				if (!string.IsNullOrEmpty(helpTopic.Name))
				{
					m_topics.Add(helpTopic.Name, helpTopic);
				}
				m_topicsList.AddItem(helpTopic);
			}
		}

		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("HelpTopic") && ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("Recipaedia") && ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("Bestiary"))
			{
				m_previousScreen = ScreensManager.PreviousScreen;
			}
		}

		public override void Leave()
		{
			m_topicsList.SelectedItem = null;
		}

		public override void Update()
		{
			if (m_recipaediaButton.IsClicked)
			{
				ScreensManager.SwitchScreen("Recipaedia");
			}
			if (m_bestiaryButton.IsClicked)
			{
				ScreensManager.SwitchScreen("Bestiary");
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(m_previousScreen);
			}
		}

		public HelpTopic GetTopic(string name)
		{
			return m_topics[name];
		}

		public void ShowTopic(HelpTopic helpTopic)
		{
			if (helpTopic.Name == "Keyboard")
			{
				DialogsManager.ShowDialog(null, new KeyboardHelpDialog());
			}
			else if (helpTopic.Name == "Gamepad")
			{
				DialogsManager.ShowDialog(null, new GamepadHelpDialog());
			}
			else
			{
				ScreensManager.SwitchScreen("HelpTopic", helpTopic);
			}
		}
	}
}
