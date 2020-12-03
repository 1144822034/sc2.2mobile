using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class TallGrassBlock : CrossBlock
	{
		public const int Index = 19;

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int data = Terrain.ExtractData(oldValue);
			if (!GetIsSmall(data))
			{
				dropValues.Add(new BlockDropValue
				{
					Value = Terrain.MakeBlockValue(19, 0, data),
					Count = 1
				});
			}
			showDebris = true;
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			if (!GetIsSmall(Terrain.ExtractData(value)))
			{
				return 85;
			}
			return 84;
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color * BlockColorsMap.GrassColorsMap.Lookup(environmentData.Temperature, environmentData.Humidity), isEmissive: false, environmentData);
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			generator.GenerateCrossfaceVertices(this, value, x, y, z, BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x, y, z), GetFaceTextureSlot(0, value), geometry.SubsetAlphaTest);
		}

		public override int GetShadowStrength(int value)
		{
			if (!GetIsSmall(Terrain.ExtractData(value)))
			{
				return DefaultShadowStrength;
			}
			return DefaultShadowStrength / 2;
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			Color color = BlockColorsMap.GrassColorsMap.Lookup(subsystemTerrain.Terrain, Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
			return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, color, GetFaceTextureSlot(4, value));
		}

		public static bool GetIsSmall(int data)
		{
			return (data & 8) != 0;
		}

		public static int SetIsSmall(int data, bool isSmall)
		{
			if (!isSmall)
			{
				return data & -9;
			}
			return data | 8;
		}
	}
}
