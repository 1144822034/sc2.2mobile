using System.Collections.Generic;

namespace GameEntitySystem
{
	public class EntityToIdMap
	{
		private Dictionary<Entity, int> m_map;

		public EntityToIdMap(Dictionary<Entity, int> map)
		{
			m_map = map;
		}

		public int FindId(Entity entity)
		{
			if (entity != null && m_map.TryGetValue(entity, out int value))
			{
				return value;
			}
			return 0;
		}

		public int FindId(Component component)
		{
			if (component == null)
			{
				return 0;
			}
			return FindId(component.Entity);
		}
	}
}
