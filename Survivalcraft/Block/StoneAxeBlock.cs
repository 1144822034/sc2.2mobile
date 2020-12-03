using Engine;
using Engine.Graphics;

namespace Game
{
	public class StoneAxeBlock : Block
	{
		public const int Index = 29;

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public override void Initialize()
		{
			int num = 47;
			int num2 = 1;
			Model model = ContentManager.Get<Model>("Models/StoneAxe");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Handle").ParentBone);
			Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Head").ParentBone);
			BlockMesh blockMesh = new BlockMesh();
			blockMesh.AppendModelMeshPart(model.FindMesh("Handle").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation((float)(num % 16) / 16f, (float)(num / 16) / 16f, 0f));
			BlockMesh blockMesh2 = new BlockMesh();
			blockMesh2.AppendModelMeshPart(model.FindMesh("Head").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			blockMesh2.TransformTextureCoordinates(Matrix.CreateTranslation((float)(num2 % 16) / 16f, (float)(num2 / 16) / 16f, 0f));
			m_standaloneBlockMesh.AppendBlockMesh(blockMesh);
			m_standaloneBlockMesh.AppendBlockMesh(blockMesh2);
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
		}
	}
}
