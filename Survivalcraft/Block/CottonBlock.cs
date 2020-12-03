using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class CottonBlock : CrossBlock
	{
		public const int Index = 204;

		public override IEnumerable<int> GetCreativeValues()
		{
			yield return Terrain.MakeBlockValue(204, 0, SetIsWild(SetSize(0, 2), isWild: true));
			yield return Terrain.MakeBlockValue(204, 0, SetIsWild(SetSize(0, 1), isWild: false));
			yield return Terrain.MakeBlockValue(204, 0, SetIsWild(SetSize(0, 2), isWild: false));
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			if (!GetIsWild(Terrain.ExtractData(value)))
			{
				return "�޻�";
			}
			return "Ұ���޻�";
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int data = Terrain.ExtractData(oldValue);
			if (GetSize(data) == 2)
			{
				BlockDropValue item = new BlockDropValue
				{
					Value = Terrain.MakeBlockValue(173, 0, 6),
					Count = Random.Int(1, 2)
				};
				dropValues.Add(item);
				if (!GetIsWild(data))
				{
					int num = Random.Int(1, 2);
					for (int i = 0; i < num; i++)
					{
						item = new BlockDropValue
						{
							Value = Terrain.MakeBlockValue(205, 0, 0),
							Count = 1
						};
						dropValues.Add(item);
					}
					if (Random.Bool(0.5f))
					{
						item = new BlockDropValue
						{
							Value = Terrain.MakeBlockValue(248),
							Count = 1
						};
						dropValues.Add(item);
					}
				}
			}
			showDebris = true;
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			switch (GetSize(Terrain.ExtractData(value)))
			{
			case 0:
				return 11;
			case 1:
				return 29;
			default:
				return 30;
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			if (GetIsWild(Terrain.ExtractData(value)))
			{
				color *= BlockColorsMap.GrassColorsMap.Lookup(environmentData.Temperature, environmentData.Humidity);
				BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color, isEmissive: false, environmentData);
			}
			else
			{
				BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color, isEmissive: false, environmentData);
			}
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			if (GetIsWild(Terrain.ExtractData(value)))
			{
				Color color = BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x, y, z);
				generator.GenerateCrossfaceVertices(this, value, x, y, z, color, GetFaceTextureSlot(0, value), geometry.SubsetAlphaTest);
			}
			else
			{
				generator.GenerateCrossfaceVertices(this, value, x, y, z, Color.White, GetFaceTextureSlot(0, value), geometry.SubsetAlphaTest);
			}
		}

		public override int GetShadowStrength(int value)
		{
			int size = GetSize(Terrain.ExtractData(value));
			return 2 + size * 2;
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			if (GetIsWild(Terrain.ExtractData(value)))
			{
				Color color = BlockColorsMap.GrassColorsMap.Lookup(subsystemTerrain.Terrain, Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
				return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, color, GetFaceTextureSlot(4, value));
			}
			return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, Color.White, GetFaceTextureSlot(4, value));
		}

		public static int GetSize(int data)
		{
			return data & 3;
		}

		public static int SetSize(int data, int size)
		{
			size = MathUtils.Clamp(size, 0, 2);
			return (data & -4) | (size & 3);
		}

		public static bool GetIsWild(int data)
		{
			return (data & 8) != 0;
		}

		public static int SetIsWild(int data, bool isWild)
		{
			if (!isWild)
			{
				return data & -9;
			}
			return data | 8;
		}
	}
}
