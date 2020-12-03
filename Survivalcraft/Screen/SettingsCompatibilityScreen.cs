using System.Xml.Linq;

namespace Game
{
	public class SettingsCompatibilityScreen : Screen
	{
		private ButtonWidget m_singlethreadedTerrainUpdateButton;

		private ButtonWidget m_useAudioTrackCachingButton;

		private ContainerWidget m_disableAudioTrackCachingContainer;

		private ButtonWidget m_useReducedZRangeButton;

		private ContainerWidget m_useReducedZRangeContainer;

		private ButtonWidget m_viewGameLogButton;

		private ButtonWidget m_resetDefaultsButton;

		private LabelWidget m_descriptionLabel;

		public SettingsCompatibilityScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsCompatibilityScreen");
			LoadContents(this, node);
			m_singlethreadedTerrainUpdateButton = Children.Find<ButtonWidget>("SinglethreadedTerrainUpdateButton");
			m_useAudioTrackCachingButton = Children.Find<ButtonWidget>("UseAudioTrackCachingButton");
			m_disableAudioTrackCachingContainer = Children.Find<ContainerWidget>("DisableAudioTrackCachingContainer");
			m_useReducedZRangeButton = Children.Find<ButtonWidget>("UseReducedZRangeButton");
			m_useReducedZRangeContainer = Children.Find<ContainerWidget>("UseReducedZRangeContainer");
			m_viewGameLogButton = Children.Find<ButtonWidget>("ViewGameLogButton");
			m_resetDefaultsButton = Children.Find<ButtonWidget>("ResetDefaultsButton");
			m_descriptionLabel = Children.Find<LabelWidget>("Description");
			m_useAudioTrackCachingButton.IsVisible = true;

		}

		public override void Enter(object[] parameters)
		{
			m_descriptionLabel.Text = string.Empty;
			m_disableAudioTrackCachingContainer.IsVisible = true;
			m_useReducedZRangeContainer.IsVisible = true;
		}

		public override void Update()
		{
			if (m_singlethreadedTerrainUpdateButton.IsClicked)
			{
				SettingsManager.MultithreadedTerrainUpdate = !SettingsManager.MultithreadedTerrainUpdate;
				m_descriptionLabel.Text = StringsManager.GetString("Settings.Compatibility.SinglethreadedTerrainUpdate.Description");
			}
			if (m_useReducedZRangeButton.IsClicked)
			{
				SettingsManager.UseReducedZRange = !SettingsManager.UseReducedZRange;
				m_descriptionLabel.Text = StringsManager.GetString("Settings.Compatibility.UseReducedZRange.Description");
			}
			if (m_viewGameLogButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ViewGameLogDialog());
			}
			if (m_resetDefaultsButton.IsClicked)
			{
				SettingsManager.MultithreadedTerrainUpdate = true;
				SettingsManager.UseReducedZRange = false;
			}
			if (m_useAudioTrackCachingButton.IsClicked) SettingsManager.EnableAndroidAudioTrackCaching = !SettingsManager.EnableAndroidAudioTrackCaching;
			m_singlethreadedTerrainUpdateButton.Text = (SettingsManager.MultithreadedTerrainUpdate ? LanguageControl.Get("Usual","off") : LanguageControl.Get("Usual", "on"));
			m_useReducedZRangeButton.Text = (SettingsManager.UseReducedZRange ? LanguageControl.Get("Usual", "on") : LanguageControl.Get("Usual", "off"));
			m_useAudioTrackCachingButton.Text = (SettingsManager.EnableAndroidAudioTrackCaching? LanguageControl.Get("Usual", "on") : LanguageControl.Get("Usual", "off"));
			m_resetDefaultsButton.IsEnabled = (!SettingsManager.MultithreadedTerrainUpdate || SettingsManager.UseReducedZRange);
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
