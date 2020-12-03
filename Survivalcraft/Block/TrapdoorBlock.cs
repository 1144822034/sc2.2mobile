using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public abstract class TrapdoorBlock : Block, IElectricElementBlock
	{
		public string m_modelName;

		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		public BlockMesh[] m_blockMeshesByData = new BlockMesh[16];

		public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[16][];

		public TrapdoorBlock(string modelName)
		{
			m_modelName = modelName;
		}

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>(m_modelName);
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Trapdoor").ParentBone);
			for (int i = 0; i < 16; i++)
			{
				int rotation = GetRotation(i);
				bool open = GetOpen(i);
				bool upsideDown = GetUpsideDown(i);
				m_blockMeshesByData[i] = new BlockMesh();
				Matrix identity = Matrix.Identity;
				identity *= Matrix.CreateTranslation(0f, -0.0625f, 0.4375f) * Matrix.CreateRotationX(open ? (-(float)Math.PI / 2f) : 0f) * Matrix.CreateTranslation(0f, 0.0625f, -0.4375f);
				identity *= Matrix.CreateRotationZ(upsideDown ? ((float)Math.PI) : 0f);
				identity *= Matrix.CreateRotationY((float)rotation * (float)Math.PI / 2f);
				identity *= Matrix.CreateTranslation(new Vector3(0.5f, upsideDown ? 1 : 0, 0.5f));
				m_blockMeshesByData[i].AppendModelMeshPart(model.FindMesh("Trapdoor").MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
				m_blockMeshesByData[i].GenerateSidesData();
				m_collisionBoxesByData[i] = new BoundingBox[1]
				{
					m_blockMeshesByData[i].CalculateBoundingBox()
				};
			}
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Trapdoor").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_blockMeshesByData.Length)
			{
				generator.GenerateShadedMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, null, geometry.SubsetAlphaTest);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, size, ref matrix, environmentData);
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int rotation;
			bool upsideDown;
			if (raycastResult.CellFace.Face < 4)
			{
				rotation = raycastResult.CellFace.Face;
				upsideDown = (raycastResult.HitPoint().Y - (float)raycastResult.CellFace.Y > 0.5f);
			}
			else
			{
				Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
				float num = Vector3.Dot(forward, Vector3.UnitZ);
				float num2 = Vector3.Dot(forward, Vector3.UnitX);
				float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
				float num4 = Vector3.Dot(forward, -Vector3.UnitX);
				rotation = ((num == MathUtils.Max(num, num2, num3, num4)) ? 2 : ((num2 == MathUtils.Max(num, num2, num3, num4)) ? 3 : ((num3 != MathUtils.Max(num, num2, num3, num4)) ? ((num4 == MathUtils.Max(num, num2, num3, num4)) ? 1 : 0) : 0)));
				upsideDown = (raycastResult.CellFace.Face == 5);
			}
			int data = SetOpen(SetRotation(SetUpsideDown(0, upsideDown), rotation), open: false);
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
			return base.GetCustomCollisionBoxes(terrain, value);
		}

		public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			return new TrapDoorElectricElement(subsystemElectricity, new CellFace(x, y, z, GetMountingFace(data)));
		}

		public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			if (face == GetMountingFace(data))
			{
				int rotation = GetRotation(data);
				if (SubsystemElectricity.GetConnectorDirection(4, (4 - rotation) % 4, connectorFace) == ElectricConnectorDirection.Top)
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

		public static bool GetUpsideDown(int data)
		{
			return (data & 8) != 0;
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

		public static int SetUpsideDown(int data, bool upsideDown)
		{
			if (!upsideDown)
			{
				return data & -9;
			}
			return data | 8;
		}

		public static int GetMountingFace(int data)
		{
			if (!GetUpsideDown(data))
			{
				return 4;
			}
			return 5;
		}
	}
}
