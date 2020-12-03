using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentUdder : Component
	{
		public SubsystemGameInfo m_subsystemGameInfo;

		public ComponentCreature m_componentCreature;

		public float m_milkRegenerationTime;

		public double m_lastMilkingTime;

		public bool HasMilk
		{
			get
			{
				if (!(m_lastMilkingTime < 0.0))
				{
					return m_subsystemGameInfo.TotalElapsedGameTime - m_lastMilkingTime >= (double)m_milkRegenerationTime;
				}
				return true;
			}
		}

		public bool Milk(ComponentMiner milker)
		{
			if (milker != null)
			{
				base.Entity.FindComponent<ComponentHerdBehavior>()?.CallNearbyCreaturesHelp(milker.ComponentCreature, 20f, 20f, isPersistent: true);
			}
			if (HasMilk)
			{
				m_componentCreature.ComponentCreatureSounds.PlayIdleSound(skipIfRecentlyPlayed: false);
				m_lastMilkingTime = m_subsystemGameInfo.TotalElapsedGameTime;
				return true;
			}
			m_componentCreature.ComponentCreatureSounds.PlayPainSound();
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_milkRegenerationTime = valuesDictionary.GetValue<float>("MilkRegenerationTime");
			m_lastMilkingTime = valuesDictionary.GetValue<double>("LastMilkingTime");
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("LastMilkingTime", m_lastMilkingTime);
		}
	}
}
