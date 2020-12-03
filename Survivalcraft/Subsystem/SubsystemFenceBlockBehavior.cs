namespace Game
{
	public class SubsystemFenceBlockBehavior : SubsystemBlockBehavior
	{
		public override int[] HandledBlocks => new int[0];

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			UpdateVariant(cellValue, x, y, z);
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			UpdateVariant(value, x, y, z);
		}

		public void UpdateVariant(int value, int x, int y, int z)
		{
			int num = Terrain.ExtractContents(value);
			FenceBlock fenceBlock = BlocksManager.Blocks[num] as FenceBlock;
			if (fenceBlock != null)
			{
				int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x + 1, y, z);
				int cellValue2 = base.SubsystemTerrain.Terrain.GetCellValue(x - 1, y, z);
				int cellValue3 = base.SubsystemTerrain.Terrain.GetCellValue(x, y, z + 1);
				int cellValue4 = base.SubsystemTerrain.Terrain.GetCellValue(x, y, z - 1);
				int num2 = 0;
				if (fenceBlock.ShouldConnectTo(cellValue))
				{
					num2++;
				}
				if (fenceBlock.ShouldConnectTo(cellValue2))
				{
					num2 += 2;
				}
				if (fenceBlock.ShouldConnectTo(cellValue3))
				{
					num2 += 4;
				}
				if (fenceBlock.ShouldConnectTo(cellValue4))
				{
					num2 += 8;
				}
				int data = Terrain.ExtractData(value);
				int value2 = Terrain.ReplaceData(value, FenceBlock.SetVariant(data, num2));
				base.SubsystemTerrain.ChangeCell(x, y, z, value2);
			}
		}
	}
}
