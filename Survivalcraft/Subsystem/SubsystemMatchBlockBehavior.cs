using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemMatchBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemAudio m_subsystemAudio;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;

		public SubsystemExplosivesBlockBehavior m_subsystemExplosivesBlockBehavior;

		public Random m_random = new Random();

		public override int[] HandledBlocks => new int[1]
		{
			108
		};

		public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
		{
			object obj = componentMiner.Raycast(ray, RaycastMode.Digging);
			if (obj is TerrainRaycastResult)
			{
				CellFace cellFace = ((TerrainRaycastResult)obj).CellFace;
				if (m_subsystemExplosivesBlockBehavior.IgniteFuse(cellFace.X, cellFace.Y, cellFace.Z))
				{
					m_subsystemAudio.PlaySound("Audio/Match", 1f, m_random.Float(-0.1f, 0.1f), ray.Position, 1f, autoDelay: true);
					componentMiner.RemoveActiveTool(1);
					return true;
				}
				if (m_subsystemFireBlockBehavior.SetCellOnFire(cellFace.X, cellFace.Y, cellFace.Z, 1f))
				{
					m_subsystemAudio.PlaySound("Audio/Match", 1f, m_random.Float(-0.1f, 0.1f), ray.Position, 1f, autoDelay: true);
					componentMiner.RemoveActiveTool(1);
					return true;
				}
			}
			else if (obj is BodyRaycastResult)
			{
				ComponentOnFire componentOnFire = ((BodyRaycastResult)obj).ComponentBody.Entity.FindComponent<ComponentOnFire>();
				if (componentOnFire != null)
				{
					if (m_subsystemGameInfo.WorldSettings.GameMode < GameMode.Challenging || m_random.Float(0f, 1f) < 0.33f)
					{
						componentOnFire.SetOnFire(componentMiner.ComponentCreature, m_random.Float(6f, 8f));
					}
					m_subsystemAudio.PlaySound("Audio/Match", 1f, m_random.Float(-0.1f, 0.1f), ray.Position, 1f, autoDelay: true);
					componentMiner.RemoveActiveTool(1);
					return true;
				}
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemFireBlockBehavior = base.Project.FindSubsystem<SubsystemFireBlockBehavior>(throwOnError: true);
			m_subsystemExplosivesBlockBehavior = base.Project.FindSubsystem<SubsystemExplosivesBlockBehavior>(throwOnError: true);
		}
	}
}
