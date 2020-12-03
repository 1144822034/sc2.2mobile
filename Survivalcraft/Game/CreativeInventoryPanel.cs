using Engine;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game
{
	public class CreativeInventoryPanel : CanvasWidget
	{
		private CreativeInventoryWidget m_creativeInventoryWidget;

		private ComponentCreativeInventory m_componentCreativeInventory;

		private List<int> m_slotIndices = new List<int>();

		private GridPanelWidget m_inventoryGrid;

		private int m_pagesCount;

		private int m_assignedCategoryIndex = -1;

		private int m_assignedPageIndex = -1;

		public CreativeInventoryPanel(CreativeInventoryWidget creativeInventoryWidget)
		{
			m_creativeInventoryWidget = creativeInventoryWidget;
			m_componentCreativeInventory = creativeInventoryWidget.Entity.FindComponent<ComponentCreativeInventory>(throwOnError: true);
			XElement node = ContentManager.Get<XElement>("Widgets/CreativeInventoryPanel");
			LoadContents(this, node);
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
			{
				for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
				{
					InventorySlotWidget widget = new InventorySlotWidget
					{
						HideEditOverlay = true,
						HideInteractiveOverlay = true,
						HideFoodOverlay = true
					};
					m_inventoryGrid.Children.Add(widget);
					m_inventoryGrid.SetWidgetCell(widget, new Point2(j, i));
				}
			}
		}

		public override void Update()
		{
			if (m_assignedCategoryIndex >= 0)
			{
				if (base.Input.Scroll.HasValue)
				{
					Widget widget = HitTestGlobal(base.Input.Scroll.Value.XY);
					if (widget != null && widget.IsChildWidgetOf(m_inventoryGrid))
					{
						m_componentCreativeInventory.PageIndex -= (int)base.Input.Scroll.Value.Z;
					}
				}
				if (m_creativeInventoryWidget.PageDownButton.IsClicked)
				{
					int num = ++m_componentCreativeInventory.PageIndex;
				}
				if (m_creativeInventoryWidget.PageUpButton.IsClicked)
				{
					int num = --m_componentCreativeInventory.PageIndex;
				}
				m_componentCreativeInventory.PageIndex = ((m_pagesCount > 0) ? MathUtils.Clamp(m_componentCreativeInventory.PageIndex, 0, m_pagesCount - 1) : 0);
			}
			if (m_componentCreativeInventory.CategoryIndex != m_assignedCategoryIndex)
			{
				if (m_creativeInventoryWidget.GetCategoryName(m_componentCreativeInventory.CategoryIndex) == LanguageControl.Get("CreativeInventoryWidget", 2))
				{
					m_slotIndices = new List<int>(Enumerable.Range(10, m_componentCreativeInventory.OpenSlotsCount - 10));
				}
				else
				{
					m_slotIndices.Clear();
					for (int i = m_componentCreativeInventory.OpenSlotsCount; i < m_componentCreativeInventory.SlotsCount; i++)
					{
						int slotValue = m_componentCreativeInventory.GetSlotValue(i);
						int num2 = Terrain.ExtractContents(slotValue);
						if (BlocksManager.Blocks[num2].GetCategory(slotValue) == m_creativeInventoryWidget.GetCategoryName(m_componentCreativeInventory.CategoryIndex))
						{
							m_slotIndices.Add(i);
						}
					}
				}
				int num3 = m_inventoryGrid.ColumnsCount * m_inventoryGrid.RowsCount;
				m_pagesCount = (m_slotIndices.Count + num3 - 1) / num3;
				m_assignedCategoryIndex = m_componentCreativeInventory.CategoryIndex;
				m_assignedPageIndex = -1;
				m_componentCreativeInventory.PageIndex = 0;
			}
			if (m_componentCreativeInventory.PageIndex != m_assignedPageIndex)
			{
				int num4 = m_inventoryGrid.ColumnsCount * m_inventoryGrid.RowsCount;
				int num5 = m_componentCreativeInventory.PageIndex * num4;
				foreach (Widget child in m_inventoryGrid.Children)
				{
					InventorySlotWidget inventorySlotWidget = child as InventorySlotWidget;
					if (inventorySlotWidget != null)
					{
						if (num5 < m_slotIndices.Count)
						{
							inventorySlotWidget.AssignInventorySlot(m_componentCreativeInventory, m_slotIndices[num5++]);
						}
						else
						{
							inventorySlotWidget.AssignInventorySlot(null, 0);
						}
					}
				}
				m_assignedPageIndex = m_componentCreativeInventory.PageIndex;
			}
			m_creativeInventoryWidget.PageLabel.Text = ((m_pagesCount > 0) ? $"{m_componentCreativeInventory.PageIndex + 1}/{m_pagesCount}" : string.Empty);
			m_creativeInventoryWidget.PageDownButton.IsEnabled = (m_componentCreativeInventory.PageIndex < m_pagesCount - 1);
			m_creativeInventoryWidget.PageUpButton.IsEnabled = (m_componentCreativeInventory.PageIndex > 0);
		}
	}
}
