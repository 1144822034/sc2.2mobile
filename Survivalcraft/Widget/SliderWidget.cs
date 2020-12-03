using Engine;
using Engine.Media;
using System;
using System.Xml.Linq;

namespace Game
{
	public class SliderWidget : CanvasWidget
	{
		public CanvasWidget m_canvasWidget;

		public CanvasWidget m_labelCanvasWidget;

		public Widget m_tabWidget;

		public LabelWidget m_labelWidget;

		public float m_minValue;

		public float m_maxValue = 1f;

		public float m_granularity = 0.1f;

		public float m_value;

		public Vector2? m_dragStartPoint;

		public bool IsSliding
		{
			get;
			set;
		}

		public LayoutDirection LayoutDirection
		{
			get;
			set;
		}

		public float MinValue
		{
			get
			{
				return m_minValue;
			}
			set
			{
				if (value != m_minValue)
				{
					m_minValue = value;
					MaxValue = MathUtils.Max(MinValue, MaxValue);
					Value = MathUtils.Clamp(Value, MinValue, MaxValue);
				}
			}
		}

		public float MaxValue
		{
			get
			{
				return m_maxValue;
			}
			set
			{
				if (value != m_maxValue)
				{
					m_maxValue = value;
					MinValue = MathUtils.Min(MinValue, MaxValue);
					Value = MathUtils.Clamp(Value, MinValue, MaxValue);
				}
			}
		}

		public float Value
		{
			get
			{
				return m_value;
			}
			set
			{
				if (m_granularity > 0f)
				{
					m_value = MathUtils.Round(MathUtils.Clamp(value, MinValue, MaxValue) / m_granularity) * m_granularity;
				}
				else
				{
					m_value = MathUtils.Clamp(value, MinValue, MaxValue);
				}
			}
		}

		public float Granularity
		{
			get
			{
				return m_granularity;
			}
			set
			{
				m_granularity = MathUtils.Max(value, 0f);
			}
		}

		public string Text
		{
			get
			{
				return m_labelWidget.Text;
			}
			set
			{
				m_labelWidget.Text = value;
			}
		}

		public BitmapFont Font
		{
			get
			{
				return m_labelWidget.Font;
			}
			set
			{
				m_labelWidget.Font = value;
			}
		}

		public string SoundName
		{
			get;
			set;
		}

		public bool IsLabelVisible
		{
			get
			{
				return m_labelCanvasWidget.IsVisible;
			}
			set
			{
				m_labelCanvasWidget.IsVisible = value;
			}
		}

		public float LabelWidth
		{
			get
			{
				return m_labelCanvasWidget.Size.X;
			}
			set
			{
				m_labelCanvasWidget.Size = new Vector2(value, m_labelCanvasWidget.Size.Y);
			}
		}

		public SliderWidget()
		{
			XElement node = ContentManager.Get<XElement>("Widgets/SliderContents");
			LoadChildren(this, node);
			m_canvasWidget = Children.Find<CanvasWidget>("Slider.Canvas");
			m_labelCanvasWidget = Children.Find<CanvasWidget>("Slider.LabelCanvas");
			m_tabWidget = Children.Find<Widget>("Slider.Tab");
			m_labelWidget = Children.Find<LabelWidget>("Slider.Label");
			LoadProperties(this, node);
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			base.MeasureOverride(parentAvailableSize);
			base.IsDrawRequired = true;
		}

		public override void ArrangeOverride()
		{
			base.ArrangeOverride();
			float num = (LayoutDirection == LayoutDirection.Horizontal) ? m_canvasWidget.ActualSize.X : m_canvasWidget.ActualSize.Y;
			float num2 = (LayoutDirection == LayoutDirection.Horizontal) ? m_tabWidget.ActualSize.X : m_tabWidget.ActualSize.Y;
			float num3 = (MaxValue > MinValue) ? ((Value - MinValue) / (MaxValue - MinValue)) : 0f;
			if (LayoutDirection == LayoutDirection.Horizontal)
			{
				Vector2 zero = Vector2.Zero;
				zero.X = num3 * (num - num2);
				zero.Y = MathUtils.Max((base.ActualSize.Y - m_tabWidget.ActualSize.Y) / 2f, 0f);
				m_canvasWidget.SetWidgetPosition(m_tabWidget, zero);
			}
			else
			{
				Vector2 zero2 = Vector2.Zero;
				zero2.X = MathUtils.Max(base.ActualSize.X - m_tabWidget.ActualSize.X, 0f) / 2f;
				zero2.Y = num3 * (num - num2);
				m_canvasWidget.SetWidgetPosition(m_tabWidget, zero2);
			}
			base.ArrangeOverride();
		}

		public override void Update()
		{
			float num = (LayoutDirection == LayoutDirection.Horizontal) ? m_canvasWidget.ActualSize.X : m_canvasWidget.ActualSize.Y;
			float num2 = (LayoutDirection == LayoutDirection.Horizontal) ? m_tabWidget.ActualSize.X : m_tabWidget.ActualSize.Y;
			if (base.Input.Tap.HasValue && HitTestGlobal(base.Input.Tap.Value) == m_tabWidget)
			{
				m_dragStartPoint = ScreenToWidget(base.Input.Press.Value);
			}
			if (base.Input.Press.HasValue)
			{
				if (m_dragStartPoint.HasValue)
				{
					Vector2 vector = ScreenToWidget(base.Input.Press.Value);
					float value = Value;
					if (LayoutDirection == LayoutDirection.Horizontal)
					{
						float f = (vector.X - num2 / 2f) / (num - num2);
						Value = MathUtils.Lerp(MinValue, MaxValue, f);
					}
					else
					{
						float f2 = (vector.Y - num2 / 2f) / (num - num2);
						Value = MathUtils.Lerp(MinValue, MaxValue, f2);
					}
					if (Value != value && m_granularity > 0f && !string.IsNullOrEmpty(SoundName))
					{
						AudioManager.PlaySound(SoundName, 1f, 0f, 0f);
					}
				}
			}
			else
			{
				m_dragStartPoint = null;
			}
			IsSliding = (m_dragStartPoint.HasValue && base.IsEnabledGlobal && base.IsVisibleGlobal);
			if (m_dragStartPoint.HasValue)
			{
				base.Input.Clear();
			}
		}
	}
}
