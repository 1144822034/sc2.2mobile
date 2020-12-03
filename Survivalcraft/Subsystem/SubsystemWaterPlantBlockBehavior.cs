namespace Game
{
	public class SubsystemWaterPlantBlockBehavior : SubsystemInWaterBlockBehavior
	{
		public override int[] HandledBlocks => new int[0];

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			base.OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
			int num = Terrain.ExtractContents(base.SubsystemTerrain.Terrain.GetCellValue(x, y, z));
			int num2 = Terrain.ExtractContents(base.SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z));
			if (num2 != 2 && num2 != 7 && num2 != 72 && num2 != num)
			{
				base.SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}
	}
}
