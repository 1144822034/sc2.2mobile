using Engine;
using Engine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TemplatesDatabase;

namespace GameEntitySystem
{
	public class Project : IDisposable
	{
		private GameDatabase m_gameDatabase;

		private DatabaseObject m_projectTemplate;

		private List<Subsystem> m_subsystems = new List<Subsystem>();

		private Dictionary<Entity, bool> m_entities = new Dictionary<Entity, bool>();

		public GameDatabase GameDatabase => m_gameDatabase;

		public DatabaseObject ProjectTemplate => m_projectTemplate;

		public ReadOnlyList<Subsystem> Subsystems => new ReadOnlyList<Subsystem>(m_subsystems);

		public Dictionary<Entity, bool>.KeyCollection Entities => m_entities.Keys;

		public event EventHandler<EntityAddRemoveEventArgs> EntityAdded;
		public event EventHandler<EntityAddRemoveEventArgs> EntityRemoved;

		public Project(GameDatabase gameDatabase, ProjectData projectData)
		{
			try
			{
				m_gameDatabase = gameDatabase;
				m_projectTemplate = projectData.ValuesDictionary.DatabaseObject;
				Dictionary<string, Subsystem> dictionary = new Dictionary<string, Subsystem>();
				foreach (ValuesDictionary item in from x in projectData.ValuesDictionary.Values
					select x as ValuesDictionary into x
					where x != null && x.DatabaseObject != null && x.DatabaseObject.Type == gameDatabase.MemberSubsystemTemplateType
					select x)
				{
					bool value = item.GetValue<bool>("IsOptional");
					string value2 = item.GetValue<string>("Class");
					Type type = TypeCache.FindType(value2, skipSystemAssemblies: false, !value);
					if (type != null)
					{
						object obj;
						try
						{
							obj = Activator.CreateInstance(type);
						}
						catch (TargetInvocationException ex)
						{
							throw ex.InnerException;
						}
						Subsystem subsystem = obj as Subsystem;
						if (subsystem == null)
						{
							throw new InvalidOperationException($"Type \"{value2}\" cannot be used as a subsystem because it does not inherit from Subsystem class.");
						}
						subsystem.Initialize(this, item);
						dictionary.Add(item.DatabaseObject.Name, subsystem);
						m_subsystems.Add(subsystem);
					}
				}
				Dictionary<Subsystem, bool> loadedSubsystems = new Dictionary<Subsystem, bool>();
				foreach (Subsystem value3 in dictionary.Values)
				{
					LoadSubsystem(value3, dictionary, loadedSubsystems, 0);
				}
				if (projectData.EntityDataList != null)
				{
					List<Entity> entities = LoadEntities(projectData.EntityDataList);
					AddEntities(entities);
				}
			}
			catch (Exception)
			{
				try
				{
					Dispose();
				}
				catch (Exception)
				{
				}
				throw;
			}
		}

		public Subsystem FindSubsystem(Type type, string name, bool throwOnError)
		{
			foreach (Subsystem subsystem in m_subsystems)
			{
				if (type.GetTypeInfo().IsAssignableFrom(subsystem.GetType().GetTypeInfo()) && (name == null || subsystem.ValuesDictionary.DatabaseObject.Name == name))
				{
					return subsystem;
				}
			}
			if (throwOnError)
			{
				if (name != null)
				{
					throw new Exception($"Required subsystem {type.FullName} with name \"{name}\" does not exist in project.");
				}
				throw new Exception($"Required subsystem {type.FullName} does not exist in project.");
			}
			return null;
		}

		public T FindSubsystem<T>() where T : class
		{
			return FindSubsystem(typeof(T), null, throwOnError: false) as T;
		}

		public T FindSubsystem<T>(bool throwOnError) where T : class
		{
			return FindSubsystem(typeof(T), null, throwOnError) as T;
		}

		public T FindSubsystem<T>(string name, bool throwOnError) where T : class
		{
			return FindSubsystem(typeof(T), name, throwOnError) as T;
		}

