using Engine.Graphics;

namespace Game
{
	public class TerrainGeometry
	{
		public TerrainGeometrySubset SubsetOpaque;

		public TerrainGeometrySubset SubsetAlphaTest;

		public Texture2D texture2D;

		public TerrainGeometrySubset SubsetTransparent;

		public TerrainGeometrySubset[] OpaqueSubsetsByFace;

		public TerrainGeometrySubset[] AlphaTestSubsetsByFace;

		public TerrainGeometrySubset[] TransparentSubsetsByFace;

	}
}
