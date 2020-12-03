using Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplatesDatabase
{
	public class Database
	{
		private DatabaseObject m_root;

		private ReadOnlyList<DatabaseObjectType> m_databaseObjectTypes;

		private Dictionary<Guid, DatabaseObject> m_databaseObjectsByGuid = new Dictionary<Guid, DatabaseObject>();

		public IList<DatabaseObjectType> DatabaseObjectTypes => m_databaseObjectTypes;

		public DatabaseObject Root => m_root;

		public Database(DatabaseObject root, IEnumerable<DatabaseObjectType> databaseObjectTypes)
		{
			if (!databaseObjectTypes.Contains(root.Type))
			{
				throw new Exception("Database root has invalid database object type.");
			}
			if (root.NestingParent != null)
			{
				throw new Exception("Database root cannot be nested.");
			}
			m_databaseObjectTypes = new ReadOnlyList<DatabaseObjectType>(new List<DatabaseObjectType>(databaseObjectTypes));
			m_root = root;
			m_root.m_database = this;
		}

		public DatabaseObjectType FindDatabaseObjectType(string name, bool throwIfNotFound)
		{
			foreach (DatabaseObjectType databaseObjectType in m_databaseObjectTypes)
			{
				if (databaseObjectType.Name == name)
				{
					return databaseObjectType;
				}
			}
			if (throwIfNotFound)
			{
				throw new Exception($"Required database object type \"{name}\" not found.");
			}
			return null;
		}

		public DatabaseObject FindDatabaseObject(Guid guid, DatabaseObjectType type, bool throwIfNotFound)
		{
			m_databaseObjectsByGuid.TryGetValue(guid, out DatabaseObject value);
			if (value != null)
			{
				if (type != null && value.Type != type)
				{
					throw new InvalidOperationException($"Database object {guid} has invalid type. Expected {type.Name}, found {value.Type.Name}.");
				}
			}
			else if (throwIfNotFound)
			{
				throw new InvalidOperationException($"Required database object {guid} not found.");
			}
			return value;
		}

		public DatabaseObject FindDatabaseObject(string name, DatabaseObjectType type, bool throwIfNotFound)
		{
			return Root.FindExplicitNestedChild(name, type, directChildrenOnly: false, throwIfNotFound);
		}

		public void FindUsedValueTypes(List<Type> typesList)
		{
			foreach (DatabaseObject explicitNestingChild in Root.GetExplicitNestingChildren(null, directChildrenOnly: false))
			{
				if (explicitNestingChild.Value != null && !typesList.Contains(explicitNestingChild.Value.GetType()))
				{
					typesList.Add(explicitNestingChild.Value.GetType());
				}
			}
		}

		public void AddDatabaseObject(DatabaseObject databaseObject, bool checkThatGuidsAreUnique)
		{
			if (databaseObject.m_database != null)
			{
				throw new InvalidOperationException("public error: database object is already in a database.");
			}
			if (!m_databaseObjectTypes.Contains(databaseObject.Type))
			{
				throw new InvalidOperationException($"Database object type \"{databaseObject.Type.Name}\" is not supported by the database.");
			}
			if (checkThatGuidsAreUnique)
			{
				if (databaseObject.Guid != Guid.Empty && m_databaseObjectsByGuid.ContainsKey(databaseObject.Guid))
				{
					throw new InvalidOperationException($"Database object {databaseObject.Guid} is already present in the database.");
				}
				foreach (DatabaseObject explicitNestingChild in databaseObject.GetExplicitNestingChildren(null, directChildrenOnly: false))
				{
					if (explicitNestingChild.Guid != Guid.Empty && m_databaseObjectsByGuid.ContainsKey(explicitNestingChild.Guid))
					{
						throw new InvalidOperationException($"Database object {explicitNestingChild.Guid} is already present in the database.");
					}
				}
			}
			databaseObject.m_database = this;
			if (databaseObject.Guid != Guid.Empty)
			{
				m_databaseObjectsByGuid.Add(databaseObject.Guid, databaseObject);
			}
			foreach (DatabaseObject explicitNestingChild2 in databaseObject.GetExplicitNestingChildren(null, directChildrenOnly: true))
			{
				AddDatabaseObject(explicitNestingChild2, checkThatGuidsAreUnique: false);
			}
		}

		public void RemoveDatabaseObject(DatabaseObject databaseObject)
		{
			if (databaseObject.m_database != this)
			{
				throw new InvalidOperationException("public error: database object is not in the database.");
			}
			databaseObject.m_database = null;
			if (databaseObject.Guid != Guid.Empty && !m_databaseObjectsByGuid.Remove(databaseObject.Guid))
			{
				throw new InvalidOperationException("public error: database object not in dictionary.");
			}
			foreach (DatabaseObject explicitNestingChild in databaseObject.GetExplicitNestingChildren(null, directChildrenOnly: true))
			{
				RemoveDatabaseObject(explicitNestingChild);
			}
		}
	}
}
