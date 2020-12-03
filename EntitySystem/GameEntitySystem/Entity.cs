using Engine;
using Engine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TemplatesDatabase;

namespace GameEntitySystem
{
	public sealed class Entity : IDisposable
	{
		public struct FilteredComponentsEnumerable<T> : IEnumerable<T>, IEnumerable where T : class
		{
			private Entity m_entity;

			public FilteredComponentsEnumerable(Entity entity)
			{
				m_entity = entity;
			}

			public FilteredComponentsEnumerator<T> GetEnumerator()
			{
				return new FilteredComponentsEnumerator<T>(m_entity);
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return new FilteredComponentsEnumerator<T>(m_entity);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new FilteredComponentsEnumerator<T>(m_entity);
			}
		}

		public struct FilteredComponentsEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator where T : class
		{
			private Entity m_entity;

			private int m_index;

			private T m_current;

			public T Current => m_current;

			object IEnumerator.Current => m_current;

			public FilteredComponentsEnumerator(Entity entity)
			{
				m_entity = entity;
				m_index = 0;
				m_current = null;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				while (m_index < m_entity.m_components.Count)
				{
					T val = m_entity.m_components[m_index++] as T;
					if (val != null)
					{
						m_current = val;
						return true;
					}
				}
				m_current = null;
				return false;
			}

			public void Reset()
			{
				m_index = 0;
				m_current = null;
			}
		}

		private Project m_project;

		private ValuesDictionary m_valuesDictionary;

		private List<Component> m_components = new List<Component>();

		public bool m_isAddedToProject;

		public Project Project => m_project;

		public ValuesDictionary ValuesDictionary => m_valuesDictionary;

		public bool IsAddedToProject => m_isAddedToProject;

		public ReadOnlyList<Component> Components => new ReadOnlyList<Component>(m_components);

		public event EventHandler EntityAdded;

		public event EventHandler EntityRemoved;

		public Entity(Project project, ValuesDictionary valuesDictionary)
		{
			if (valuesDictionary.DatabaseObject.Type != project.GameDatabase.EntityTemplateType)
			{
				throw new InvalidOperationException("ValuesDictionary was not created from EntityTemplate.");
			}
			m_project = project;
			m_valuesDictionary = valuesDictionary;
			List<KeyValuePair<int, Component>> list = new List<KeyValuePair<int, Component>>();
			foreach (ValuesDictionary item in from x in valuesDictionary.Values
				select x as ValuesDictionary into x
				where x != null && x.DatabaseObject != null && x.DatabaseObject.Type == project.GameDatabase.MemberComponentTemplateType
				select x)
			{
				bool value = item.GetValue<bool>("IsOptional");
				string value2 = item.GetValue<string>("Class");
				int value3 = item.GetValue<int>("LoadOrder");
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
					Component component = obj as Component;
					if (component == null)
					{
						throw new InvalidOperationException($"Type \"{value2}\" cannot be used as a component because it does not inherit from Component class.");
					}
					component.Initialize(this, item);
					list.Add(new KeyValuePair<int, Component>(value3, component));
				}
			}
			list.Sort((KeyValuePair<int, Component> x, KeyValuePair<int, Component> y) => x.Key - y.Key);
			m_components = new List<Component>(list.Select((KeyValuePair<int, Component> x) => x.Value));
		}

		public Component FindComponent(Type type, string name, bool throwOnError)
		{
			foreach (Component component in m_components)
			{
				if (type.GetTypeInfo().IsAssignableFrom(component.GetType().GetTypeInfo()) && (string.IsNullOrEmpty(name) || component.ValuesDictionary.DatabaseObject.Name == name))
				{
					return component;
				}
			}
			if (throwOnError)
			{
				if (string.IsNullOrEmpty(name))
				{
					throw new Exception($"Required component {type.FullName} does not exist in entity.");
				}
				throw new Exception($"Required component {type.FullName} with name \"{name}\" does not exist in entity.");
			}
			return null;
		}

		public T FindComponent<T>() where T : class
		{
			return FindComponent(typeof(T), null, throwOnError: false) as T;
		}

		public T FindComponent<T>(bool throwOnError) where T : class
		{
			return FindComponent(typeof(T), null, throwOnError) as T;
		}

		public T FindComponent<T>(string name, bool throwOnError) where T : class
		{
			return FindComponent(typeof(T), name, throwOnError) as T;
		}

		public FilteredComponentsEnumerable<T> FindComponents<T>() where T : class
		{
			return new FilteredComponentsEnumerable<T>(this);
		}

		public void Dispose()
		{
			foreach (Component component in m_components)
			{
				component.Dispose();
			}
		}

		public List<Entity> publicGetOwnedEntities()
		{
			List<Entity> list = null;
			foreach (Component component in m_components)
			{
				IEnumerable<Entity> ownedEntities = component.GetOwnedEntities();
				list = ((list != null) ? list : new List<Entity>());
				list.AddRange(ownedEntities);
			}
			return list;
		}

		public void publicLoadEntity(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			foreach (Component component in m_components)
			{
				try
				{
					component.Load(component.ValuesDictionary, idToEntityMap);
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException($"Error loading component {component.GetType().FullName}.", innerException);
				}
			}
		}

		public void publicSaveEntity(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			foreach (Component component in Components)
			{
				ValuesDictionary valuesDictionary2 = new ValuesDictionary();
				component.Save(valuesDictionary2, entityToIdMap);
				if (valuesDictionary2.Count > 0)
				{
					valuesDictionary.SetValue(component.ValuesDictionary.DatabaseObject.Name, valuesDictionary2);
				}
			}
		}

		public void FireEntityAddedEvent()
		{
			if (this.EntityAdded != null)
			{
				this.EntityAdded(this, EventArgs.Empty);
			}
		}

		public void FireEntityRemovedEvent()
		{
			if (this.EntityRemoved != null)
			{
				this.EntityRemoved(this, EventArgs.Empty);
			}
		}
	}
}
