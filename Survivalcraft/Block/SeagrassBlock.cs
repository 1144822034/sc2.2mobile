using Engine;
using Engine.Graphics;

namespace Game
{
	public class SeagrassBlock : WaterPlantBlock
	{
		public new const int Index = 233;

		public override int GetFaceTextureSlot(int face, int value)
		{
			if (face < 0)
			{
				return 105;
			}
			return base.GetFaceTextureSlot(face, value);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color * BlockColorsMap.SeagrassColorsMap.Lookup(environmentData.Temperature, environmentData.Humidity), isEmissive: false, environmentData);
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			Color color = BlockColorsMap.SeagrassColorsMap.Lookup(generator.Terrain, x, y, z);
			generator.GenerateCrossfaceVertices(this, value, x, y, z, color, GetFaceTextureSlot(-1, value), geometry.SubsetAlphaTest);
			base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return new BlockDebrisParticleSystem(subsystemTerrain, position, 0.75f * strength, DestructionDebrisScale, BlockColorsMap.SeagrassColorsMap.Lookup(subsystemTerrain.Terrain, Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z)), 104);
		}
	}
}
