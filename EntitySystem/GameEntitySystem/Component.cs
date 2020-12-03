using Engine;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace GameEntitySystem
{
	public abstract class Component : IDisposable
	{
		public Entity m_entity;

		public ValuesDictionary m_valuesDictionary;

		public ValuesDictionary ValuesDictionary => m_valuesDictionary;

		public Entity Entity => m_entity;

		public Project Project => m_entity.Project;

		public bool IsAddedToProject => m_entity.IsAddedToProject;

		public virtual IEnumerable<Entity> GetOwnedEntities()
		{
			return ReadOnlyList<Entity>.Empty;
		}

		 public virtual void OnEntityAdded()
		{
		}

		public virtual void OnEntityRemoved()
		{
		}

		public virtual void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
		}

		public virtual void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
		}

		public virtual void Dispose()
		{
		}

		public void Initialize(Entity entity, ValuesDictionary valuesDictionary)
		{
			if (valuesDictionary.DatabaseObject.Type != entity.Project.GameDatabase.MemberComponentTemplateType)
			{
				throw new InvalidOperationException("ValuesDictionary has invalid type.");
			}
			m_entity = entity;
			m_valuesDictionary = valuesDictionary;
		}
	}
}
