using Engine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class SettingsPerformanceScreen : Screen
	{
		public static List<int> m_presentationIntervals = new List<int>
		{
			2,
			1,
			0
		};

		public static List<int> m_visibilityRanges = new List<int>
		{
			32,
			48,
			64,
			80,
			96,
			112,
			128,
			160,
			192,
			224,
			256,
			320,
			384,
			448,
			512,
			576,
			640,
			704,
			768,
			832,
			896,
			960,
			1024
		};

		public ButtonWidget m_resolutionButton;

		public SliderWidget m_visibilityRangeSlider;

		public LabelWidget m_visibilityRangeWarningLabel;

		public ButtonWidget m_viewAnglesButton;

		public ButtonWidget m_terrainMipmapsButton;

		public ButtonWidget m_skyRenderingModeButton;

		public ButtonWidget m_objectShadowsButton;

		public SliderWidget m_framerateLimitSlider;

		public ButtonWidget m_displayFpsCounterButton;

		public ButtonWidget m_displayFpsRibbonButton;

		public int m_enterVisibilityRange;
		public static string fName = "SettingsPerformanceScreen";
		public SettingsPerformanceScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsPerformanceScreen");
			LoadContents(this, node);
			m_resolutionButton = Children.Find<ButtonWidget>("ResolutionButton");
			m_visibilityRangeSlider = Children.Find<SliderWidget>("VisibilityRangeSlider");
			m_visibilityRangeWarningLabel = Children.Find<LabelWidget>("VisibilityRangeWarningLabel");
			m_viewAnglesButton = Children.Find<ButtonWidget>("ViewAnglesButton");
			m_terrainMipmapsButton = Children.Find<ButtonWidget>("TerrainMipmapsButton");
			m_skyRenderingModeButton = Children.Find<ButtonWidget>("SkyRenderingModeButton");
			m_objectShadowsButton = Children.Find<ButtonWidget>("ObjectShadowsButton");
			m_framerateLimitSlider = Children.Find<SliderWidget>("FramerateLimitSlider");
			m_displayFpsCounterButton = Children.Find<ButtonWidget>("DisplayFpsCounterButton");
			m_displayFpsRibbonButton = Children.Find<ButtonWidget>("DisplayFpsRibbonButton");
			m_visibilityRangeSlider.MinValue = 0f;
			m_visibilityRangeSlider.MaxValue = m_visibilityRanges.Count - 1;
		}

		public override void Enter(object[] parameters)
		{
			m_enterVisibilityRange = SettingsManager.VisibilityRange;
		}

		public override void Update()
		{
			if (m_resolutionButton.IsClicked)
			{
				IList<int> enumValues = EnumUtils.GetEnumValues(typeof(ResolutionMode));
				SettingsManager.ResolutionMode = (ResolutionMode)((enumValues.IndexOf((int)SettingsManager.ResolutionMode) + 1) % enumValues.Count);
			}
			if (m_visibilityRangeSlider.IsSliding)
			{
				SettingsManager.VisibilityRange = m_visibilityRanges[MathUtils.Clamp((int)m_visibilityRangeSlider.Value, 0, m_visibilityRanges.Count - 1)];
			}
			if (m_viewAnglesButton.IsClicked)
			{
				IList<int> enumValues2 = EnumUtils.GetEnumValues(typeof(ViewAngleMode));
				SettingsManager.ViewAngleMode = (ViewAngleMode)((enumValues2.IndexOf((int)SettingsManager.ViewAngleMode) + 1) % enumValues2.Count);
			}
			if (m_terrainMipmapsButton.IsClicked)
			{
				SettingsManager.TerrainMipmapsEnabled = !SettingsManager.TerrainMipmapsEnabled;
			}
			if (m_skyRenderingModeButton.IsClicked)
			{
				IList<int> enumValues3 = EnumUtils.GetEnumValues(typeof(SkyRenderingMode));
				SettingsManager.SkyRenderingMode = (SkyRenderingMode)((enumValues3.IndexOf((int)SettingsManager.SkyRenderingMode) + 1) % enumValues3.Count);
			}
			if (m_objectShadowsButton.IsClicked)
			{
				SettingsManager.ObjectsShadowsEnabled = !SettingsManager.ObjectsShadowsEnabled;
			}
			if (m_framerateLimitSlider.IsSliding)
			{
				SettingsManager.PresentationInterval = m_presentationIntervals[MathUtils.Clamp((int)m_framerateLimitSlider.Value, 0, m_presentationIntervals.Count - 1)];
			}
			if (m_displayFpsCounterButton.IsClicked)
			{
				SettingsManager.DisplayFpsCounter = !SettingsManager.DisplayFpsCounter;
			}
			if (m_displayFpsRibbonButton.IsClicked)
			{
				SettingsManager.DisplayFpsRibbon = !SettingsManager.DisplayFpsRibbon;
			}
			m_resolutionButton.Text =LanguageControl.Get("ResolutionMode" , SettingsManager.ResolutionMode.ToString());
			m_visibilityRangeSlider.Value = ((m_visibilityRanges.IndexOf(SettingsManager.VisibilityRange) >= 0) ? m_visibilityRanges.IndexOf(SettingsManager.VisibilityRange) : 64);
			m_visibilityRangeSlider.Text = string.Format(LanguageControl.Get(fName,1), SettingsManager.VisibilityRange);
			if (SettingsManager.VisibilityRange <= 48)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName,2);
			}
			else if (SettingsManager.VisibilityRange <= 64)
			{
				m_visibilityRangeWarningLabel.IsVisible = false;
			}
			else if (SettingsManager.VisibilityRange <= 112)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName, 3);
			}
			else if (SettingsManager.VisibilityRange <= 224)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName, 4);
			}
			else if (SettingsManager.VisibilityRange <= 384)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName, 5);
			}
			else if (SettingsManager.VisibilityRange <= 512)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName, 6);
			}
			else
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = LanguageControl.Get(fName, 7);
			}
			m_viewAnglesButton.Text =LanguageControl.Get("ViewAngleMode" , SettingsManager.ViewAngleMode.ToString());
			if (SettingsManager.TerrainMipmapsEnabled) {
				m_terrainMipmapsButton.Text = LanguageControl.Get("Usual","enable");
			}
			else {
				m_terrainMipmapsButton.Text = LanguageControl.Get("Usual", "disable");
			}
			
			m_skyRenderingModeButton.Text =LanguageControl.Get("SkyRenderingMode" , SettingsManager.SkyRenderingMode.ToString());
			m_objectShadowsButton.Text = SettingsManager.ObjectsShadowsEnabled ? LanguageControl.Get("Usual", "enable") : LanguageControl.Get("Usual", "disable");
			m_framerateLimitSlider.Value = (m_presentationIntervals.IndexOf(SettingsManager.PresentationInterval) >= 0) ? m_presentationIntervals.IndexOf(SettingsManager.PresentationInterval) : (m_presentationIntervals.Count - 1);
			m_framerateLimitSlider.Text = (SettingsManager.PresentationInterval != 0) ? string.Format(LanguageControl.Get(fName,8), SettingsManager.PresentationInterval) : LanguageControl.Get(fName,9);
			m_displayFpsCounterButton.Text = (SettingsManager.DisplayFpsCounter ? LanguageControl.Get("Usual","yes") : LanguageControl.Get("Usual","no"));
			m_displayFpsRibbonButton.Text = (SettingsManager.DisplayFpsRibbon ? LanguageControl.Get("Usual","yes") : LanguageControl.Get("Usual","no"));
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				bool flag = SettingsManager.VisibilityRange > 128;
				if (SettingsManager.VisibilityRange > m_enterVisibilityRange && flag)
				{
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName,10), LanguageControl.Get(fName,11), LanguageControl.Get("Usual", "ok"),  LanguageControl.Get("Usual","back"), delegate(MessageDialogButton button)
					{
						if (button == MessageDialogButton.Button1)
						{
							ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
						}
					}));
				}
				else
				{
					ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
				}
			}
		}
	}
}
