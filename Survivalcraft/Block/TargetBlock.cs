using Engine;
using Engine.Graphics;

namespace Game
{
	public class TargetBlock : MountedElectricElementBlock
	{
		public const int Index = 199;

		public BoundingBox[][] m_boundingBoxes = new BoundingBox[4][]
		{
			new BoundingBox[1]
			{
				new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0.0625f))
			},
			new BoundingBox[1]
			{
				new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(0.0625f, 1f, 1f))
			},
			new BoundingBox[1]
			{
				new BoundingBox(new Vector3(0f, 0f, 0.9375f), new Vector3(1f, 1f, 1f))
			},
			new BoundingBox[1]
			{
				new BoundingBox(new Vector3(0.9375f, 0f, 0f), new Vector3(1f, 1f, 1f))
			}
		};

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int mountingFace = GetMountingFace(Terrain.ExtractData(value));
			if (mountingFace >= 0 && mountingFace < 4)
			{
				return m_boundingBoxes[mountingFace];
			}
			return base.GetCustomCollisionBoxes(terrain, value);
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			BlockPlacementData result = default(BlockPlacementData);
			if (raycastResult.CellFace.Face < 4)
			{
				result.CellFace = raycastResult.CellFace;
				result.Value = Terrain.MakeBlockValue(199, 0, SetMountingFace(0, raycastResult.CellFace.Face));
			}
			return result;
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			TerrainGeometrySubset subsetAlphaTest = geometry.SubsetAlphaTest;
			DynamicArray<TerrainVertex> vertices = subsetAlphaTest.Vertices;
			DynamicArray<ushort> indices = subsetAlphaTest.Indices;
			int count = vertices.Count;
			int data = Terrain.ExtractData(value);
			int num = Terrain.ExtractLight(value);
			int mountingFace = GetMountingFace(data);
			float s = LightingManager.LightIntensityByLightValueAndFace[num + 16 * mountingFace];
			Color color = Color.White * s;
			switch (mountingFace)
			{
			case 2:
				vertices.Count += 4;
				BlockGeometryGenerator.SetupLitCornerVertex(x, y, z + 1, color, DefaultTextureSlot, 0, ref vertices.Array[count]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y, z + 1, color, DefaultTextureSlot, 1, ref vertices.Array[count + 1]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y + 1, z + 1, color, DefaultTextureSlot, 2, ref vertices.Array[count + 2]);
				BlockGeometryGenerator.SetupLitCornerVertex(x, y + 1, z + 1, color, DefaultTextureSlot, 3, ref vertices.Array[count + 3]);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)count);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)(count + 2));
				break;
			case 3:
				vertices.Count += 4;
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y, z, color, DefaultTextureSlot, 0, ref vertices.Array[count]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y + 1, z, color, DefaultTextureSlot, 3, ref vertices.Array[count + 1]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y + 1, z + 1, color, DefaultTextureSlot, 2, ref vertices.Array[count + 2]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y, z + 1, color, DefaultTextureSlot, 1, ref vertices.Array[count + 3]);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)count);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)(count + 2));
				break;
			case 0:
				vertices.Count += 4;
				BlockGeometryGenerator.SetupLitCornerVertex(x, y, z, color, DefaultTextureSlot, 0, ref vertices.Array[count]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y, z, color, DefaultTextureSlot, 1, ref vertices.Array[count + 1]);
				BlockGeometryGenerator.SetupLitCornerVertex(x + 1, y + 1, z, color, DefaultTextureSlot, 2, ref vertices.Array[count + 2]);
				BlockGeometryGenerator.SetupLitCornerVertex(x, y + 1, z, color, DefaultTextureSlot, 3, ref vertices.Array[count + 3]);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				break;
			case 1:
				vertices.Count += 4;
				BlockGeometryGenerator.SetupLitCornerVertex(x, y, z, color, DefaultTextureSlot, 0, ref vertices.Array[count]);
				BlockGeometryGenerator.SetupLitCornerVertex(x, y + 1, z, color, DefaultTextureSlot, 3, ref vertices.Array[count + 1]);
				BlockGeometryGenerator.SetupLitCornerVertex(x, y + 1, z + 1, color, DefaultTextureSlot, 2, ref vertices.Array[count + 2]);
				BlockGeometryGenerator.SetupLitCornerVertex(x, y, z + 1, color, DefaultTextureSlot, 1, ref vertices.Array[count + 3]);
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 1));
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)(count + 3));
				indices.Add((ushort)count);
				indices.Add((ushort)(count + 2));
				break;
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color, isEmissive: false, environmentData);
		}

		public static int GetMountingFace(int data)
		{
			return data & 3;
		}

		public static int SetMountingFace(int data, int face)
		{
			return (data & -4) | (face & 3);
		}

		public override int GetFace(int value)
		{
			return GetMountingFace(Terrain.ExtractData(value));
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new TargetElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int face2 = GetFace(value);
			if (face == face2 && SubsystemElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue)
			{
				return ElectricConnectorType.Output;
			}
			return null;
		}
	}
}
