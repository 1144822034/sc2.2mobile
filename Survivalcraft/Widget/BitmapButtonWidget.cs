using Engine;
using Engine.Media;
using System.Xml.Linq;

namespace Game
{
	public class BitmapButtonWidget : ButtonWidget
	{
		public RectangleWidget m_rectangleWidget;

		public RectangleWidget m_imageWidget;

		public LabelWidget m_labelWidget;

		public ClickableWidget m_clickableWidget;

		public override bool IsClicked => m_clickableWidget.IsClicked;

		public override bool IsChecked
		{
			get
			{
				return m_clickableWidget.IsChecked;
			}
			set
			{
				m_clickableWidget.IsChecked = value;
			}
		}

		public override bool IsAutoCheckingEnabled
		{
			get
			{
				return m_clickableWidget.IsAutoCheckingEnabled;
			}
			set
			{
				m_clickableWidget.IsAutoCheckingEnabled = value;
			}
		}

		public override string Text
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

		public override BitmapFont Font
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

		public Subtexture NormalSubtexture
		{
			get;
			set;
		}

		public Subtexture ClickedSubtexture
		{
			get;
			set;
		}

		public override Color Color
		{
			get;
			set;
		}

		public BitmapButtonWidget()
		{
			Color = Color.White;
			XElement node = ContentManager.Get<XElement>("Widgets/BitmapButtonContents");
			LoadChildren(this, node);
			m_rectangleWidget = Children.Find<RectangleWidget>("Button.Rectangle");
			m_imageWidget = Children.Find<RectangleWidget>("Button.Image");
			m_labelWidget = Children.Find<LabelWidget>("Button.Label");
			m_clickableWidget = Children.Find<ClickableWidget>("Button.Clickable");
			LoadProperties(this, node);
		}

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			bool isEnabledGlobal = base.IsEnabledGlobal;
			m_labelWidget.Color = (isEnabledGlobal ? Color : new Color(112, 112, 112));
			m_imageWidget.FillColor = (isEnabledGlobal ? Color : new Color(112, 112, 112));
			if (m_clickableWidget.IsPressed || IsChecked)
			{
				m_rectangleWidget.Subtexture = ClickedSubtexture;
			}
			else
			{
				m_rectangleWidget.Subtexture = NormalSubtexture;
			}
			base.MeasureOverride(parentAvailableSize);
		}
	}
}
