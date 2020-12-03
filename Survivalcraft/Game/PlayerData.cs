using Engine;
using GameEntitySystem;
using System;
using System.Globalization;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class PlayerData : IDisposable
	{
		public enum SpawnMode
		{
			InitialIntro,
			InitialNoIntro,
			Respawn
		}
		public static string fName = "PlayerData";
		public Project m_project;

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemSky m_subsystemSky;

		public GameWidget m_gameWidget;

		public StateMachine m_stateMachine = new StateMachine();

		public PlayerClass m_playerClass;

		public string m_name;

		public SpawnMode m_spawnMode;

		public double? m_playerDeathTime;

		public double m_terrainWaitStartTime;

		public SpawnDialog m_spawnDialog;

		public float m_progress;
		public int PlayerIndex
		{
			get;
			set;
		}

		public SubsystemGameWidgets SubsystemGameWidgets
		{
			get;
			set;
		}

		public SubsystemPlayers SubsystemPlayers
		{
			get;
			set;
		}

		public ComponentPlayer ComponentPlayer
		{
			get;
			set;
		}

		public GameWidget GameWidget
		{
			get
			{
				if (m_gameWidget == null)
				{
					foreach (GameWidget gameWidget in SubsystemGameWidgets.GameWidgets)
					{
						if (gameWidget.PlayerData == this)
						{
							m_gameWidget = gameWidget;
							break;
						}
					}
					if (m_gameWidget == null)
					{
						throw new InvalidOperationException(LanguageControl.Get(fName,11));
					}
				}
				return m_gameWidget;
			}
		}

		public Vector3 SpawnPosition
		{
			get;
			set;
		}

		public double FirstSpawnTime
		{
			get;
			set;
		}

		public double LastSpawnTime
		{
			get;
			set;
		}

		public int SpawnsCount
		{
			get;
			set;
		}

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				if (value != m_name)
				{
					m_name = value;
					IsDefaultName = false;
				}
			}
		}

		public bool IsDefaultName
		{
			get;
			set;
		}

		public PlayerClass PlayerClass
		{
			get
			{
				return m_playerClass;
			}
			set
			{
				if (SubsystemPlayers.PlayersData.Contains(this))
				{
					throw new InvalidOperationException(LanguageControl.Get(fName, 1));
				}
				m_playerClass = value;
			}
		}

		public float Level
		{
			get;
			set;
		}

		public string CharacterSkinName
		{
			get;
			set;
		}

		public WidgetInputDevice InputDevice
		{
			get;
			set;
		}

		public bool IsReadyForPlaying
		{
			get
			{
				if (!(m_stateMachine.CurrentState == "Playing"))
				{
					return m_stateMachine.CurrentState == "PlayerDead";
				}
				return true;
			}
		}

		public PlayerData(Project project)
		{
			m_project = project;
			SubsystemPlayers = project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			SubsystemGameWidgets = project.FindSubsystem<SubsystemGameWidgets>(throwOnError: true);
			m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemSky = project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_playerClass = PlayerClass.Male;
			Level = 1f;
			FirstSpawnTime = -1.0;
			LastSpawnTime = -1.0;
			RandomizeCharacterSkin();
			ResetName();
			InputDevice = WidgetInputDevice.None;
			m_stateMachine.AddState("FirstUpdate", null, delegate
			{
				if (ComponentPlayer != null)
				{
					UpdateSpawnDialog(string.Format(LanguageControl.Get(fName,4),Name, MathUtils.Floor(Level)), null, 0f, resetProgress: true);
					m_stateMachine.TransitionTo("WaitForTerrain");
				}
				else
				{
					m_stateMachine.TransitionTo("PrepareSpawn");
				}
			}, null);
			m_stateMachine.AddState("PrepareSpawn", delegate
			{
				if (SpawnPosition == Vector3.Zero)
				{
					if (SubsystemPlayers.GlobalSpawnPosition == Vector3.Zero)
					{
						PlayerData playerData = SubsystemPlayers.PlayersData.FirstOrDefault((PlayerData pd) => pd.SpawnPosition != Vector3.Zero);
						if (playerData != null)
						{
							if (playerData.ComponentPlayer != null)
							{
								SpawnPosition = playerData.ComponentPlayer.ComponentBody.Position;
								m_spawnMode = SpawnMode.InitialNoIntro;
							}
							else
							{
								SpawnPosition = playerData.SpawnPosition;
								m_spawnMode = SpawnMode.InitialNoIntro;
							}
						}
						else
						{
							SpawnPosition = m_subsystemTerrain.TerrainContentsGenerator.FindCoarseSpawnPosition();
							m_spawnMode = SpawnMode.InitialIntro;
						}
						SubsystemPlayers.GlobalSpawnPosition = SpawnPosition;
					}
					else
					{
						SpawnPosition = SubsystemPlayers.GlobalSpawnPosition;
						m_spawnMode = SpawnMode.InitialNoIntro;
					}
				}
				else
				{
					m_spawnMode = SpawnMode.Respawn;
				}
				if (m_spawnMode == SpawnMode.Respawn)
				{
					UpdateSpawnDialog(string.Format(LanguageControl.Get(fName, 2),Name,MathUtils.Floor(Level)), LanguageControl.Get(fName, 3), 0f, resetProgress: true);
				}
				else
				{
					UpdateSpawnDialog(string.Format(LanguageControl.Get(fName, 4), Name, MathUtils.Floor(Level)), null, 0f, resetProgress: true);
				}
				m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(PlayerIndex, SpawnPosition.XZ, 0f, 64f);
				m_terrainWaitStartTime = Time.FrameStartTime;
			}, delegate
			{
				if (Time.PeriodicEvent(0.1, 0.0))
				{
					float updateProgress2 = m_subsystemTerrain.TerrainUpdater.GetUpdateProgress(PlayerIndex, 0f, 64f);
					UpdateSpawnDialog(null, null, 0.5f * updateProgress2, resetProgress: false);
					if (!(updateProgress2 < 1f) || !(Time.FrameStartTime - m_terrainWaitStartTime < 15.0))
					{
						switch (m_spawnMode)
						{
						case SpawnMode.InitialIntro:
							SpawnPosition = FindIntroSpawnPosition(SpawnPosition.XZ);
							break;
						case SpawnMode.InitialNoIntro:
							SpawnPosition = FindNoIntroSpawnPosition(SpawnPosition, respawn: false);
							break;
						case SpawnMode.Respawn:
							SpawnPosition = FindNoIntroSpawnPosition(SpawnPosition, respawn: true);
							break;
						default:
							throw new InvalidOperationException(LanguageControl.Get(fName, 5));
						}
						m_stateMachine.TransitionTo("WaitForTerrain");
					}
				}
			}, null);
			m_stateMachine.AddState("WaitForTerrain", delegate
			{
				m_terrainWaitStartTime = Time.FrameStartTime;
				Vector2 center = (ComponentPlayer != null) ? ComponentPlayer.ComponentBody.Position.XZ : SpawnPosition.XZ;
				m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(PlayerIndex, center, MathUtils.Min(m_subsystemSky.VisibilityRange, 64f), 0f);
			}, delegate
			{
				if (Time.PeriodicEvent(0.1, 0.0))
				{
					float updateProgress = m_subsystemTerrain.TerrainUpdater.GetUpdateProgress(PlayerIndex, MathUtils.Min(m_subsystemSky.VisibilityRange, 64f), 0f);
					UpdateSpawnDialog(null, null, 0.5f + 0.5f * updateProgress, resetProgress: false);
					if ((updateProgress >= 1f && Time.FrameStartTime - m_terrainWaitStartTime > 2.0) || Time.FrameStartTime - m_terrainWaitStartTime >= 15.0)
					{
						if (ComponentPlayer == null)
						{
							SpawnPlayer(SpawnPosition, m_spawnMode);
						}
						m_stateMachine.TransitionTo("Playing");
					}
				}
			}, null);
			m_stateMachine.AddState("Playing", delegate
			{
				HideSpawnDialog();
			}, delegate
			{
				if (ComponentPlayer == null)
				{
					m_stateMachine.TransitionTo("PrepareSpawn");
				}
				else if (m_playerDeathTime.HasValue)
				{
					m_stateMachine.TransitionTo("PlayerDead");
				}
				else if (ComponentPlayer.ComponentHealth.Health <= 0f)
				{
					m_playerDeathTime = Time.RealTime;
				}
			}, null);
			m_stateMachine.AddState("PlayerDead", delegate
			{
				GameWidget.ActiveCamera = GameWidget.FindCamera<DeathCamera>();
				if (ComponentPlayer != null)
				{
					string text = ComponentPlayer.ComponentHealth.CauseOfDeath;
					if (string.IsNullOrEmpty(text))
					{
						text = LanguageControl.Get(fName, 12);
					}
					string arg = string.Format(LanguageControl.Get(fName, 13),text);
					if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
					{
						ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(fName, 6), string.Format(LanguageControl.Get(fName, 7),arg,LanguageControl.Get("GameMode", m_subsystemGameInfo.WorldSettings.GameMode.ToString())), 30f, 1.5f);
					}
					else if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure && !m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed)
					{
						ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(fName, 6),string.Format( LanguageControl.Get(fName,8),arg), 30f, 1.5f);
					}
					else
					{
						ComponentPlayer.ComponentGui.DisplayLargeMessage(LanguageControl.Get(fName, 6), string.Format(LanguageControl.Get(fName, 9), arg), 30f, 1.5f);
					}
				}
				Level = MathUtils.Max(MathUtils.Floor(Level / 2f), 1f);
			}, delegate
			{
				if (ComponentPlayer == null)
				{
					m_stateMachine.TransitionTo("PrepareSpawn");
				}
				else if (Time.RealTime - m_playerDeathTime.Value > 1.5 && !DialogsManager.HasDialogs(ComponentPlayer.GuiWidget) && ComponentPlayer.GameWidget.Input.Any)
				{
					if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
					{
						DialogsManager.ShowDialog(ComponentPlayer.GuiWidget, new GameMenuDialog(ComponentPlayer));
					}
					else if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure && !m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed)
					{
						ScreensManager.SwitchScreen("GameLoading", GameManager.WorldInfo, "AdventureRestart");
					}
					else
					{
						m_project.RemoveEntity(ComponentPlayer.Entity, disposeEntity: true);
					}
				}
			}, null);
			m_stateMachine.TransitionTo("FirstUpdate");
		}

		public void Dispose()
		{
			HideSpawnDialog();
		}

		public void RandomizeCharacterSkin()
		{
			Random random = new Random();
			CharacterSkinsManager.UpdateCharacterSkinsList();
			string[] array = CharacterSkinsManager.CharacterSkinsNames.Where((string n) => CharacterSkinsManager.IsBuiltIn(n) && CharacterSkinsManager.GetPlayerClass(n) == m_playerClass).ToArray();
			string[] second = SubsystemPlayers.PlayersData.Select((PlayerData pd) => pd.CharacterSkinName).ToArray();
			string[] array2 = array.Except(second).ToArray();
			if (array2.Length != 0)
			{
				CharacterSkinName = array2[random.Int(0, array2.Length - 1)];
			}
			else
			{
				CharacterSkinName = array[random.Int(0, array.Length - 1)];
			}
		}

		public void ResetName()
		{
			m_name = CharacterSkinsManager.GetDisplayName(CharacterSkinName);
			IsDefaultName = true;
		}

		public static bool VerifyName(string name)
		{
			if (name.Length < 2)
			{
				return false;
			}

			return true;
		}

		public void Update()
		{
			m_stateMachine.Update();
		}

		public void Load(ValuesDictionary valuesDictionary)
		{
			SpawnPosition = valuesDictionary.GetValue("SpawnPosition", Vector3.Zero);
			FirstSpawnTime = valuesDictionary.GetValue("FirstSpawnTime", 0.0);
			LastSpawnTime = valuesDictionary.GetValue("LastSpawnTime", 0.0);
			SpawnsCount = valuesDictionary.GetValue("SpawnsCount", 0);
			Name = valuesDictionary.GetValue("Name", "Walter");
			PlayerClass = valuesDictionary.GetValue("PlayerClass", PlayerClass.Male);
			Level = valuesDictionary.GetValue("Level", 1f);
			CharacterSkinName = valuesDictionary.GetValue("CharacterSkinName", CharacterSkinsManager.CharacterSkinsNames[0]);
			InputDevice = valuesDictionary.GetValue("InputDevice", InputDevice);
		}

		public void Save(ValuesDictionary valuesDictionary)
		{
			valuesDictionary.SetValue("SpawnPosition", SpawnPosition);
			valuesDictionary.SetValue("FirstSpawnTime", FirstSpawnTime);
			valuesDictionary.SetValue("LastSpawnTime", LastSpawnTime);
			valuesDictionary.SetValue("SpawnsCount", SpawnsCount);
			valuesDictionary.SetValue("Name", Name);
			valuesDictionary.SetValue("PlayerClass", PlayerClass);
			valuesDictionary.SetValue("Level", Level);
			valuesDictionary.SetValue("CharacterSkinName", CharacterSkinName);
			valuesDictionary.SetValue("InputDevice", InputDevice);
		}

		public void OnEntityAdded(Entity entity)
		{
			ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>();
			if (componentPlayer != null && componentPlayer.PlayerData == this)
			{
				if (ComponentPlayer != null)
				{
					throw new InvalidOperationException(string.Format(LanguageControl.Get(fName, 10), PlayerIndex));
				}
				ComponentPlayer = componentPlayer;
				GameWidget.ActiveCamera = GameWidget.FindCamera<FppCamera>();
				GameWidget.Target = componentPlayer;
				if (FirstSpawnTime < 0.0)
				{
					FirstSpawnTime = m_subsystemGameInfo.TotalElapsedGameTime;
				}
			}
		}

		public void OnEntityRemoved(Entity entity)
		{
			if (ComponentPlayer != null && entity == ComponentPlayer.Entity)
			{
				ComponentPlayer = null;
				m_playerDeathTime = null;
			}
		}

		public Vector3 FindIntroSpawnPosition(Vector2 desiredSpawnPosition)
		{
			Vector2 vector = Vector2.Zero;
			float num = float.MinValue;
			for (int i = -30; i <= 30; i += 2)
			{
				for (int j = -30; j <= 30; j += 2)
				{
					int num2 = Terrain.ToCell(desiredSpawnPosition.X) + i;
					int num3 = Terrain.ToCell(desiredSpawnPosition.Y) + j;
					float num4 = ScoreIntroSpawnPosition(desiredSpawnPosition, num2, num3);
					if (num4 > num)
					{
						num = num4;
						vector = new Vector2(num2, num3);
					}
				}
			}
			float num5 = m_subsystemTerrain.Terrain.CalculateTopmostCellHeight(Terrain.ToCell(vector.X), Terrain.ToCell(vector.Y)) + 1;
			return new Vector3(vector.X + 0.5f, num5 + 0.01f, vector.Y + 0.5f);
		}

		public Vector3 FindNoIntroSpawnPosition(Vector3 desiredSpawnPosition, bool respawn)
		{
			Vector3 vector = Vector3.Zero;
			float num = float.MinValue;
			for (int i = -8; i <= 8; i++)
			{
				for (int j = -8; j <= 8; j++)
				{
					for (int k = -8; k <= 8; k++)
					{
						int num2 = Terrain.ToCell(desiredSpawnPosition.X) + i;
						int num3 = Terrain.ToCell(desiredSpawnPosition.Y) + j;
						int num4 = Terrain.ToCell(desiredSpawnPosition.Z) + k;
						float num5 = ScoreNoIntroSpawnPosition(desiredSpawnPosition, num2, num3, num4);
						if (num5 > num)
						{
							num = num5;
							vector = new Vector3(num2, num3, num4);
						}
					}
				}
			}
			return new Vector3(vector.X + 0.5f, vector.Y + 0.01f, vector.Z + 0.5f);
		}

		public float ScoreIntroSpawnPosition(Vector2 desiredSpawnPosition, int x, int z)
		{
			float num = -0.01f * Vector2.Distance(new Vector2(x, z), desiredSpawnPosition);
			int num2 = m_subsystemTerrain.Terrain.CalculateTopmostCellHeight(x, z);
			if (num2 < 64 || num2 > 85)
			{
				num -= 5f;
			}
			if (m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) < 8)
			{
				num -= 5f;
			}
			int cellContents = m_subsystemTerrain.Terrain.GetCellContents(x, num2, z);
			if (BlocksManager.Blocks[cellContents].IsTransparent)
			{
				num -= 5f;
			}
			for (int i = x - 1; i <= x + 1; i++)
			{
				for (int j = z - 1; j <= z + 1; j++)
				{
					if (m_subsystemTerrain.Terrain.GetCellContents(i, num2 + 2, j) != 0)
					{
						num -= 1f;
					}
				}
			}
			Vector2 vector = ComponentIntro.FindOceanDirection(m_subsystemTerrain.TerrainContentsGenerator, new Vector2(x, z));
			Vector3 vector2 = new Vector3(x, (float)num2 + 1.5f, z);
			for (int k = -1; k <= 1; k++)
			{
				Vector3 end = vector2 + new Vector3(30f * vector.X, 5f * (float)k, 30f * vector.Y);
				TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(vector2, end, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => Terrain.ExtractContents(value) != 0);
				if (terrainRaycastResult.HasValue)
				{
					CellFace cellFace = terrainRaycastResult.Value.CellFace;
					int cellContents2 = m_subsystemTerrain.Terrain.GetCellContents(cellFace.X, cellFace.Y, cellFace.Z);
					if (cellContents2 != 18 && cellContents2 != 0)
					{
						num -= 2f;
					}
				}
			}
			return num;
		}

		public float ScoreNoIntroSpawnPosition(Vector3 desiredSpawnPosition, int x, int y, int z)
		{
			float num = -0.01f * Vector3.Distance(new Vector3(x, y, z), desiredSpawnPosition);
			if (y < 1 || y >= 255)
			{
				num -= 100f;
			}
			Block obj = BlocksManager.Blocks[m_subsystemTerrain.Terrain.GetCellContents(x, y - 1, z)];
			Block block = BlocksManager.Blocks[m_subsystemTerrain.Terrain.GetCellContents(x, y, z)];
			Block block2 = BlocksManager.Blocks[m_subsystemTerrain.Terrain.GetCellContents(x, y + 1, z)];
			if (obj.IsTransparent)
			{
				num -= 10f;
			}
			if (!obj.IsCollidable)
			{
				num -= 10f;
			}
			if (block.IsCollidable)
			{
				num -= 10f;
			}
			if (block2.IsCollidable)
			{
				num -= 10f;
			}
			foreach (PlayerData playersDatum in SubsystemPlayers.PlayersData)
			{
				if (playersDatum != this && Vector3.DistanceSquared(playersDatum.SpawnPosition, new Vector3(x, y, z)) < (float)MathUtils.Sqr(2))
				{
					num -= 1f;
				}
			}
			return num;
		}

		public bool CheckIsPointInWater(Point3 p)
		{
			bool result = true;
			for (int i = p.X - 1; i < p.X + 1; i++)
			{
				for (int j = p.Z - 1; j < p.Z + 1; j++)
				{
					for (int num = p.Y; num > 0; num--)
					{
						int cellContents = m_subsystemTerrain.Terrain.GetCellContents(p.X, num, p.Z);
						Block block = BlocksManager.Blocks[cellContents];
						if (block.IsCollidable)
						{
							return false;
						}
						if (block is WaterBlock)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		public void SpawnPlayer(Vector3 position, SpawnMode spawnMode)
		{
			ComponentMount componentMount = null;
			if (spawnMode != SpawnMode.Respawn && CheckIsPointInWater(Terrain.ToCell(position)))
			{
				Entity entity = DatabaseManager.CreateEntity(m_project, "Boat", throwIfNotFound: true);
				entity.FindComponent<ComponentBody>(throwOnError: true).Position = position;
				entity.FindComponent<ComponentBody>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.DegToRad(45f));
				componentMount = entity.FindComponent<ComponentMount>(throwOnError: true);
				m_project.AddEntity(entity);
				position.Y += entity.FindComponent<ComponentBody>(throwOnError: true).BoxSize.Y;
			}
			string value = "";
			string value2 = "";
			string value3 = "";
			string value4 = "";
			if (spawnMode != SpawnMode.Respawn)
			{
				if (PlayerClass == PlayerClass.Female)
				{
					if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("2"))
					{
						value = "";
						value2 = MakeClothingValue(37, 2);
						value3 = MakeClothingValue(16, 14);
						value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(27, 0);
					}
					else if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("3"))
					{
						value = MakeClothingValue(31, 0);
						value2 = MakeClothingValue(13, 7) + ";" + MakeClothingValue(5, 0);
						value3 = MakeClothingValue(17, 15);
						value4 = MakeClothingValue(29, 0);
					}
					else if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("4"))
					{
						value = MakeClothingValue(30, 7);
						value2 = MakeClothingValue(14, 6);
						value3 = MakeClothingValue(25, 7);
						value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(8, 0);
					}
					else
					{
						value = MakeClothingValue(30, 12);
						value2 = MakeClothingValue(37, 3) + ";" + MakeClothingValue(1, 3);
						value3 = MakeClothingValue(0, 12);
						value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(29, 0);
					}
				}
				else if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("2"))
				{
					value = "";
					value2 = MakeClothingValue(13, 0) + ";" + MakeClothingValue(5, 0);
					value3 = MakeClothingValue(25, 8);
					value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(29, 0);
				}
				else if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("3"))
				{
					value = MakeClothingValue(32, 0);
					value2 = MakeClothingValue(37, 5);
					value3 = MakeClothingValue(0, 15);
					value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(8, 0);
				}
				else if (CharacterSkinsManager.IsBuiltIn(CharacterSkinName) && CharacterSkinName.Contains("4"))
				{
					value = MakeClothingValue(31, 0);
					value2 = MakeClothingValue(15, 14);
					value3 = MakeClothingValue(0, 0);
					value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(8, 0);
				}
				else
				{
					value = MakeClothingValue(32, 0);
					value2 = MakeClothingValue(37, 0) + ";" + MakeClothingValue(1, 9);
					value3 = MakeClothingValue(0, 12);
					value4 = MakeClothingValue(26, 6) + ";" + MakeClothingValue(29, 0);
				}
			}
			ValuesDictionary overrides = new ValuesDictionary
			{
				{
					"Player",
					new ValuesDictionary
					{
						{
							"PlayerIndex",
							PlayerIndex
						}
					}
				},
				{
					"Intro",
					new ValuesDictionary
					{
						{
							"PlayIntro",
							spawnMode == SpawnMode.InitialIntro
						}
					}
				},
				{
					"Clothing",
					new ValuesDictionary
					{
						{
							"Clothes",
							new ValuesDictionary
							{
								{
									"Feet",
									value4
								},
								{
									"Legs",
									value3
								},
								{
									"Torso",
									value2
								},
								{
									"Head",
									value
								}
							}
						}
					}
				}
			};
			Vector2 v = ComponentIntro.FindOceanDirection(m_subsystemTerrain.TerrainContentsGenerator, position.XZ);
			string entityTemplateName = (PlayerClass == PlayerClass.Male) ? "MalePlayer" : "FemalePlayer";
			Entity entity2 = DatabaseManager.CreateEntity(m_project, entityTemplateName, overrides, throwIfNotFound: true);
			entity2.FindComponent<ComponentBody>(throwOnError: true).Position = position;
			entity2.FindComponent<ComponentBody>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, Vector2.Angle(v, -Vector2.UnitY));
			m_project.AddEntity(entity2);
			if (componentMount != null)
			{
				entity2.FindComponent<ComponentRider>(throwOnError: true).StartMounting(componentMount);
			}
			LastSpawnTime = m_subsystemGameInfo.TotalElapsedGameTime;
			int num = ++SpawnsCount;
		}

		public string GetEntityTemplateName()
		{
			if (PlayerClass != 0)
			{
				return "FemalePlayer";
			}
			return "MalePlayer";
		}

		public void UpdateSpawnDialog(string largeMessage, string smallMessage, float progress, bool resetProgress)
		{
			if (resetProgress)
			{
				m_progress = 0f;
			}
			m_progress = MathUtils.Max(progress, m_progress);
			if (m_spawnDialog == null)
			{
				m_spawnDialog = new SpawnDialog();
				DialogsManager.ShowDialog(GameWidget.GuiWidget, m_spawnDialog);
			}
			if (largeMessage != null)
			{
				m_spawnDialog.LargeMessage = largeMessage;
			}
			if (smallMessage != null)
			{
				m_spawnDialog.SmallMessage = smallMessage;
			}
			m_spawnDialog.Progress = m_progress;
		}

		public void HideSpawnDialog()
		{
			if (m_spawnDialog != null)
			{
				DialogsManager.HideDialog(m_spawnDialog);
				m_spawnDialog = null;
			}
		}

		public static string MakeClothingValue(int index, int color)
		{
			return Terrain.MakeBlockValue(203, 0, ClothingBlock.SetClothingIndex(ClothingBlock.SetClothingColor(0, color), index)).ToString(CultureInfo.InvariantCulture);
		}
	}
}
