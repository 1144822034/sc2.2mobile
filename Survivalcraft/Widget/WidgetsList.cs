using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Game
{
	public class WidgetsList : IEnumerable<Widget>, IEnumerable
	{
		public struct Enumerator : IEnumerator<Widget>, IDisposable, IEnumerator
		{
			public WidgetsList m_collection;

			public Widget m_current;

			public int m_index;

			public int m_version;

			public Widget Current => m_current;

			object IEnumerator.Current => m_current;

			public Enumerator(WidgetsList collection)
			{
				m_collection = collection;
				m_current = null;
				m_index = 0;
				m_version = collection.m_version;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (m_collection.m_version != m_version)
				{
					throw new InvalidOperationException("WidgetsList was modified, enumeration cannot continue.");
				}
				if (m_index < m_collection.m_widgets.Count)
				{
					m_current = m_collection.m_widgets[m_index];
					m_index++;
					return true;
				}
				m_current = null;
				return false;
			}

			public void Reset()
			{
				if (m_collection.m_version != m_version)
				{
					throw new InvalidOperationException("SortedMultiCollection was modified, enumeration cannot continue.");
				}
				m_index = 0;
				m_current = null;
			}
		}

		public ContainerWidget m_containerWidget;

		public List<Widget> m_widgets = new List<Widget>();

		public int m_version;

		public int Count => m_widgets.Count;

		public Widget this[int index] => m_widgets[index];

		public WidgetsList(ContainerWidget containerWidget)
		{
			m_containerWidget = containerWidget;
		}

		public void Add(Widget widget)
		{
			Insert(Count, widget);
		}

		public void Add(params Widget[] widgets)
		{
			AddRange(widgets);
		}

		public void AddRange(IEnumerable<Widget> widgets)
		{
			foreach (Widget widget in widgets)
			{
				Add(widget);
			}
		}

		public void Insert(int index, Widget widget)
		{
			if (m_widgets.Contains(widget))
			{
				throw new InvalidOperationException("Child widget already present in container.");
			}
			if (index < 0 || index > m_widgets.Count)
			{
				throw new InvalidOperationException("Widget index out of range.");
			}
			widget.ChangeParent(m_containerWidget);
			m_widgets.Insert(index, widget);
			m_containerWidget.WidgetAdded(widget);
			m_version++;
		}

		public void InsertBefore(Widget beforeWidget, Widget widget)
		{
			int num = m_widgets.IndexOf(beforeWidget);
			if (num < 0)
			{
				throw new InvalidOperationException("Widget not present in container.");
			}
			Insert(num, widget);
		}

		public void InsertAfter(Widget afterWidget, Widget widget)
		{
			int num = m_widgets.IndexOf(afterWidget);
			if (num < 0)
			{
				throw new InvalidOperationException("Widget not present in container.");
			}
			Insert(num + 1, widget);
		}

		public void Remove(Widget widget)
		{
			int num = IndexOf(widget);
			if (num >= 0)
			{
				RemoveAt(num);
				return;
			}
			throw new InvalidOperationException("Child widget not present in container.");
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= m_widgets.Count)
			{
				throw new InvalidOperationException("Widget index out of range.");
			}
			Widget widget = m_widgets[index];
			widget.ChangeParent(null);
			m_widgets.RemoveAt(index);
			m_containerWidget.WidgetRemoved(widget);
			m_version--;
		}

		public void Clear()
		{
			while (Count > 0)
			{
				RemoveAt(Count - 1);
			}
		}

		public int IndexOf(Widget widget)
		{
			return m_widgets.IndexOf(widget);
		}

		public bool Contains(Widget widget)
		{
			return m_widgets.Contains(widget);
		}

		public Widget Find(string name, Type type, bool throwIfNotFound = true)
		{
			foreach (Widget widget2 in m_widgets)
			{
				if ((name == null || (widget2.Name != null && widget2.Name == name)) && (type == null || type == widget2.GetType() || widget2.GetType().GetTypeInfo().IsSubclassOf(type)))
				{
					return widget2;
				}
				ContainerWidget containerWidget = widget2 as ContainerWidget;
				if (containerWidget != null)
				{
					Widget widget = containerWidget.Children.Find(name, type, throwIfNotFound: false);
					if (widget != null)
					{
						return widget;
					}
				}
			}
			if (throwIfNotFound)
			{
				throw new Exception($"Required widget \"{name}\" of type \"{type}\" not found.");
			}
			return null;
		}

		public Widget Find(string name, bool throwIfNotFound = true)
		{
			return Find(name, null, throwIfNotFound);
		}

		public T Find<T>(string name, bool throwIfNotFound = true) where T : class
		{
			return Find(name, typeof(T), throwIfNotFound) as T;
		}

		public T Find<T>(bool throwIfNotFound = true) where T : class
		{
			return Find(null, typeof(T), throwIfNotFound) as T;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<Widget> IEnumerable<Widget>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
}
