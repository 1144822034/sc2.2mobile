using Engine;

namespace Game
{
	public class SubsystemTruthTableCircuitBlockBehavior : SubsystemEditableItemBehavior<TruthTableData>
	{
		public override int[] HandledBlocks => new int[1]
		{
			188
		};

		public SubsystemTruthTableCircuitBlockBehavior()
			: base(188)
		{
		}

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			int value = inventory.GetSlotValue(slotIndex);
			int count = inventory.GetSlotCount(slotIndex);
			int id = Terrain.ExtractData(value);
			TruthTableData truthTableData = GetItemData(id);
			if (truthTableData != null)
			{
				truthTableData = (TruthTableData)truthTableData.Copy();
			}
			else
			{
				truthTableData = new TruthTableData();
			}
			DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditTruthTableDialog(truthTableData, delegate
			{
				int data = StoreItemDataAtUniqueId(truthTableData);
				int value2 = Terrain.ReplaceData(value, data);
				inventory.RemoveSlotItems(slotIndex, count);
				inventory.AddSlotItems(slotIndex, value2, 1);
			}));
			return true;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			TruthTableData truthTableData = GetBlockData(new Point3(x, y, z)) ?? new TruthTableData();
			DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditTruthTableDialog(truthTableData, delegate
			{
				SetBlockData(new Point3(x, y, z), truthTableData);
				int face = ((TruthTableCircuitBlock)BlocksManager.Blocks[188]).GetFace(value);
				SubsystemElectricity subsystemElectricity = base.SubsystemTerrain.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
				ElectricElement electricElement = subsystemElectricity.GetElectricElement(x, y, z, face);
				if (electricElement != null)
				{
					subsystemElectricity.QueueElectricElementForSimulation(electricElement, subsystemElectricity.CircuitStep + 1);
				}
			}));
			return true;
		}
	}
}
