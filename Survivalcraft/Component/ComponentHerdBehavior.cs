using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentHerdBehavior : ComponentBehavior, IUpdateable
	{
		public SubsystemCreatureSpawn m_subsystemCreatureSpawn;

		public SubsystemTime m_subsystemTime;

		public ComponentCreature m_componentCreature;

		public ComponentPathfinding m_componentPathfinding;

		public StateMachine m_stateMachine = new StateMachine();

		public float m_dt;

		public float m_importanceLevel;

		public Random m_random = new Random();

		public Vector2 m_look;

		public float m_herdingRange;

		public bool m_autoNearbyCreaturesHelp;

		public string HerdName
		{
			get;
			set;
		}

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override float ImportanceLevel => m_importanceLevel;

		public void CallNearbyCreaturesHelp(ComponentCreature target, float maxRange, float maxChaseTime, bool isPersistent)
		{
			Vector3 position = target.ComponentBody.Position;
			foreach (ComponentCreature creature in m_subsystemCreatureSpawn.Creatures)
			{
				if (Vector3.DistanceSquared(position, creature.ComponentBody.Position) < 256f)
				{
					ComponentHerdBehavior componentHerdBehavior = creature.Entity.FindComponent<ComponentHerdBehavior>();
					if (componentHerdBehavior != null && componentHerdBehavior.HerdName == HerdName && componentHerdBehavior.m_autoNearbyCreaturesHelp)
					{
						ComponentChaseBehavior componentChaseBehavior = creature.Entity.FindComponent<ComponentChaseBehavior>();
						if (componentChaseBehavior != null && componentChaseBehavior.Target == null)
						{
							componentChaseBehavior.Attack(target, maxRange, maxChaseTime, isPersistent);
						}
					}
				}
			}
		}

		public Vector3? FindHerdCenter()
		{
			if (string.IsNullOrEmpty(HerdName))
			{
				return null;
			}
			Vector3 position = m_componentCreature.ComponentBody.Position;
			int num = 0;
			Vector3 zero = Vector3.Zero;
			foreach (ComponentCreature creature in m_subsystemCreatureSpawn.Creatures)
			{
				if (creature.ComponentHealth.Health > 0f)
				{
					ComponentHerdBehavior componentHerdBehavior = creature.Entity.FindComponent<ComponentHerdBehavior>();
					if (componentHerdBehavior != null && componentHerdBehavior.HerdName == HerdName)
					{
						Vector3 position2 = creature.ComponentBody.Position;
						if (Vector3.DistanceSquared(position, position2) < m_herdingRange * m_herdingRange)
						{
							zero += position2;
							num++;
						}
					}
				}
			}
			if (num > 0)
			{
				return zero / num;
			}
			return null;
		}

		public void Update(float dt)
		{
			if (string.IsNullOrEmpty(m_stateMachine.CurrentState) || !IsActive)
			{
				m_stateMachine.TransitionTo("Inactive");
			}
			m_dt = dt;
			m_stateMachine.Update();
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemCreatureSpawn = base.Project.FindSubsystem<SubsystemCreatureSpawn>(throwOnError: true);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(throwOnError: true);
			m_componentPathfinding = base.Entity.FindComponent<ComponentPathfinding>(throwOnError: true);
			HerdName = valuesDictionary.GetValue<string>("HerdName");
			m_herdingRange = valuesDictionary.GetValue<float>("HerdingRange");
			m_autoNearbyCreaturesHelp = valuesDictionary.GetValue<bool>("AutoNearbyCreaturesHelp");
			m_componentCreature.ComponentHealth.Attacked += delegate(ComponentCreature attacker)
			{
				CallNearbyCreaturesHelp(attacker, 20f, 30f, isPersistent: false);
			};
			m_stateMachine.AddState("Inactive", null, delegate
			{
				if (m_subsystemTime.PeriodicGameTimeEvent(1.0, 1f * ((float)(GetHashCode() % 256) / 256f)))
				{
					Vector3? vector2 = FindHerdCenter();
					if (vector2.HasValue)
					{
						float num = Vector3.Distance(vector2.Value, m_componentCreature.ComponentBody.Position);
						if (num > 10f)
						{
							m_importanceLevel = 1f;
						}
						if (num > 12f)
						{
							m_importanceLevel = 3f;
						}
						if (num > 16f)
						{
							m_importanceLevel = 50f;
						}
						if (num > 20f)
						{
							m_importanceLevel = 250f;
						}
					}
				}
				if (IsActive)
				{
					m_stateMachine.TransitionTo("Herd");
				}
			}, null);
			m_stateMachine.AddState("Stuck", delegate
			{
				m_stateMachine.TransitionTo("Herd");
				if (m_random.Bool(0.5f))
				{
					m_componentCreature.ComponentCreatureSounds.PlayIdleSound(skipIfRecentlyPlayed: false);
					m_importanceLevel = 0f;
				}
			}, null, null);
			m_stateMachine.AddState("Herd", delegate
			{
				Vector3? vector = FindHerdCenter();
				if (vector.HasValue && Vector3.Distance(m_componentCreature.ComponentBody.Position, vector.Value) > 6f)
				{
					float speed = (m_importanceLevel > 10f) ? m_random.Float(0.9f, 1f) : m_random.Float(0.25f, 0.35f);
					int maxPathfindingPositions = (m_importanceLevel > 200f) ? 100 : 0;
					m_componentPathfinding.SetDestination(vector.Value, speed, 7f, maxPathfindingPositions, useRandomMovements: false, ignoreHeightDifference: true, raycastDestination: false, null);
				}
				else
				{
					m_importanceLevel = 0f;
				}
			}, delegate
			{
				m_componentCreature.ComponentLocomotion.LookOrder = m_look - m_componentCreature.ComponentLocomotion.LookAngles;
				if (m_componentPathfinding.IsStuck)
				{
					m_stateMachine.TransitionTo("Stuck");
				}
				if (!m_componentPathfinding.Destination.HasValue)
				{
					m_importanceLevel = 0f;
				}
				if (m_random.Float(0f, 1f) < 0.05f * m_dt)
				{
					m_componentCreature.ComponentCreatureSounds.PlayIdleSound(skipIfRecentlyPlayed: false);
				}
				if (m_random.Float(0f, 1f) < 1.5f * m_dt)
				{
					m_look = new Vector2(MathUtils.DegToRad(45f) * m_random.Float(-1f, 1f), MathUtils.DegToRad(10f) * m_random.Float(-1f, 1f));
				}
			}, null);
		}
	}
}
