using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemHammerBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemFurnitureBlockBehavior m_subsystemFurnitureBlockBehavior;

		public override int[] HandledBlocks => new int[0];

		public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
		{
			TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
			if (terrainRaycastResult.HasValue)
			{
				m_subsystemFurnitureBlockBehavior.ScanDesign(terrainRaycastResult.Value.CellFace, ray.Direction, componentMiner);
				return true;
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemFurnitureBlockBehavior = base.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(throwOnError: true);
		}
	}
}
