using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class BulletBlock : FlatBlock
	{
		public enum BulletType
		{
			MusketBall,
			Buckshot,
			BuckshotBall
		}

		public const int Index = 214;

		public static string[] m_displayNames = new string[3]
		{
			"ǹ��",
			"Ǧ��",
			"Ǧ����"
		};

		public static float[] m_sizes = new float[3]
		{
			1f,
			1f,
			0.33f
		};

		public static int[] m_textureSlots = new int[3]
		{
			229,
			231,
			229
		};

		public static float[] m_weaponPowers = new float[3]
		{
			80f,
			0f,
			3.6f
		};

		public static float[] m_explosionPressures = new float[3];

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			int bulletType = (int)GetBulletType(Terrain.ExtractData(value));
			float size2 = (bulletType >= 0 && bulletType < m_sizes.Length) ? (size * m_sizes[bulletType]) : size;
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size2, ref matrix, null, color, isEmissive: false, environmentData);
		}

		public override float GetProjectilePower(int value)
		{
			int bulletType = (int)GetBulletType(Terrain.ExtractData(value));
			if (bulletType < 0 || bulletType >= m_weaponPowers.Length)
			{
				return 0f;
			}
			return m_weaponPowers[bulletType];
		}

		public override float GetExplosionPressure(int value)
		{
			int bulletType = (int)GetBulletType(Terrain.ExtractData(value));
			if (bulletType < 0 || bulletType >= m_explosionPressures.Length)
			{
				return 0f;
			}
			return m_explosionPressures[bulletType];
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			foreach (int enumValue in EnumUtils.GetEnumValues(typeof(BulletType)))
			{
				yield return Terrain.MakeBlockValue(214, 0, SetBulletType(0, (BulletType)enumValue));
			}
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int bulletType = (int)GetBulletType(Terrain.ExtractData(value));
			if (bulletType < 0 || bulletType >= m_displayNames.Length)
			{
				return string.Empty;
			}
			return m_displayNames[bulletType];
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			int bulletType = (int)GetBulletType(Terrain.ExtractData(value));
			if (bulletType < 0 || bulletType >= m_textureSlots.Length)
			{
				return 229;
			}
			return m_textureSlots[bulletType];
		}

		public static BulletType GetBulletType(int data)
		{
			return (BulletType)(data & 0xF);
		}

		public static int SetBulletType(int data, BulletType bulletType)
		{
			return (data & -16) | (int)(bulletType & (BulletType)15);
		}
	}
}
