using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace GameEntitySystem
{
	public class EntityDataList
	{
		public List<EntityData> EntitiesData;

		public EntityDataList()
		{
		}

		public EntityDataList(GameDatabase gameDatabase, XElement entitiesNode, bool ignoreInvalidEntities)
		{
			EntitiesData = new List<EntityData>(entitiesNode.Elements().Count());
			foreach (XElement item in entitiesNode.Elements())
			{
				try
				{
					EntitiesData.Add(new EntityData(gameDatabase, item));
				}
				catch (Exception ex)
				{
					if (!ignoreInvalidEntities)
					{
						throw ex;
					}
					Log.Warning("Ignoring invalid entity. Reason: {0}", ex.Message);
				}
			}
		}

		public void Save(XElement entitiesNode)
		{
			foreach (EntityData entitiesDatum in EntitiesData)
			{
				XElement entityNode = XmlUtils.AddElement(entitiesNode, "Entity");
				entitiesDatum.Save(entityNode);
			}
		}
	}
}
