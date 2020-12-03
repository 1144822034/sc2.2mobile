using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public class JackOLanternBlock : Block
	{
		public const int Index = 132;

		public BlockMesh[] m_blockMeshesByData = new BlockMesh[4];

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public BoundingBox[] m_collisionBoxes = new BoundingBox[1];

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Pumpkins");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("JackOLantern").ParentBone);
			for (int i = 0; i < 4; i++)
			{
				float radians = (float)i * (float)Math.PI / 2f;
				BlockMesh blockMesh = new BlockMesh();
				blockMesh.AppendModelMeshPart(model.FindMesh("JackOLantern").MeshParts[0], boneAbsoluteTransform * Matrix.CreateRotationY(radians) * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(232, 232, 232));
				blockMesh.AppendModelMeshPart(model.FindMesh("JackOLantern").MeshParts[0], boneAbsoluteTransform * Matrix.CreateRotationY(radians) * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: true, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
				m_blockMeshesByData[i] = blockMesh;
			}
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("JackOLantern").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.23f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(232, 232, 232));
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("JackOLantern").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.23f, 0f), makeEmissive: true, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
			m_collisionBoxes[0] = m_blockMeshesByData[0].CalculateBoundingBox();
			base.Initialize();
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			float num = Vector3.Dot(forward, Vector3.UnitZ);
			float num2 = Vector3.Dot(forward, Vector3.UnitX);
			float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
			float num4 = Vector3.Dot(forward, -Vector3.UnitX);
			int data = 0;
			if (num == MathUtils.Max(num, num2, num3, num4))
			{
				data = 0;
			}
			else if (num2 == MathUtils.Max(num, num2, num3, num4))
			{
				data = 1;
			}
			else if (num3 == MathUtils.Max(num, num2, num3, num4))
			{
				data = 2;
			}
			else if (num4 == MathUtils.Max(num, num2, num3, num4))
			{
				data = 3;
			}
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, 132), data);
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			return m_collisionBoxes;
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_blockMeshesByData.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, geometry.SubsetAlphaTest);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
		}
	}
}
