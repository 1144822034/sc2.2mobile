using Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplatesDatabase
{
	public class DatabaseObjectType
	{
		private string m_name;

		private string m_defaultInstanceName;

		private string m_iconName;

		private int m_order;

		private bool m_supportsValue;

		private bool m_mustInherit;

		private int m_nameLengthLimit;

		private bool m_saveStandalone;

		private List<DatabaseObjectType> m_allowedNestingParents;

		private List<DatabaseObjectType> m_allowedInheritanceParents;

		private List<DatabaseObjectType> m_allowedNestingChildren = new List<DatabaseObjectType>();

		private List<DatabaseObjectType> m_allowedInheritanceChildren = new List<DatabaseObjectType>();

		private DatabaseObjectType m_nestedValueType;

		public bool IsInitialized => m_allowedNestingParents != null;

		public string Name => m_name;

		public string DefaultInstanceName => m_defaultInstanceName;

		public string IconName => m_iconName;

		public int Order => m_order;

		public bool SupportsValue => m_supportsValue;

		public bool MustInherit => m_mustInherit;

		public int NameLengthLimit => m_nameLengthLimit;

		public bool SaveStandalone => m_saveStandalone;

		public ReadOnlyList<DatabaseObjectType> AllowedNestingParents => new ReadOnlyList<DatabaseObjectType>(m_allowedNestingParents);

		public ReadOnlyList<DatabaseObjectType> AllowedInheritanceParents => new ReadOnlyList<DatabaseObjectType>(m_allowedInheritanceParents);

		public ReadOnlyList<DatabaseObjectType> AllowedNestingChildren => new ReadOnlyList<DatabaseObjectType>(m_allowedNestingChildren);

		public ReadOnlyList<DatabaseObjectType> AllowedInheritanceChildren => new ReadOnlyList<DatabaseObjectType>(m_allowedInheritanceChildren);

		public DatabaseObjectType NestedValueType => m_nestedValueType;

		public DatabaseObjectType(string name, string defaultInstanceName, string iconName, int order, bool supportsValue, bool mustInherit, int nameLengthLimit, bool saveStandalone)
		{
			m_name = name;
			m_defaultInstanceName = defaultInstanceName;
			m_iconName = iconName;
			m_order = order;
			m_supportsValue = supportsValue;
			m_mustInherit = mustInherit;
			m_nameLengthLimit = nameLengthLimit;
			m_saveStandalone = saveStandalone;
		}

		public void InitializeRelations(IEnumerable<DatabaseObjectType> allowedNestingParents, IEnumerable<DatabaseObjectType> allowedInheritanceParents, DatabaseObjectType nestedValueType)
		{
			if (IsInitialized)
			{
				throw new InvalidOperationException("InitializeRelations of this DatabaseObjectType was already called.");
			}
			m_allowedNestingParents = ((allowedNestingParents != null) ? allowedNestingParents.Distinct().ToList() : new List<DatabaseObjectType>(0));
			m_allowedInheritanceParents = ((allowedInheritanceParents != null) ? allowedInheritanceParents.Distinct().ToList() : new List<DatabaseObjectType>(0));
			m_nestedValueType = nestedValueType;
			foreach (DatabaseObjectType allowedNestingParent in m_allowedNestingParents)
			{
				allowedNestingParent.m_allowedNestingChildren.Add(this);
			}
			foreach (DatabaseObjectType allowedInheritanceParent in allowedInheritanceParents)
			{
				allowedInheritanceParent.m_allowedInheritanceChildren.Add(this);
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
