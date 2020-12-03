using Engine;

namespace Game
{
	public class FixedSizePanelWidget : ContainerWidget
	{
		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			Vector2 zero = Vector2.Zero;
			foreach (Widget child in Children)
			{
				if (child.IsVisible)
				{
					child.Measure(Vector2.Max(parentAvailableSize - 2f * child.Margin, Vector2.Zero));
					if (child.ParentDesiredSize.X != float.PositiveInfinity)
					{
						zero.X = MathUtils.Max(zero.X, child.ParentDesiredSize.X + 2f * child.Margin.X);
					}
					if (child.ParentDesiredSize.Y != float.PositiveInfinity)
					{
						zero.Y = MathUtils.Max(zero.Y, child.ParentDesiredSize.Y + 2f * child.Margin.Y);
					}
				}
			}
			base.DesiredSize = zero;
		}

		public override void ArrangeOverride()
		{
			foreach (Widget child in Children)
			{
				ContainerWidget.ArrangeChildWidgetInCell(Vector2.Zero, base.ActualSize, child);
			}
		}
	}
}
