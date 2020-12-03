using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public abstract class BasePumpkinBlock : Block
	{
		public BlockMesh[] m_blockMeshesBySize = new BlockMesh[8];

		public BlockMesh[] m_standaloneBlockMeshesBySize = new BlockMesh[8];

		public BoundingBox[][] m_collisionBoxesBySize = new BoundingBox[8][];

		public bool m_isRotten;

		public BasePumpkinBlock(bool isRotten)
		{
			m_isRotten = isRotten;
		}

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Pumpkins");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Pumpkin").ParentBone);
			for (int i = 0; i < 8; i++)
			{
				float num = MathUtils.Lerp(0.2f, 1f, (float)i / 7f);
				float num2 = MathUtils.Min(0.3f * num, 0.7f * (1f - num));
				Color color;
				if (m_isRotten)
				{
					color = Color.White;
				}
				else
				{
					color = Color.Lerp(new Color(0, 128, 128), new Color(80, 255, 255), (float)i / 7f);
					if (i == 7)
					{
						color.R = byte.MaxValue;
					}
				}
				m_blockMeshesBySize[i] = new BlockMesh();
				if (i >= 1)
				{
					m_blockMeshesBySize[i].AppendModelMeshPart(model.FindMesh("Pumpkin").MeshParts[0], boneAbsoluteTransform * Matrix.CreateScale(num) * Matrix.CreateTranslation(0.5f + num2, 0f, 0.5f + num2), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
				}
				if (m_isRotten)
				{
					m_blockMeshesBySize[i].TransformTextureCoordinates(Matrix.CreateTranslation(-0.375f, 0.25f, 0f));
				}
				m_standaloneBlockMeshesBySize[i] = new BlockMesh();
				m_standaloneBlockMeshesBySize[i].AppendModelMeshPart(model.FindMesh("Pumpkin").MeshParts[0], boneAbsoluteTransform * Matrix.CreateScale(num) * Matrix.CreateTranslation(0f, -0.23f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
				if (m_isRotten)
				{
					m_standaloneBlockMeshesBySize[i].TransformTextureCoordinates(Matrix.CreateTranslation(-0.375f, 0.25f, 0f));
				}
			}
			for (int j = 0; j < 8; j++)
			{
				BoundingBox boundingBox = (m_blockMeshesBySize[j].Vertices.Count > 0) ? m_blockMeshesBySize[j].CalculateBoundingBox() : new BoundingBox(new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f));
				float num3 = boundingBox.Max.X - boundingBox.Min.X;
				if (num3 < 0.8f)
				{
					float num4 = (0.8f - num3) / 2f;
					boundingBox.Min.X -= num4;
					boundingBox.Min.Z -= num4;
					boundingBox.Max.X += num4;
					boundingBox.Max.Y = 0.4f;
					boundingBox.Max.Z += num4;
				}
				m_collisionBoxesBySize[j] = new BoundingBox[1]
				{
					boundingBox
				};
			}
			base.Initialize();
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int size = GetSize(Terrain.ExtractData(value));
			return m_collisionBoxesBySize[size];
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			int size = GetSize(data);
			bool isDead = GetIsDead(data);
			if (size >= 1)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesBySize[size], Color.White, null, geometry.SubsetOpaque);
			}
			if (size == 0)
			{
				generator.GenerateCrossfaceVertices(this, value, x, y, z, new Color(160, 160, 160), 11, geometry.SubsetAlphaTest);
			}
			else if (size < 7 && !isDead)
			{
				generator.GenerateCrossfaceVertices(this, value, x, y, z, Color.White, 28, geometry.SubsetAlphaTest);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			int size2 = GetSize(Terrain.ExtractData(value));
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMeshesBySize[size2], color, 2f * size, ref matrix, environmentData);
		}

		public override int GetShadowStrength(int value)
		{
			return GetSize(Terrain.ExtractData(value));
		}

		public override float GetNutritionalValue(int value)
		{
			if (GetSize(Terrain.ExtractData(value)) != 7)
			{
				return 0f;
			}
			return base.GetNutritionalValue(value);
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int size = GetSize(Terrain.ExtractData(oldValue));
			if (size >= 1)
			{
				int value = SetDamage(Terrain.MakeBlockValue(BlockIndex, 0, SetSize(SetIsDead(0, isDead: true), size)), GetDamage(oldValue));
				dropValues.Add(new BlockDropValue
				{
					Value = value,
					Count = 1
				});
			}
			showDebris = true;
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			int size = GetSize(Terrain.ExtractData(value));
			float num = MathUtils.Lerp(0.2f, 1f, (float)size / 7f);
			Color color = (size == 7) ? Color.White : new Color(0, 128, 128);
			return new BlockDebrisParticleSystem(subsystemTerrain, position, 1.5f * strength, DestructionDebrisScale * num, color, DefaultTextureSlot);
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int size = GetSize(Terrain.ExtractData(value));
			if (m_isRotten)
			{
				if (size >= 7)
				{
					return "���õ��Ϲ�";
				}
				return "����δ������Ϲ�";
			}
			if (size >= 7)
			{
				return "�Ϲ�";
			}
			return "δ������Ϲ�";
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			yield return Terrain.MakeBlockValue(BlockIndex, 0, SetSize(SetIsDead(0, isDead: true), 1));
			yield return Terrain.MakeBlockValue(BlockIndex, 0, SetSize(SetIsDead(0, isDead: true), 3));
			yield return Terrain.MakeBlockValue(BlockIndex, 0, SetSize(SetIsDead(0, isDead: true), 5));
			yield return Terrain.MakeBlockValue(BlockIndex, 0, SetSize(SetIsDead(0, isDead: true), 7));
		}

		public static int GetSize(int data)
		{
			return 7 - (data & 7);
		}

		public static int SetSize(int data, int size)
		{
			return (data & -8) | (7 - (size & 7));
		}

		public static bool GetIsDead(int data)
		{
			return (data & 8) != 0;
		}

		public static int SetIsDead(int data, bool isDead)
		{
			if (!isDead)
			{
				return data & -9;
			}
			return data | 8;
		}

		public override int GetDamage(int value)
		{
			return (Terrain.ExtractData(value) & 0x10) >> 4;
		}

		public override int SetDamage(int value, int damage)
		{
			int num = Terrain.ExtractData(value);
			return Terrain.ReplaceData(value, (num & -17) | ((damage & 1) << 4));
		}

		public override int GetDamageDestructionValue(int value)
		{
			if (m_isRotten)
			{
				return 0;
			}
			int data = Terrain.ExtractData(value);
			return SetDamage(Terrain.MakeBlockValue(244, 0, data), 0);
		}

		public override int GetRotPeriod(int value)
		{
			if (!GetIsDead(Terrain.ExtractData(value)))
			{
				return 0;
			}
			return DefaultRotPeriod;
		}
	}
}
