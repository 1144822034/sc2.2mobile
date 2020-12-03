using Engine;
using System.Xml.Linq;

namespace Game
{
	public class MusketWidget : CanvasWidget
	{
		public IInventory m_inventory;

		public int m_slotIndex;

		public GridPanelWidget m_inventoryGrid;

		public InventorySlotWidget m_inventorySlotWidget;

		public LabelWidget m_instructionsLabel;

		public MusketWidget(IInventory inventory, int slotIndex)
		{
			m_inventory = inventory;
			m_slotIndex = slotIndex;
			XElement node = ContentManager.Get<XElement>("Widgets/MusketWidget");
			LoadContents(this, node);
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			m_inventorySlotWidget = Children.Find<InventorySlotWidget>("InventorySlot");
			m_instructionsLabel = Children.Find<LabelWidget>("InstructionsLabel");
			for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
			{
				for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
				{
					InventorySlotWidget widget = new InventorySlotWidget();
					m_inventoryGrid.Children.Add(widget);
					m_inventoryGrid.SetWidgetCell(widget, new Point2(j, i));
				}
			}
			int num = 10;
			foreach (Widget child in m_inventoryGrid.Children)
			{
				(child as InventorySlotWidget)?.AssignInventorySlot(inventory, num++);
			}
			m_inventorySlotWidget.AssignInventorySlot(inventory, slotIndex);
			m_inventorySlotWidget.CustomViewMatrix = Matrix.CreateLookAt(new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 0f), -Vector3.UnitZ);
		}

		public override void Update()
		{
			int slotValue = m_inventory.GetSlotValue(m_slotIndex);
			int slotCount = m_inventory.GetSlotCount(m_slotIndex);
			if (Terrain.ExtractContents(slotValue) == 212 && slotCount > 0)
			{
				switch (MusketBlock.GetLoadState(Terrain.ExtractData(slotValue)))
				{
				case MusketBlock.LoadState.Empty:
					m_instructionsLabel.Text = "Load gunpowder";
					break;
				case MusketBlock.LoadState.Gunpowder:
					m_instructionsLabel.Text = "Load cotton wad";
					break;
				case MusketBlock.LoadState.Wad:
					m_instructionsLabel.Text = "Load bullet";
					break;
				case MusketBlock.LoadState.Loaded:
					m_instructionsLabel.Text = "Ready to fire";
					break;
				default:
					m_instructionsLabel.Text = string.Empty;
					break;
				}
			}
			else
			{
				base.ParentWidget.Children.Remove(this);
			}
		}
	}
}
