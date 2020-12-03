using System.Collections.Generic;

namespace GameEntitySystem
{
	public class IdToEntityMap
	{
		private Dictionary<int, Entity> m_map;

		public IdToEntityMap(Dictionary<int, Entity> map)
		{
			m_map = map;
		}

		public Entity FindEntity(int id)
		{
			if (m_map.TryGetValue(id, out Entity value))
			{
				return value;
			}
			return null;
		}

		public T FindComponent<T>(int id, string name) where T : Component
		{
			Entity entity = FindEntity(id);
			if (entity != null)
			{
				return entity.FindComponent<T>(name, throwOnError: false);
			}
			return null;
		}
	}
}
