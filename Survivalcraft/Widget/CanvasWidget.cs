using Engine;
using System.Collections.Generic;

namespace Game
{
	public class CanvasWidget : ContainerWidget
	{
		public Dictionary<Widget, Vector2> m_positions = new Dictionary<Widget, Vector2>();

		public Vector2 Size
		{
			get;
			set;
		} = new Vector2(-1f);


		public static void SetPosition(Widget widget, Vector2 position)
		{
			(widget.ParentWidget as CanvasWidget)?.SetWidgetPosition(widget, position);
		}

		public Vector2? GetWidgetPosition(Widget widget)
		{
			if (m_positions.TryGetValue(widget, out Vector2 value))
			{
				return value;
			}
			return null;
		}

		public void SetWidgetPosition(Widget widget, Vector2? position)
		{
			if (position.HasValue)
			{
				m_positions[widget] = position.Value;
			}
			else
			{
				m_positions.Remove(widget);
			}
		}

		public override void WidgetRemoved(Widget widget)
		{
			m_positions.Remove(widget);
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			Vector2 desiredSize = Vector2.Zero;
			if (Size.X >= 0f)
			{
				parentAvailableSize.X = MathUtils.Min(parentAvailableSize.X, Size.X);
			}
			if (Size.Y >= 0f)
			{
				parentAvailableSize.Y = MathUtils.Min(parentAvailableSize.Y, Size.Y);
			}
			foreach (Widget child in Children)
			{
				if (child.IsVisible)
				{
					Vector2? widgetPosition = GetWidgetPosition(child);
					Vector2 v = widgetPosition.HasValue ? widgetPosition.Value : Vector2.Zero;
					child.Measure(Vector2.Max(parentAvailableSize - v - 2f * child.Margin, Vector2.Zero));
					Vector2 vector = default(Vector2);
					vector.X = MathUtils.Max(desiredSize.X, v.X + child.ParentDesiredSize.X + 2f * child.Margin.X);
					vector.Y = MathUtils.Max(desiredSize.Y, v.Y + child.ParentDesiredSize.Y + 2f * child.Margin.Y);
					desiredSize = vector;
				}
			}
			if (Size.X >= 0f)
			{
				desiredSize.X = Size.X;
			}
			if (Size.Y >= 0f)
			{
				desiredSize.Y = Size.Y;
			}
			base.DesiredSize = desiredSize;
		}

		public override void ArrangeOverride()
		{
			foreach (Widget child in Children)
			{
				if (child.IsVisible)
				{
					Vector2? widgetPosition = GetWidgetPosition(child);
					if (widgetPosition.HasValue)
					{
						Vector2 zero = Vector2.Zero;
						if (!float.IsPositiveInfinity(child.ParentDesiredSize.X))
						{
							zero.X = child.ParentDesiredSize.X;
						}
						else
						{
							zero.X = MathUtils.Max(base.ActualSize.X - widgetPosition.Value.X, 0f);
						}
						if (!float.IsPositiveInfinity(child.ParentDesiredSize.Y))
						{
							zero.Y = child.ParentDesiredSize.Y;
						}
						else
						{
							zero.Y = MathUtils.Max(base.ActualSize.Y - widgetPosition.Value.Y, 0f);
						}
						child.Arrange(widgetPosition.Value, zero);
					}
					else
					{
						ContainerWidget.ArrangeChildWidgetInCell(Vector2.Zero, base.ActualSize, child);
					}
				}
			}
		}
	}
}
