using System;
using System.Xml.Linq;
using TemplatesDatabase;
using XmlUtilities;

namespace GameEntitySystem
{
	public class ProjectData
	{
		public ValuesDictionary ValuesDictionary;

		public EntityDataList EntityDataList;

		public ProjectData()
		{
		}

		public ProjectData(GameDatabase gameDatabase, DatabaseObject projectTemplate, ValuesDictionary overrides)
		{
			ValuesDictionary = new ValuesDictionary();
			ValuesDictionary.PopulateFromDatabaseObject(projectTemplate);
			if (overrides != null)
			{
				ValuesDictionary.ApplyOverrides(overrides);
			}
		}

		public ProjectData(GameDatabase gameDatabase, XElement projectNode, ValuesDictionary overrides, bool ignoreInvalidEntities)
		{
			Guid attributeValue = XmlUtils.GetAttributeValue(projectNode, "Guid", Guid.Empty);
			string attributeValue2 = XmlUtils.GetAttributeValue(projectNode, "Name", string.Empty);
			DatabaseObject databaseObject;
			if (attributeValue != Guid.Empty)
			{
				databaseObject = gameDatabase.Database.FindDatabaseObject(attributeValue, gameDatabase.ProjectTemplateType, throwIfNotFound: true);
			}
			else
			{
				if (string.IsNullOrEmpty(attributeValue2))
				{
					throw new InvalidOperationException("Project template guid or name must be specified.");
				}
				databaseObject = gameDatabase.Database.FindDatabaseObject(attributeValue2, gameDatabase.ProjectTemplateType, throwIfNotFound: true);
			}
			ValuesDictionary = new ValuesDictionary();
			ValuesDictionary.PopulateFromDatabaseObject(databaseObject);
			XElement xElement = XmlUtils.FindChildElement(projectNode, "Subsystems", throwIfNotFound: false);
			if (xElement != null)
			{
				ValuesDictionary.ApplyOverrides(xElement);
			}
			if (overrides != null)
			{
				ValuesDictionary.ApplyOverrides(overrides);
			}
			XElement xElement2 = XmlUtils.FindChildElement(projectNode, "Entities", throwIfNotFound: false);
			if (xElement2 != null)
			{
				EntityDataList = new EntityDataList(gameDatabase, xElement2, ignoreInvalidEntities);
			}
		}

		public void Save(XElement projectNode)
		{
			XmlUtils.SetAttributeValue(projectNode, "Guid", ValuesDictionary.DatabaseObject.Guid);
			XmlUtils.SetAttributeValue(projectNode, "Name", ValuesDictionary.DatabaseObject.Name);
			XElement node = XmlUtils.AddElement(projectNode, "Subsystems");
			ValuesDictionary.Save(node);
			if (EntityDataList != null)
			{
				XElement entitiesNode = XmlUtils.AddElement(projectNode, "Entities");
				EntityDataList.Save(entitiesNode);
			}
		}
	}
}