		public IEnumerable<Subsystem> FindSubsystems(Type type)
		{
			foreach (Subsystem subsystem in m_subsystems)
			{
				if (type.GetTypeInfo().IsAssignableFrom(subsystem.GetType().GetTypeInfo()))
				{
					yield return subsystem;
				}
			}
		}

		public IEnumerable<T> FindSubsystems<T>() where T : class
		{
			foreach (Subsystem subsystem in m_subsystems)
			{
				T val = subsystem as T;
				if (val != null)
				{
					yield return val;
				}
			}
		}

		public Entity CreateEntity(ValuesDictionary valuesDictionary)
		{
			try
			{
				Entity entity = new Entity(this, valuesDictionary);
				IdToEntityMap idToEntityMap = new IdToEntityMap(new Dictionary<int, Entity>());
				entity.publicLoadEntity(valuesDictionary, idToEntityMap);
				return entity;
			}
			catch (Exception innerException)
			{
				throw new Exception($"Error creating entity from template \"{valuesDictionary.DatabaseObject.Name}\".", innerException);
			}
		}

		public void AddEntity(Entity entity)
		{
			if (entity.Project != this)
			{
				throw new Exception("Entity does not belong to this project.");
			}
			if (!entity.IsAddedToProject)
			{
				m_entities.Add(entity, value: true);
				entity.m_isAddedToProject = true;
				FireEntityAddedEvents(entity);
			}
		}

		public void RemoveEntity(Entity entity, bool disposeEntity)
		{
			if (entity.Project != this)
			{
				throw new Exception("Entity does not belong to this project.");
			}
			if (entity.IsAddedToProject)
			{
				m_entities.Remove(entity);
				entity.m_isAddedToProject = false;
				FireEntityRemovedEvents(entity);
				if (disposeEntity)
				{
					entity.Dispose();
				}
			}
		}

		public void AddEntities(IEnumerable<Entity> entities)
		{
			foreach (Entity entity in entities)
			{
				AddEntity(entity);
			}
		}

		public void RemoveEntities(IEnumerable<Entity> entities, bool disposeEntities)
		{
			foreach (Entity entity in entities)
			{
				RemoveEntity(entity, disposeEntities);
			}
		}

		public List<Entity> LoadEntities(EntityDataList entityDataList)
		{
			List<Entity> list = new List<Entity>(entityDataList.EntitiesData.Count);
			Dictionary<int, Entity> dictionary = new Dictionary<int, Entity>();
			IdToEntityMap idToEntityMap = new IdToEntityMap(dictionary);
			foreach (EntityData entitiesDatum in entityDataList.EntitiesData)
			{
				try
				{
					Entity entity = new Entity(this, entitiesDatum.ValuesDictionary);
					list.Add(entity);
					if (entitiesDatum.Id != 0)
					{
						dictionary.Add(entitiesDatum.Id, entity);
					}
				}
				catch (Exception innerException)
				{
					throw new Exception($"Error creating entity from template \"{entitiesDatum.ValuesDictionary.DatabaseObject.Name}\".", innerException);
				}
			}
			int num = 0;
			foreach (EntityData entitiesDatum2 in entityDataList.EntitiesData)
			{
				list[num].publicLoadEntity(entitiesDatum2.ValuesDictionary, idToEntityMap);
				num++;
			}
			return list;
		}

		public EntityDataList SaveEntities(IEnumerable<Entity> entities)
		{
			Dictionary<Entity, bool> dictionary = DetermineNotOwnedEntities(entities);
			int num = 1;
			Dictionary<Entity, int> dictionary2 = new Dictionary<Entity, int>();
			EntityToIdMap entityToIdMap = new EntityToIdMap(dictionary2);
			foreach (Entity key in dictionary.Keys)
			{
				dictionary2.Add(key, num);
				num++;
			}
			EntityDataList entityDataList = new EntityDataList();
			entityDataList.EntitiesData = new List<EntityData>(dictionary.Keys.Count);
			foreach (Entity key2 in dictionary.Keys)
			{
				EntityData entityData = new EntityData();
				entityData.Id = entityToIdMap.FindId(key2);
				entityData.ValuesDictionary = new ValuesDictionary();
				entityData.ValuesDictionary.DatabaseObject = key2.ValuesDictionary.DatabaseObject;
				key2.publicSaveEntity(entityData.ValuesDictionary, entityToIdMap);
				entityDataList.EntitiesData.Add(entityData);
			}
			return entityDataList;
		}

