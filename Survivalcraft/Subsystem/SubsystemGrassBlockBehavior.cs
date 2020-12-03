using Engine;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemGrassBlockBehavior : SubsystemPollableBlockBehavior, IUpdateable
	{
		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemTime m_subsystemTime;

		public Dictionary<Point3, int> m_toUpdate = new Dictionary<Point3, int>();

		public Random m_random = new Random();

		public override int[] HandledBlocks => new int[1]
		{
			8
		};

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override void OnPoll(int value, int x, int y, int z, int pollPass)
		{
			if (Terrain.ExtractData(value) != 0 || m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != 0)
			{
				return;
			}
			int num = Terrain.ExtractLight(base.SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z));
			if (num == 0)
			{
				m_toUpdate[new Point3(x, y, z)] = Terrain.ReplaceContents(value, 8);
			}
			if (num < 13)
			{
				return;
			}
			for (int i = x - 1; i <= x + 1; i++)
			{
				for (int j = z - 1; j <= z + 1; j++)
				{
					for (int k = y - 2; k <= y + 1; k++)
					{
						int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(i, k, j);
						if (Terrain.ExtractContents(cellValue) != 2)
						{
							continue;
						}
						int cellValue2 = base.SubsystemTerrain.Terrain.GetCellValue(i, k + 1, j);
						if (KillsGrassIfOnTopOfIt(cellValue2) || Terrain.ExtractLight(cellValue2) < 13 || !(m_random.Float(0f, 1f) < 0.1f))
						{
							continue;
						}
						int num2 = Terrain.ReplaceContents(cellValue, 8);
						m_toUpdate[new Point3(i, k, j)] = num2;
						if (Terrain.ExtractContents(cellValue2) == 0)
						{
							int temperature = base.SubsystemTerrain.Terrain.GetTemperature(i, j);
							int humidity = base.SubsystemTerrain.Terrain.GetHumidity(i, j);
							int num3 = PlantsManager.GenerateRandomPlantValue(m_random, num2, temperature, humidity, k + 1);
							if (num3 != 0)
							{
								m_toUpdate[new Point3(i, k + 1, j)] = num3;
							}
						}
					}
				}
			}
		}

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
			if (Terrain.ExtractContents(cellValue) == 61)
			{
				int cellValueFast = base.SubsystemTerrain.Terrain.GetCellValueFast(x, y, z);
				cellValueFast = Terrain.ReplaceData(cellValueFast, 1);
				base.SubsystemTerrain.ChangeCell(x, y, z, cellValueFast);
			}
			else
			{
				int cellValueFast2 = base.SubsystemTerrain.Terrain.GetCellValueFast(x, y, z);
				cellValueFast2 = Terrain.ReplaceData(cellValueFast2, 0);
				base.SubsystemTerrain.ChangeCell(x, y, z, cellValueFast2);
			}
			if (KillsGrassIfOnTopOfIt(cellValue))
			{
				base.SubsystemTerrain.ChangeCell(x, y, z, Terrain.MakeBlockValue(2, 0, 0));
			}
		}

		public override void OnExplosion(int value, int x, int y, int z, float damage)
		{
			if (damage > BlocksManager.Blocks[8].ExplosionResilience * m_random.Float(0f, 1f))
			{
				base.SubsystemTerrain.ChangeCell(x, y, z, Terrain.MakeBlockValue(2, 0, 0));
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			base.Load(valuesDictionary);
		}

		public void Update(float dt)
		{
			if (m_subsystemTime.PeriodicGameTimeEvent(60.0, 0.0))
			{
				foreach (KeyValuePair<Point3, int> item in m_toUpdate)
				{
					if (Terrain.ExtractContents(item.Value) == 8)
					{
						if (base.SubsystemTerrain.Terrain.GetCellContents(item.Key.X, item.Key.Y, item.Key.Z) != 2)
						{
							continue;
						}
					}
					else
					{
						int cellContents = base.SubsystemTerrain.Terrain.GetCellContents(item.Key.X, item.Key.Y - 1, item.Key.Z);
						if ((cellContents != 8 && cellContents != 2) || base.SubsystemTerrain.Terrain.GetCellContents(item.Key.X, item.Key.Y, item.Key.Z) != 0)
						{
							continue;
						}
					}
					base.SubsystemTerrain.ChangeCell(item.Key.X, item.Key.Y, item.Key.Z, item.Value);
				}
				m_toUpdate.Clear();
			}
		}

		public bool KillsGrassIfOnTopOfIt(int value)
		{
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			if (!(block is FluidBlock))
			{
				if (!block.IsFaceTransparent(base.SubsystemTerrain, 5, value))
				{
					return block.IsCollidable;
				}
				return false;
			}
			return true;
		}
	}
}
