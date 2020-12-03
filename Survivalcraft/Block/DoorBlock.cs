using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public abstract class DoorBlock : Block, IElectricElementBlock
	{
		public float m_pivotDistance;

		public string m_modelName;

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public BlockMesh[] m_blockMeshesByData = new BlockMesh[16];

		public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[16][];

		public DoorBlock(string modelName, float pivotDistance)
		{
			m_modelName = modelName;
			m_pivotDistance = pivotDistance;
		}

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>(m_modelName);
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Door").ParentBone);
			for (int i = 0; i < 16; i++)
			{
				int rotation = GetRotation(i);
				bool open = GetOpen(i);
				bool rightHanded = GetRightHanded(i);
				float num = (!rightHanded) ? 1 : (-1);
				m_blockMeshesByData[i] = new BlockMesh();
				Matrix identity = Matrix.Identity;
				identity *= Matrix.CreateScale(0f - num, 1f, 1f);
				identity *= Matrix.CreateTranslation((0.5f - m_pivotDistance) * num, 0f, 0f) * Matrix.CreateRotationY(open ? (num * (float)Math.PI / 2f) : 0f) * Matrix.CreateTranslation((0f - (0.5f - m_pivotDistance)) * num, 0f, 0f);
				identity *= Matrix.CreateTranslation(0f, 0f, 0.5f - m_pivotDistance) * Matrix.CreateRotationY((float)rotation * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
				m_blockMeshesByData[i].AppendModelMeshPart(model.FindMesh("Door").MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, !rightHanded, doubleSided: false, flipNormals: false, Color.White);
				BoundingBox boundingBox = m_blockMeshesByData[i].CalculateBoundingBox();
				boundingBox.Max.Y = 1f;
				m_collisionBoxesByData[i] = new BoundingBox[1]
				{
					boundingBox
				};
			}
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Door").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -1f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (IsBottomPart(generator.Terrain, x, y, z) && num < m_blockMeshesByData.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, geometry.SubsetAlphaTest);
			}
			Vector2 centerOffset = GetRightHanded(num) ? new Vector2(-0.45f, 0f) : new Vector2(0.45f, 0f);
			generator.GenerateWireVertices(value, x, y, z, GetHingeFace(num), 0.01f, centerOffset, geometry.SubsetOpaque);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 0.75f * size, ref matrix, environmentData);
		}

		public override int GetShadowStrength(int value)
		{
			if (!GetOpen(Terrain.ExtractData(value)))
			{
				return DefaultShadowStrength;
			}
			return 4;
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			float num = Vector3.Dot(forward, Vector3.UnitZ);
			float num2 = Vector3.Dot(forward, Vector3.UnitX);
			float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
			float num4 = Vector3.Dot(forward, -Vector3.UnitX);
			int num5 = 0;
			if (num == MathUtils.Max(num, num2, num3, num4))
			{
				num5 = 2;
			}
			else if (num2 == MathUtils.Max(num, num2, num3, num4))
			{
				num5 = 3;
			}
			else if (num3 == MathUtils.Max(num, num2, num3, num4))
			{
				num5 = 0;
			}
			else if (num4 == MathUtils.Max(num, num2, num3, num4))
			{
				num5 = 1;
			}
			Point3 point = CellFace.FaceToPoint3(raycastResult.CellFace.Face);
			int num6 = raycastResult.CellFace.X + point.X;
			int y = raycastResult.CellFace.Y + point.Y;
			int num7 = raycastResult.CellFace.Z + point.Z;
			bool rightHanded = true;
			switch (num5)
			{
			case 0:
				rightHanded = BlocksManager.Blocks[subsystemTerrain.Terrain.GetCellContents(num6 - 1, y, num7)].IsTransparent;
				break;
			case 1:
				rightHanded = BlocksManager.Blocks[subsystemTerrain.Terrain.GetCellContents(num6, y, num7 + 1)].IsTransparent;
				break;
			case 2:
				rightHanded = BlocksManager.Blocks[subsystemTerrain.Terrain.GetCellContents(num6 + 1, y, num7)].IsTransparent;
				break;
			case 3:
				rightHanded = BlocksManager.Blocks[subsystemTerrain.Terrain.GetCellContents(num6, y, num7 - 1)].IsTransparent;
				break;
			}
			int data = SetRightHanded(SetOpen(SetRotation(0, num5), open: false), rightHanded);
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, BlockIndex), data);
			result.CellFace = raycastResult.CellFace;
			return result;
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

		public override bool ShouldAvoid(int value)
		{
			return !GetOpen(Terrain.ExtractData(value));
		}

		public override bool IsHeatBlocker(int value)
		{
			return !GetOpen(Terrain.ExtractData(value));
		}

		public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			return new DoorElectricElement(subsystemElectricity, new CellFace(x, y, z, GetHingeFace(data)));
		}

		public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int hingeFace = GetHingeFace(Terrain.ExtractData(value));
			if (face == hingeFace)
			{
				ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(hingeFace, 0, connectorFace);
				if (connectorDirection == ElectricConnectorDirection.Right || connectorDirection == ElectricConnectorDirection.Left || connectorDirection == ElectricConnectorDirection.In)
				{
					return ElectricConnectorType.Input;
				}
			}
			return null;
		}

		public int GetConnectionMask(int value)
		{
			return int.MaxValue;
		}

		public static int GetRotation(int data)
		{
			return data & 3;
		}

		public static bool GetOpen(int data)
		{
			return (data & 4) != 0;
		}

		public static bool GetRightHanded(int data)
		{
			return (data & 8) == 0;
		}

		public static int SetRotation(int data, int rotation)
		{
			return (data & -4) | (rotation & 3);
		}

		public static int SetOpen(int data, bool open)
		{
			if (!open)
			{
				return data & -5;
			}
			return data | 4;
		}

		public static int SetRightHanded(int data, bool rightHanded)
		{
			if (rightHanded)
			{
				return data & -9;
			}
			return data | 8;
		}

		public static bool IsTopPart(Terrain terrain, int x, int y, int z)
		{
			return BlocksManager.Blocks[terrain.GetCellContents(x, y - 1, z)] is DoorBlock;
		}

		public static bool IsBottomPart(Terrain terrain, int x, int y, int z)
		{
			return BlocksManager.Blocks[terrain.GetCellContents(x, y + 1, z)] is DoorBlock;
		}

		public static int GetHingeFace(int data)
		{
			int rotation = GetRotation(data);
			int num = (rotation - 1 < 0) ? 3 : (rotation - 1);
			if (!GetRightHanded(data))
			{
				num = CellFace.OppositeFace(num);
			}
			return num;
		}
	}
}
