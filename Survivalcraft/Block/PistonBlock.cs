using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Game
{
	public class PistonBlock : Block, IElectricElementBlock
	{
		public const int Index = 237;

		public BlockMesh[] m_standaloneBlockMeshes = new BlockMesh[4];

		public BlockMesh[] m_blockMeshesByData = new BlockMesh[48];

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Pistons");
			for (PistonMode pistonMode = PistonMode.Pushing; pistonMode <= PistonMode.StrictPulling; pistonMode++)
			{
				for (int i = 0; i < 2; i++)
				{
					string name = (i == 0) ? "PistonRetracted" : "PistonExtended";
					Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone);
					for (int j = 0; j < 6; j++)
					{
						int num = SetFace(SetIsExtended(SetMode(0, pistonMode), i != 0), j);
						Matrix m = (j < 4) ? (Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationY((float)j * (float)Math.PI / 2f + (float)Math.PI) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)) : ((j != 4) ? (Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)) : (Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)));
						m_blockMeshesByData[num] = new BlockMesh();
						m_blockMeshesByData[num].AppendModelMeshPart(model.FindMesh(name).MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
						if (i == 0)
						{
							switch (pistonMode)
							{
							case PistonMode.Pulling:
								m_blockMeshesByData[num].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.0625f, 0f), 1 << j);
								break;
							case PistonMode.StrictPulling:
								m_blockMeshesByData[num].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.125f, 0f), 1 << j);
								break;
							}
						}
					}
				}
				Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("PistonRetracted").ParentBone);
				m_standaloneBlockMeshes[(int)pistonMode] = new BlockMesh();
				m_standaloneBlockMeshes[(int)pistonMode].AppendModelMeshPart(model.FindMesh("PistonRetracted").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
				switch (pistonMode)
				{
				case PistonMode.Pulling:
					m_standaloneBlockMeshes[(int)pistonMode].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.0625f, 0f), 4);
					break;
				case PistonMode.StrictPulling:
					m_standaloneBlockMeshes[(int)pistonMode].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.125f, 0f), 4);
					break;
				}
			}
		}

		public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
		{
			int data = Terrain.ExtractData(value);
			int face2 = GetFace(data);
			if (GetIsExtended(data))
			{
				if (face != face2)
				{
					return face != CellFace.OppositeFace(face2);
				}
				return false;
			}
			return false;
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value) & 0x3F;
			if (num < m_blockMeshesByData.Length && m_blockMeshesByData[num] != null)
			{
				generator.GenerateShadedMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, null, geometry.SubsetOpaque);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			int mode = (int)GetMode(Terrain.ExtractData(value));
			if (mode < m_standaloneBlockMeshes.Length && m_standaloneBlockMeshes[mode] != null)
			{
				BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMeshes[mode], color, 1f * size, ref matrix, environmentData);
			}
		}

		public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new PistonElectricElement(subsystemElectricity, new Point3(x, y, z));
		}

		public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			return ElectricConnectorType.Input;
		}

		public int GetConnectionMask(int value)
		{
			return int.MaxValue;
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			yield return Terrain.MakeBlockValue(237, 0, SetMode(SetMaxExtension(0, 7), PistonMode.Pushing));
			yield return Terrain.MakeBlockValue(237, 0, SetMode(SetMaxExtension(0, 7), PistonMode.Pulling));
			yield return Terrain.MakeBlockValue(237, 0, SetMode(SetMaxExtension(0, 7), PistonMode.StrictPulling));
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			switch (GetMode(Terrain.ExtractData(value)))
			{
			case PistonMode.Pulling:
				return "ճ�Ի���";
			case PistonMode.StrictPulling:
				return "�ϸ�ճ�Ի���";
			default:
				return "����";
			}
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			float num = float.PositiveInfinity;
			int face = 0;
			for (int i = 0; i < 6; i++)
			{
				float num2 = Vector3.Dot(CellFace.FaceToVector3(i), forward);
				if (num2 < num)
				{
					num = num2;
					face = i;
				}
			}
			int data = Terrain.ExtractData(value);
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetFace(data, face));
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int data = Terrain.ExtractData(oldValue);
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(237, 0, SetFace(SetIsExtended(data, isExtended: false), 0)),
				Count = 1
			});
			showDebris = true;
		}

		public static bool GetIsExtended(int data)
		{
			return (data & 1) != 0;
		}

		public static int SetIsExtended(int data, bool isExtended)
		{
			return (data & -2) | (isExtended ? 1 : 0);
		}

		public static PistonMode GetMode(int data)
		{
			return (PistonMode)((data >> 1) & 3);
		}

		public static int SetMode(int data, PistonMode mode)
		{
			return (data & -7) | ((int)(mode & (PistonMode)3) << 1);
		}

		public static int GetFace(int data)
		{
			return (data >> 3) & 7;
		}

		public static int SetFace(int data, int face)
		{
			return (data & -57) | ((face & 7) << 3);
		}

		public static int GetMaxExtension(int data)
		{
			return (data >> 6) & 7;
		}

		public static int SetMaxExtension(int data, int maxExtension)
		{
			return (data & -449) | ((maxExtension & 7) << 6);
		}

		public static int GetPullCount(int data)
		{
			return (data >> 9) & 7;
		}

		public static int SetPullCount(int data, int pullCount)
		{
			return (data & -3585) | ((pullCount & 7) << 9);
		}

		public static int GetSpeed(int data)
		{
			return (data >> 12) & 7;
		}

		public static int SetSpeed(int data, int speed)
		{
			return (data & -12289) | ((speed & 3) << 12);
		}
	}
}
