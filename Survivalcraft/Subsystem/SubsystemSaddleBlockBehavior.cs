using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemSaddleBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemAudio m_subsystemAudio;

		public Random m_random = new Random();

		public override int[] HandledBlocks => new int[1]
		{
			158
		};

		public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
		{
			BodyRaycastResult? bodyRaycastResult = componentMiner.Raycast<BodyRaycastResult>(ray, RaycastMode.Interaction);
			if (bodyRaycastResult.HasValue)
			{
				ComponentHealth componentHealth = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentHealth>();
				if (componentHealth == null || componentHealth.Health > 0f)
				{
					string entityTemplateName = bodyRaycastResult.Value.ComponentBody.Entity.ValuesDictionary.DatabaseObject.Name + "_Saddled";
					Entity entity = DatabaseManager.CreateEntity(base.Project, entityTemplateName, throwIfNotFound: false);
					if (entity != null)
					{
						ComponentBody componentBody = entity.FindComponent<ComponentBody>(throwOnError: true);
						componentBody.Position = bodyRaycastResult.Value.ComponentBody.Position;
						componentBody.Rotation = bodyRaycastResult.Value.ComponentBody.Rotation;
						componentBody.Velocity = bodyRaycastResult.Value.ComponentBody.Velocity;
						entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
						base.Project.RemoveEntity(bodyRaycastResult.Value.ComponentBody.Entity, disposeEntity: true);
						base.Project.AddEntity(entity);
						m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, m_random.Float(-0.1f, 0.1f), ray.Position, 1f, autoDelay: true);
						componentMiner.RemoveActiveTool(1);
					}
				}
				return true;
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		}
	}
}
