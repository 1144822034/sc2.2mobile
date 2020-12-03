using TemplatesDatabase;

namespace Game
{
	public class SubsystemCarpetBlockBehavior : SubsystemPollableBlockBehavior
	{
		public SubsystemWeather m_subsystemWeather;

		public Random m_random = new Random();

		public override int[] HandledBlocks => new int[0];

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemWeather = base.Project.FindSubsystem<SubsystemWeather>(throwOnError: true);
			base.Load(valuesDictionary);
		}

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellContents = base.SubsystemTerrain.Terrain.GetCellContents(x, y - 1, z);
			if (BlocksManager.Blocks[cellContents].IsTransparent)
			{
				base.SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}

		public override void OnPoll(int value, int x, int y, int z, int pollPass)
		{
			if (m_random.Float(0f, 1f) < 0.25f)
			{
				PrecipitationShaftInfo precipitationShaftInfo = m_subsystemWeather.GetPrecipitationShaftInfo(x, z);
				if (precipitationShaftInfo.Intensity > 0f && y >= precipitationShaftInfo.YLimit - 1)
				{
					base.SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: true, noParticleSystem: false);
				}
			}
		}
	}
}