		public ProjectData Save()
		{
			ProjectData projectData = new ProjectData();
			projectData.ValuesDictionary = new ValuesDictionary();
			projectData.ValuesDictionary.DatabaseObject = ProjectTemplate;
			foreach (Subsystem subsystem in Subsystems)
			{
				ValuesDictionary valuesDictionary = new ValuesDictionary();
				subsystem.Save(valuesDictionary);
				if (valuesDictionary.Count > 0)
				{
					projectData.ValuesDictionary.SetValue(subsystem.ValuesDictionary.DatabaseObject.Name, valuesDictionary);
				}
			}
			projectData.EntityDataList = SaveEntities(Entities);
			return projectData;
		}

		public void Dispose()
		{
			if (m_entities != null)
			{
				foreach (Entity key in m_entities.Keys)
				{
					key.Dispose();
				}
			}
			if (m_subsystems != null)
			{
				foreach (Subsystem subsystem in m_subsystems)
				{
					subsystem.Dispose();
				}
			}
		}

		private void FireEntityAddedEvents(Entity entity)
		{
			foreach (Component component in entity.Components)
			{
				component.OnEntityAdded();
			}
			foreach (Subsystem subsystem in Subsystems)
			{
				subsystem.OnEntityAdded(entity);
			}
			if (this.EntityAdded != null)
			{
				this.EntityAdded(this, new EntityAddRemoveEventArgs(entity));
			}
			entity.FireEntityAddedEvent();
		}

		private void FireEntityRemovedEvents(Entity entity)
		{
			foreach (Component component in entity.Components)
			{
				component.OnEntityRemoved();
			}
			foreach (Subsystem subsystem in Subsystems)
			{
				subsystem.OnEntityRemoved(entity);
			}
			if (this.EntityRemoved != null)
			{
				this.EntityRemoved(this, new EntityAddRemoveEventArgs(entity));
			}
			entity.FireEntityRemovedEvent();
		}

		private static Dictionary<Entity, bool> DetermineNotOwnedEntities(IEnumerable<Entity> entities)
		{
			Dictionary<Entity, bool> dictionary = new Dictionary<Entity, bool>();
			List<Entity> list = new List<Entity>();
			foreach (Entity entity in entities)
			{
				dictionary.Add(entity, value: true);
				List<Entity> list2 = entity.publicGetOwnedEntities();
				if (list2 != null)
				{
					list.AddRange(list2);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				List<Entity> list3 = list[i].publicGetOwnedEntities();
				if (list3 != null)
				{
					list.AddRange(list3);
				}
				dictionary.Remove(list[i]);
			}
			return dictionary;
		}

		private void LoadSubsystem(Subsystem subsystem, Dictionary<string, Subsystem> subsystemsByName, Dictionary<Subsystem, bool> loadedSubsystems, int depth)
		{
			if (depth > 100)
			{
				throw new InvalidOperationException($"Too deep dependencies recursion while loading subsystem \"{subsystem.ValuesDictionary.DatabaseObject.Name}\".");
			}
			if (loadedSubsystems.ContainsKey(subsystem))
			{
				return;
			}
			string value = subsystem.ValuesDictionary.GetValue("Dependencies", string.Empty);
			if (!string.IsNullOrEmpty(value))
			{
				string[] array = value.Split(new char[1]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i].Trim();
					if (subsystemsByName.TryGetValue(text, out Subsystem value2))
					{
						LoadSubsystem(value2, subsystemsByName, loadedSubsystems, depth + 1);
						continue;
					}
					throw new InvalidOperationException($"Dependency subsystem \"{text}\" not found when loading subsystem \"{subsystem.ValuesDictionary.DatabaseObject.Name}\".");
				}
			}
			subsystem.Load(subsystem.ValuesDictionary);
			loadedSubsystems.Add(subsystem, value: true);
		}
	}
}
