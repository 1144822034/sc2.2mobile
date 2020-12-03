using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemRakeBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemParticles m_subsystemParticles;

		public SubsystemAudio m_subsystemAudio;

		public override int[] HandledBlocks => new int[4]
		{
			169,
			219,
			171,
			172
		};

		public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
		{
			TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
			if (terrainRaycastResult.HasValue)
			{
				if (terrainRaycastResult.Value.CellFace.Face == 4)
				{
					int cellValue = m_subsystemTerrain.Terrain.GetCellValue(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z);
					int num = Terrain.ExtractContents(cellValue);
					Block block = BlocksManager.Blocks[num];
					switch (num)
					{
					case 2:
					{
						int value2 = Terrain.ReplaceContents(cellValue, 168);
						m_subsystemTerrain.ChangeCell(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z, value2);
						m_subsystemAudio.PlayRandomSound("Audio/Impacts/Dirt", 0.5f, 0f, new Vector3(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z), 3f, autoDelay: true);
						Vector3 position2 = new Vector3((float)terrainRaycastResult.Value.CellFace.X + 0.5f, (float)terrainRaycastResult.Value.CellFace.Y + 1.25f, (float)terrainRaycastResult.Value.CellFace.Z + 0.5f);
						m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, position2, cellValue, 0.5f));
						break;
					}
					case 8:
					{
						int value = Terrain.ReplaceContents(cellValue, 2);
						m_subsystemTerrain.ChangeCell(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z, value);
						m_subsystemAudio.PlayRandomSound("Audio/Impacts/Plant", 0.5f, 0f, new Vector3(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z), 3f, autoDelay: true);
						Vector3 position = new Vector3((float)terrainRaycastResult.Value.CellFace.X + 0.5f, (float)terrainRaycastResult.Value.CellFace.Y + 1.2f, (float)terrainRaycastResult.Value.CellFace.Z + 0.5f);
						m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, position, cellValue, 0.75f));
						break;
					}
					}
				}
				componentMiner.DamageActiveTool(1);
				return true;
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemParticles = base.Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		}
	}
}
