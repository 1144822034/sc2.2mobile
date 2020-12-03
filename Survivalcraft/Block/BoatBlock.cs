using Engine;
using Engine.Graphics;

namespace Game
{
	public class BoatBlock : Block
	{
		public const int Index = 178;

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/BoatItem");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Boat").ParentBone);
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Boat").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.4f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(96, 96, 96));
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Boat").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.4f, 0f), makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, new Color(255, 255, 255));
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 1f * size, ref matrix, environmentData);
		}
	}
}
