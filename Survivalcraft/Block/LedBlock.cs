using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Game
{
	public class LedBlock : MountedElectricElementBlock
	{
		public const int Index = 152;

		public static readonly Color[] LedColors = new Color[8]
		{
			new Color(255, 255, 255),
			new Color(0, 255, 255),
			new Color(255, 0, 0),
			new Color(0, 0, 255),
			new Color(255, 240, 0),
			new Color(0, 255, 0),
			new Color(255, 120, 0),
			new Color(255, 0, 255)
		};


		public BlockMesh[] m_standaloneBlockMeshesByColor = new BlockMesh[8];

		public BlockMesh[] m_blockMeshesByData = new BlockMesh[64];

		public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[64][];

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Leds");
			ModelMesh modelMesh = model.FindMesh("Led");
			ModelMesh modelMesh2 = model.FindMesh("LedBulb");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(modelMesh.ParentBone);
			Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(modelMesh2.ParentBone);
			for (int i = 0; i < 8; i++)
			{
				Color color = LedColors[i];
				color *= 0.5f;
				color.A = byte.MaxValue;
				Matrix m = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
				m_standaloneBlockMeshesByColor[i] = new BlockMesh();
				m_standaloneBlockMeshesByColor[i].AppendModelMeshPart(modelMesh.MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
				m_standaloneBlockMeshesByColor[i].AppendModelMeshPart(modelMesh2.MeshParts[0], boneAbsoluteTransform2 * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
				for (int j = 0; j < 6; j++)
				{
					int num = SetMountingFace(SetColor(0, i), j);
					Matrix m2 = (j >= 4) ? ((j != 4) ? (Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f)) : Matrix.CreateTranslation(0.5f, 0f, 0.5f)) : (Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY((float)j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f));
					m_blockMeshesByData[num] = new BlockMesh();
					m_blockMeshesByData[num].AppendModelMeshPart(modelMesh.MeshParts[0], boneAbsoluteTransform * m2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
					m_blockMeshesByData[num].AppendModelMeshPart(modelMesh2.MeshParts[0], boneAbsoluteTransform2 * m2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
					m_collisionBoxesByData[num] = new BoundingBox[1]
					{
						m_blockMeshesByData[num].CalculateBoundingBox()
					};
				}
			}
		}

		public override IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
		{
			int color = 0;
			while (color < 8)
			{
				CraftingRecipe craftingRecipe = new CraftingRecipe
				{
					ResultCount = 4,
					ResultValue = Terrain.MakeBlockValue(152, 0, SetColor(0, color)),
					RemainsCount = 1,
					RemainsValue = Terrain.MakeBlockValue(90),
					RequiredHeatLevel = 0f,
					Description = LanguageControl.Get(GetType().Name,1)
				};
				craftingRecipe.Ingredients[1] = "glass";
				craftingRecipe.Ingredients[4] = "paintbucket:" + color.ToString(CultureInfo.InvariantCulture);
				craftingRecipe.Ingredients[6] = "copperingot";
				craftingRecipe.Ingredients[7] = "copperingot";
				craftingRecipe.Ingredients[8] = "copperingot";
				yield return craftingRecipe;
				int num = color + 1;
				color = num;
			}
		}

		public override int GetFace(int value)
		{
			return GetMountingFace(Terrain.ExtractData(value));
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int data = Terrain.ExtractData(value);
			int color = GetColor(data);
			return LanguageControl.Get("LedBlock", color) + LanguageControl.GetBlock(string.Format("{0}:{1}", GetType().Name, data.ToString()), "DisplayName");
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			int i = 0;
			while (i < 8)
			{
				yield return Terrain.MakeBlockValue(152, 0, SetColor(0, i));
				int num = i + 1;
				i = num;
			}
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int data = SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face);
			int value2 = Terrain.ReplaceData(value, data);
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = value2;
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int color = GetColor(Terrain.ExtractData(oldValue));
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(152, 0, SetColor(0, color)),
				Count = 1
			});
			showDebris = true;
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int num = Terrain.ExtractData(value);
			if (num >= m_collisionBoxesByData.Length)
			{
				return null;
			}
			return m_collisionBoxesByData[num];
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int num = Terrain.ExtractData(value);
			if (num < m_blockMeshesByData.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, geometry.SubsetOpaque);
				generator.GenerateWireVertices(value, x, y, z, GetFace(value), 0.25f, Vector2.Zero, geometry.SubsetOpaque);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			int color2 = GetColor(Terrain.ExtractData(value));
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMeshesByColor[color2], color, 2f * size, ref matrix, environmentData);
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new LedElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int face2 = GetFace(value);
			if (face == face2 && SubsystemElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue)
			{
				return ElectricConnectorType.Input;
			}
			return null;
		}

		public static int GetMountingFace(int data)
		{
			return data & 7;
		}

		public static int SetMountingFace(int data, int face)
		{
			return (data & -8) | (face & 7);
		}

		public static int GetColor(int data)
		{
			return (data >> 3) & 7;
		}

		public static int SetColor(int data, int color)
		{
			return (data & -57) | ((color & 7) << 3);
		}
	}
}
