using System;
using System.Xml.Linq;

namespace Game
{
	public class EditTruthTableDialog : Dialog
	{
		public Action<bool> m_handler;

		public Widget m_linearPanel;

		public Widget m_gridPanel;

		public ButtonWidget m_okButton;

		public ButtonWidget m_cancelButton;

		public ButtonWidget m_switchViewButton;

		public CheckboxWidget[] m_lineCheckboxes = new CheckboxWidget[16];

		public TextBoxWidget m_linearTextBox;

		public TruthTableData m_truthTableData;

		public TruthTableData m_tmpTruthTableData;

		public bool m_ignoreTextChanges;

		public EditTruthTableDialog(TruthTableData truthTableData, Action<bool> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditTruthTableDialog");
			LoadContents(this, node);
			m_linearPanel = Children.Find<Widget>("EditTruthTableDialog.LinearPanel");
			m_gridPanel = Children.Find<Widget>("EditTruthTableDialog.GridPanel");
			m_okButton = Children.Find<ButtonWidget>("EditTruthTableDialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("EditTruthTableDialog.Cancel");
			m_switchViewButton = Children.Find<ButtonWidget>("EditTruthTableDialog.SwitchViewButton");
			m_linearTextBox = Children.Find<TextBoxWidget>("EditTruthTableDialog.LinearText");
			for (int i = 0; i < 16; i++)
			{
				m_lineCheckboxes[i] = Children.Find<CheckboxWidget>("EditTruthTableDialog.Line" + i.ToString());
			}
			m_handler = handler;
			m_truthTableData = truthTableData;
			m_tmpTruthTableData = (TruthTableData)m_truthTableData.Copy();
			m_linearPanel.IsVisible = false;
			m_linearTextBox.TextChanged += delegate
			{
				if (!m_ignoreTextChanges)
				{
					m_tmpTruthTableData = new TruthTableData();
					m_tmpTruthTableData.LoadBinaryString(m_linearTextBox.Text);
				}
			};
		}

		public override void Update()
		{
			m_ignoreTextChanges = true;
			try
			{
				m_linearTextBox.Text = m_tmpTruthTableData.SaveBinaryString();
			}
			finally
			{
				m_ignoreTextChanges = false;
			}
			for (int i = 0; i < 16; i++)
			{
				if (m_lineCheckboxes[i].IsClicked)
				{
					m_tmpTruthTableData.Data[i] = (byte)((m_tmpTruthTableData.Data[i] == 0) ? 15 : 0);
				}
				m_lineCheckboxes[i].IsChecked = (m_tmpTruthTableData.Data[i] > 0);
			}
			if (m_linearPanel.IsVisible)
			{
				m_switchViewButton.Text = "Table";
				if (m_switchViewButton.IsClicked)
				{
					m_linearPanel.IsVisible = false;
					m_gridPanel.IsVisible = true;
				}
			}
			else
			{
				m_switchViewButton.Text = "Linear";
				if (m_switchViewButton.IsClicked)
				{
					m_linearPanel.IsVisible = true;
					m_gridPanel.IsVisible = false;
				}
			}
			if (m_okButton.IsClicked)
			{
				m_truthTableData.Data = m_tmpTruthTableData.Data;
				Dismiss(result: true);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				Dismiss(result: false);
			}
		}

		public void Dismiss(bool result)
		{
			DialogsManager.HideDialog(this);
			if (m_handler != null)
			{
				m_handler(result);
			}
		}
	}
}
