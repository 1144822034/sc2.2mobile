using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game
{
	public class GameMenuDialog : Dialog
	{
		public static bool m_increaseDetailDialogShown;

		public static bool m_decreaseDetailDialogShown;

		public bool m_adventureRestartExists;

		public StackPanelWidget m_statsPanel;

		public ComponentPlayer m_componentPlayer;

		public static string fName = "GameMenuDialog";

		public GameMenuDialog(ComponentPlayer componentPlayer)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/GameMenuDialog");
			LoadContents(this, node);
			m_statsPanel = Children.Find<StackPanelWidget>("StatsPanel");
			m_componentPlayer = componentPlayer;
			m_adventureRestartExists = WorldsManager.SnapshotExists(GameManager.WorldInfo.DirectoryName, "AdventureRestart");
			if (!m_increaseDetailDialogShown && PerformanceManager.LongTermAverageFrameTime.HasValue && PerformanceManager.LongTermAverageFrameTime.Value * 1000f < 25f && (SettingsManager.VisibilityRange <= 64 || SettingsManager.ResolutionMode == ResolutionMode.Low))
			{
				m_increaseDetailDialogShown = true;
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog(LanguageControl.Get(fName, 1), LanguageControl.Get(fName, 2), LanguageControl.Get("Usual", "ok"), null, null));
				AnalyticsManager.LogEvent("[GameMenuScreen] IncreaseDetailDialog Shown");
			}
			if (!m_decreaseDetailDialogShown && PerformanceManager.LongTermAverageFrameTime.HasValue && PerformanceManager.LongTermAverageFrameTime.Value * 1000f > 50f && (SettingsManager.VisibilityRange >= 64 || SettingsManager.ResolutionMode == ResolutionMode.High))
			{
				m_decreaseDetailDialogShown = true;
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog(LanguageControl.Get(fName, 3), LanguageControl.Get(fName, 4), LanguageControl.Get("Usual", "ok"), null, null));
				AnalyticsManager.LogEvent("[GameMenuScreen] DecreaseDetailDialog Shown");
			}
			m_statsPanel.Children.Clear();
			Project project = componentPlayer.Project;
			PlayerData playerData = componentPlayer.PlayerData;
			PlayerStats playerStats = componentPlayer.PlayerStats;
			SubsystemGameInfo subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			SubsystemFurnitureBlockBehavior subsystemFurnitureBlockBehavior = project.FindSubsystem<SubsystemFurnitureBlockBehavior>(throwOnError: true);
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			BitmapFont font2 = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			Color white = Color.White;
			StackPanelWidget stackPanelWidget = new StackPanelWidget
			{
				Direction = LayoutDirection.Vertical,
				HorizontalAlignment = WidgetAlignment.Center
			};
			m_statsPanel.Children.Add(stackPanelWidget);
			stackPanelWidget.Children.Add(new LabelWidget
			{
				Text = LanguageControl.Get(fName, 5),
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 6), LanguageControl.Get("GameMode", subsystemGameInfo.WorldSettings.GameMode.ToString()) + ", " + LanguageControl.Get("EnvironmentBehaviorMode", subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode.ToString()));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 7), StringsManager.GetString("TerrainGenerationMode." + subsystemGameInfo.WorldSettings.TerrainGenerationMode.ToString() + ".Name"));
			string seed = subsystemGameInfo.WorldSettings.Seed;
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 8), (!string.IsNullOrEmpty(seed)) ? seed : LanguageControl.Get(fName, 9));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 10), WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.SeaLevelOffset));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 11), WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.TemperatureOffset));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 12), WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.HumidityOffset));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 13), subsystemGameInfo.WorldSettings.BiomeSize.ToString() + "x");
			int num = 0;
			for (int i = 0; i < ComponentFurnitureInventory.maxDesign; i++)
			{
				if (subsystemFurnitureBlockBehavior.GetDesign(i) != null)
				{
					num++;
				}
			}
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 14), $"{num}/{ComponentFurnitureInventory.maxDesign}");
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 15), string.IsNullOrEmpty(subsystemGameInfo.WorldSettings.OriginalSerializationVersion) ? LanguageControl.Get(fName, 16) : subsystemGameInfo.WorldSettings.OriginalSerializationVersion);
			stackPanelWidget.Children.Add(new LabelWidget
			{
				Text = LanguageControl.Get(fName, 17),
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 18), playerData.Name);
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 19), playerData.PlayerClass.ToString());
			string value = (playerData.FirstSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.FirstSpawnTime) / 1200.0).ToString("N1") + LanguageControl.Get(fName, 20)) : LanguageControl.Get(fName, 21);
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 22), value);
			string value2 = (playerData.LastSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.LastSpawnTime) / 1200.0).ToString("N1") + LanguageControl.Get(fName, 23)) : LanguageControl.Get(fName, 24);
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 25), value2);
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 26), MathUtils.Max(playerData.SpawnsCount - 1, 0).ToString("N0") + LanguageControl.Get(fName, 27));
			AddStat(stackPanelWidget, LanguageControl.Get(fName, 28), string.Format(LanguageControl.Get(fName, 29), ((int)MathUtils.Floor(playerStats.HighestLevel)).ToString("N0")));
			if (componentPlayer != null)
			{
				Vector3 position = componentPlayer.ComponentBody.Position;
				if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative)
				{
					AddStat(stackPanelWidget, LanguageControl.Get(fName, 30), string.Format(LanguageControl.Get(fName, 31), $"{position.X:0}", $"{position.Z:0}", $"{position.Y:0}"));
				}
				else
				{
					AddStat(stackPanelWidget, LanguageControl.Get(fName, 30), string.Format(LanguageControl.Get(fName, 32), LanguageControl.Get("GameMode", subsystemGameInfo.WorldSettings.GameMode.ToString())));
				}
			}
			if (string.CompareOrdinal(subsystemGameInfo.WorldSettings.OriginalSerializationVersion, "1.29") > 0)
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 33),
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 34), playerStats.PlayerKills.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 35), playerStats.LandCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 36), playerStats.WaterCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 37), playerStats.AirCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 38), playerStats.MeleeAttacks.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 39), playerStats.MeleeHits.ToString("N0"), $"({((playerStats.MeleeHits == 0L) ? 0.0 : ((double)playerStats.MeleeHits / (double)playerStats.MeleeAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 40), playerStats.RangedAttacks.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 41), playerStats.RangedHits.ToString("N0"), $"({((playerStats.RangedHits == 0L) ? 0.0 : ((double)playerStats.RangedHits / (double)playerStats.RangedAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 42), playerStats.HitsReceived.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 43),
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 44), playerStats.BlocksDug.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 45), playerStats.BlocksPlaced.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 46), playerStats.BlocksInteracted.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 47), playerStats.ItemsCrafted.ToString("N0"));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 48), playerStats.FurnitureItemsMade.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 49),
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 50), FormatDistance(playerStats.DistanceTravelled));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 51), FormatDistance(playerStats.DistanceWalked), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceWalked / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 52), FormatDistance(playerStats.DistanceFallen), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFallen / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 53), FormatDistance(playerStats.DistanceClimbed), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceClimbed / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 54), FormatDistance(playerStats.DistanceFlown), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFlown / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 55), FormatDistance(playerStats.DistanceSwam), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceSwam / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 56), FormatDistance(playerStats.DistanceRidden), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceRidden / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 57), FormatDistance(playerStats.LowestAltitude));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 58), FormatDistance(playerStats.HighestAltitude));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 59), playerStats.DeepestDive.ToString("N1") + "m");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 60), playerStats.Jumps.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 61),
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 62), (playerStats.TotalHealthLost * 100.0).ToString("N0") + "%");
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 63), playerStats.FoodItemsEaten.ToString("N0") + LanguageControl.Get(fName, 64));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 65), playerStats.TimesWentToSleep.ToString("N0") + LanguageControl.Get(fName, 66));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 67), (playerStats.TimeSlept / 1200.0).ToString("N1") + LanguageControl.Get(fName, 68));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 69), playerStats.TimesWasSick.ToString("N0") + LanguageControl.Get(fName, 66));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 70), playerStats.TimesPuked.ToString("N0") + LanguageControl.Get(fName, 66));
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 71), playerStats.TimesHadFlu.ToString("N0") + LanguageControl.Get(fName, 66));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 72),
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 73), playerStats.StruckByLightning.ToString("N0") + LanguageControl.Get(fName, 66));
				GameMode easiestModeUsed = playerStats.EasiestModeUsed;
				AddStat(stackPanelWidget, LanguageControl.Get(fName, 74), LanguageControl.Get("GameMode", easiestModeUsed.ToString()));
				if (playerStats.DeathRecords.Count > 0)
				{
					stackPanelWidget.Children.Add(new LabelWidget
					{
						Text = LanguageControl.Get(fName, 75),
						Font = font,
						HorizontalAlignment = WidgetAlignment.Center,
						Margin = new Vector2(0f, 10f),
						Color = white
					});
					foreach (PlayerStats.DeathRecord deathRecord in playerStats.DeathRecords)
					{
						float num2 = (float)MathUtils.Remainder(deathRecord.Day, 1.0);
						string arg = (!(num2 < 0.2f) && !(num2 >= 0.8f)) ? ((!(num2 >= 0.7f)) ? ((!(num2 >= 0.5f)) ? LanguageControl.Get(fName, 76) : LanguageControl.Get(fName, 77)) : LanguageControl.Get(fName, 78)) : LanguageControl.Get(fName, 79);
						AddStat(stackPanelWidget, string.Format(LanguageControl.Get(fName, 80), MathUtils.Floor(deathRecord.Day) + 1.0, arg), "", deathRecord.Cause);
					}
				}
			}
			else
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = LanguageControl.Get(fName, 81),
					WordWrap = true,
					Font = font2,
					HorizontalAlignment = WidgetAlignment.Center,
					TextAnchor = TextAnchor.HorizontalCenter,
					Margin = new Vector2(20f, 10f),
					Color = white
				});
			}
		}

		public override void Update()
		{
			if (Children.Find<ButtonWidget>("More").IsClicked)
			{
				List<Tuple<string, Action>> list = new List<Tuple<string, Action>>();
				if (m_adventureRestartExists && GameManager.WorldInfo.WorldSettings.GameMode == GameMode.Adventure)
				{
					list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 82), delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog(LanguageControl.Get(fName, 83), LanguageControl.Get(fName, 84), LanguageControl.Get("Usual", "yes"), LanguageControl.Get("Usual", "no"), delegate (MessageDialogButton result)
						{
							if (result == MessageDialogButton.Button1)
							{
								ScreensManager.SwitchScreen("GameLoading", GameManager.WorldInfo, "AdventureRestart");
							}
						}));
					}));
				}
				if (GetRateableItems().FirstOrDefault() != null && UserManager.ActiveUser != null)
				{
					list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 85), delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new ListSelectionDialog(LanguageControl.Get(fName, 86), GetRateableItems(), 60f, (object o) => ((ActiveExternalContentInfo)o).DisplayName, delegate (object o)
						{
							ActiveExternalContentInfo activeExternalContentInfo = (ActiveExternalContentInfo)o;
							DialogsManager.ShowDialog(base.ParentWidget, new RateCommunityContentDialog(activeExternalContentInfo.Address, activeExternalContentInfo.DisplayName, UserManager.ActiveUser.UniqueId));
						}));
					}));
				}
				list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 87), delegate
				{
					ScreensManager.SwitchScreen("Players", m_componentPlayer.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true));
				}));
				list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 88), delegate
				{
					ScreensManager.SwitchScreen("Settings");
				}));
				list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 89), delegate
				{
					ScreensManager.SwitchScreen("Help");
				}));
				if ((base.Input.Devices & (WidgetInputDevice.Keyboard | WidgetInputDevice.Mouse)) != 0)
				{
					list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 90), delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new KeyboardHelpDialog());
					}));
				}
				if ((base.Input.Devices & WidgetInputDevice.Gamepads) != 0)
				{
					list.Add(new Tuple<string, Action>(LanguageControl.Get(fName, 91), delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new GamepadHelpDialog());
					}));
				}
				ListSelectionDialog dialog = new ListSelectionDialog(LanguageControl.Get(fName, 92), list, 60f, (object t) => ((Tuple<string, Action>)t).Item1, delegate (object t)
				{
					((Tuple<string, Action>)t).Item2();
				});
				DialogsManager.ShowDialog(base.ParentWidget, dialog);
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("Resume").IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			if (Children.Find<ButtonWidget>("Quit").IsClicked)
			{
				DialogsManager.HideDialog(this);
				GameManager.SaveProject(waitForCompletion: true, showErrorDialog: true);
				GameManager.DisposeProject();
				ScreensManager.SwitchScreen("MainMenu");
			}
		}

		public IEnumerable<ActiveExternalContentInfo> GetRateableItems()
		{
			if (GameManager.Project != null && UserManager.ActiveUser != null)
			{
				SubsystemGameInfo subsystemGameInfo = GameManager.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
				foreach (ActiveExternalContentInfo item in subsystemGameInfo.GetActiveExternalContent())
				{
					if (!CommunityContentManager.IsContentRated(item.Address, UserManager.ActiveUser.UniqueId))
					{
						yield return item;
					}
				}
			}
		}

		public static string FormatDistance(double value)
		{
			if (value < 1000.0)
			{
				return $"{value:0}m";
			}
			return $"{value / 1000.0:N2}km";
		}

		public void AddStat(ContainerWidget containerWidget, string title, string value1, string value2 = "")
		{
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			Color white = Color.White;
			Color gray = Color.Gray;
			containerWidget.Children.Add(new UniformSpacingPanelWidget
			{
				Direction = LayoutDirection.Horizontal,
				HorizontalAlignment = WidgetAlignment.Center,
				Children =
				{
					(Widget)new LabelWidget
					{
						Text = title + ":",
						HorizontalAlignment = WidgetAlignment.Far,
						Font = font,
						Color = gray,
						Margin = new Vector2(5f, 1f)
					},
					(Widget)new StackPanelWidget
					{
						Direction = LayoutDirection.Horizontal,
						HorizontalAlignment = WidgetAlignment.Near,
						Children =
						{
							(Widget)new LabelWidget
							{
								Text = value1,
								Font = font,
								Color = white,
								Margin = new Vector2(5f, 1f)
							},
							(Widget)new LabelWidget
							{
								Text = value2,
								Font = font,
								Color = gray,
								Margin = new Vector2(5f, 1f)
							}
						}
					}
				}
			});
		}
	}
}
