using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public class MagnetBlock : Block
	{
		public const int Index = 167;

		public BlockMesh[] m_meshesByData = new BlockMesh[2];

		public BlockMesh m_standaloneMesh = new BlockMesh();

		public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[2][];

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Magnet");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Magnet").ParentBone);
			for (int i = 0; i < 2; i++)
			{
				m_meshesByData[i] = new BlockMesh();
				m_meshesByData[i].AppendModelMeshPart(model.FindMesh("Magnet").MeshParts[0], boneAbsoluteTransform * Matrix.CreateRotationY((float)Math.PI / 2f * (float)i) * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
				m_collisionBoxesByData[i] = new BoundingBox[1]
				{
					m_meshesByData[i].CalculateBoundingBox()
				};
			}
			m_standaloneMesh.AppendModelMeshPart(model.FindMesh("Magnet").MeshParts[0], boneAbsoluteTransform * Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0f, -0.25f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
			base.Initialize();
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_collisionBoxesByData.Length)
			{
				return m_collisionBoxesByData[num];
			}
			return null;
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_collisionBoxesByData.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[num], Color.White, null, geometry.SubsetOpaque);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneMesh, color, size, ref matrix, environmentData);
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			BlockPlacementData result;
			if (componentMiner.Project.FindSubsystem<SubsystemMagnetBlockBehavior>(throwOnError: true).MagnetsCount < 8)
			{
				Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
				int data = (!(MathUtils.Abs(forward.X) > MathUtils.Abs(forward.Z))) ? 1 : 0;
				result = default(BlockPlacementData);
				result.CellFace = raycastResult.CellFace;
				result.Value = Terrain.ReplaceData(value, data);
				return result;
			}
			componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage("Too many magnets", Color.White, blinking: true, playNotificationSound: false);
			result = default(BlockPlacementData);
			return result;
		}
	}
}
