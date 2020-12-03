using System.Xml.Linq;

namespace Game
{
	public class SpawnDialog : Dialog
	{
		public LabelWidget m_largeLabelWidget;

		public LabelWidget m_smallLabelWidget;

		public ValueBarWidget m_progressWidget;

		public string LargeMessage
		{
			get
			{
				return m_largeLabelWidget.Text;
			}
			set
			{
				m_largeLabelWidget.Text = value;
			}
		}

		public string SmallMessage
		{
			get
			{
				return m_smallLabelWidget.Text;
			}
			set
			{
				m_smallLabelWidget.Text = value;
			}
		}

		public float Progress
		{
			get
			{
				return m_progressWidget.Value;
			}
			set
			{
				m_progressWidget.Value = value;
			}
		}

		public SpawnDialog()
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/SpawnDialog");
			LoadContents(this, node);
			m_largeLabelWidget = Children.Find<LabelWidget>("SpawnDialog.LargeLabel");
			m_smallLabelWidget = Children.Find<LabelWidget>("SpawnDialog.SmallLabel");
			m_progressWidget = Children.Find<ValueBarWidget>("SpawnDialog.Progress");
		}
	}
}
