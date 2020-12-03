using Engine;
using Engine.Graphics;

namespace Game
{
	public class DirtBlock : CubeBlock
	{
		public const int Index = 2;
        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            generator.GenerateCubeVertices(this, value, x, y, z, Color.White, geometry.OpaqueSubsetsByFace);
        }
    }
}
