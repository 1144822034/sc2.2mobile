using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public class TorchBlock : Block
	{
		public const int Index = 31;

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public BlockMesh[] m_blockMeshesByVariant = new BlockMesh[5];

		public BoundingBox[][] m_collisionBoxes = new BoundingBox[5][];

		public override void Initialize()
		{
			for (int i = 0; i < m_blockMeshesByVariant.Length; i++)
			{
				m_blockMeshesByVariant[i] = new BlockMesh();
			}
			Model model = ContentManager.Get<Model>("Models/Torch");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Torch").ParentBone);
			Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Flame").ParentBone);
			Matrix m = Matrix.CreateRotationX(0.6f) * Matrix.CreateRotationY(0f) * Matrix.CreateTranslation(0.5f, 0.15f, -0.05f);
			m_blockMeshesByVariant[0].AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_blockMeshesByVariant[0].AppendModelMeshPart(model.FindMesh("Flame").MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m = Matrix.CreateRotationX(0.6f) * Matrix.CreateRotationY((float)Math.PI / 2f) * Matrix.CreateTranslation(-0.05f, 0.15f, 0.5f);
			m_blockMeshesByVariant[1].AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_blockMeshesByVariant[1].AppendModelMeshPart(model.FindMesh("Flame").MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m = Matrix.CreateRotationX(0.6f) * Matrix.CreateRotationY((float)Math.PI) * Matrix.CreateTranslation(0.5f, 0.15f, 1.05f);
			m_blockMeshesByVariant[2].AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_blockMeshesByVariant[2].AppendModelMeshPart(model.FindMesh("Flame").MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m = Matrix.CreateRotationX(0.6f) * Matrix.CreateRotationY(4.712389f) * Matrix.CreateTranslation(1.05f, 0.15f, 0.5f);
			m_blockMeshesByVariant[3].AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_blockMeshesByVariant[3].AppendModelMeshPart(model.FindMesh("Flame").MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m = Matrix.CreateTranslation(0.5f, 0f, 0.5f);
			m_blockMeshesByVariant[4].AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_blockMeshesByVariant[4].AppendModelMeshPart(model.FindMesh("Flame").MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Torch").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.25f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			for (int j = 0; j < 5; j++)
			{
				m_collisionBoxes[j] = new BoundingBox[1]
				{
					m_blockMeshesByVariant[j].CalculateBoundingBox()
				};
			}
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_blockMeshesByVariant.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByVariant[num], Color.White, null, geometry.SubsetOpaque);
			}
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int value2 = 0;
			if (raycastResult.CellFace.Face == 0)
			{
				value2 = Terrain.ReplaceData(Terrain.ReplaceContents(0, 31), 0);
			}
			if (raycastResult.CellFace.Face == 1)
			{
				value2 = Terrain.ReplaceData(Terrain.ReplaceContents(0, 31), 1);
			}
			if (raycastResult.CellFace.Face == 2)
			{
				value2 = Terrain.ReplaceData(Terrain.ReplaceContents(0, 31), 2);
			}
			if (raycastResult.CellFace.Face == 3)
			{
				value2 = Terrain.ReplaceData(Terrain.ReplaceContents(0, 31), 3);
			}
			if (raycastResult.CellFace.Face == 4)
			{
				value2 = Terrain.ReplaceData(Terrain.ReplaceContents(0, 31), 4);
			}
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = value2;
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_collisionBoxes.Length)
			{
				return m_collisionBoxes[num];
			}
			return null;
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
		}
	}
}
