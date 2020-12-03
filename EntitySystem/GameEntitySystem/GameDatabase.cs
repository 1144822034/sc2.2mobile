using TemplatesDatabase;

namespace GameEntitySystem
{
	public class GameDatabase
	{
		public Database Database
		{
			get;
			private set;
		}

		public DatabaseObjectType FolderType
		{
			get;
			private set;
		}

		public DatabaseObjectType ProjectTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType MemberSubsystemTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType SubsystemTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType EntityTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType MemberComponentTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType ComponentTemplateType
		{
			get;
			private set;
		}

		public DatabaseObjectType ParameterSetType
		{
			get;
			private set;
		}

		public DatabaseObjectType ParameterType
		{
			get;
			private set;
		}

		public GameDatabase(Database database)
		{
			Database = database;
			FolderType = database.FindDatabaseObjectType("Folder", throwIfNotFound: true);
			ProjectTemplateType = database.FindDatabaseObjectType("ProjectTemplate", throwIfNotFound: true);
			MemberSubsystemTemplateType = database.FindDatabaseObjectType("MemberSubsystemTemplate", throwIfNotFound: true);
			SubsystemTemplateType = database.FindDatabaseObjectType("SubsystemTemplate", throwIfNotFound: true);
			EntityTemplateType = database.FindDatabaseObjectType("EntityTemplate", throwIfNotFound: true);
			MemberComponentTemplateType = database.FindDatabaseObjectType("MemberComponentTemplate", throwIfNotFound: true);
			ComponentTemplateType = database.FindDatabaseObjectType("ComponentTemplate", throwIfNotFound: true);
			ParameterSetType = database.FindDatabaseObjectType("ParameterSet", throwIfNotFound: true);
			ParameterType = database.FindDatabaseObjectType("Parameter", throwIfNotFound: true);
		}
	}
}
