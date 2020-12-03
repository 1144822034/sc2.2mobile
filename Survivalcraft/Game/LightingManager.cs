using Engine;

namespace Game
{
	public static class LightingManager
	{
		public static readonly float LightAmbient = 0.5f;

		public static readonly Vector3 DirectionToLight1 = new Vector3(0.12f, 0.25f, 0.34f);

		public static readonly Vector3 DirectionToLight2 = new Vector3(-0.12f, 0.25f, -0.34f);

		public static readonly float[] LightIntensityByLightValue = new float[16];

		public static readonly float[] LightIntensityByLightValueAndFace = new float[96];

		public static void Initialize()
		{
			SettingsManager.SettingChanged += delegate(string name)
			{
				if (name == "Brightness")
				{
					CalculateLightingTables();
				}
			};
			CalculateLightingTables();
		}

		public static float CalculateLighting(Vector3 normal)
		{
			return LightAmbient + MathUtils.Max(Vector3.Dot(normal, DirectionToLight1), 0f) + MathUtils.Max(Vector3.Dot(normal, DirectionToLight2), 0f);
		}

		public static float? CalculateSmoothLight(SubsystemTerrain subsystemTerrain, Vector3 p)
		{
			p -= new Vector3(0.5f);
			int num = (int)MathUtils.Floor(p.X);
			int num2 = (int)MathUtils.Floor(p.Y);
			int num3 = (int)MathUtils.Floor(p.Z);
			int x = (int)MathUtils.Ceiling(p.X);
			int num4 = (int)MathUtils.Ceiling(p.Y);
			int z = (int)MathUtils.Ceiling(p.Z);
			Terrain terrain = subsystemTerrain.Terrain;
			if (num2 >= 0 && num4 <= 255)
			{
				TerrainChunk chunkAtCell = terrain.GetChunkAtCell(num, num3);
				TerrainChunk chunkAtCell2 = terrain.GetChunkAtCell(x, num3);
				TerrainChunk chunkAtCell3 = terrain.GetChunkAtCell(num, z);
				TerrainChunk chunkAtCell4 = terrain.GetChunkAtCell(x, z);
				if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1 && chunkAtCell2 != null && chunkAtCell2.State >= TerrainChunkState.InvalidVertices1 && chunkAtCell3 != null && chunkAtCell3.State >= TerrainChunkState.InvalidVertices1 && chunkAtCell4 != null && chunkAtCell4.State >= TerrainChunkState.InvalidVertices1)
				{
					float f = p.X - (float)num;
					float f2 = p.Y - (float)num2;
					float f3 = p.Z - (float)num3;
					float x2 = terrain.GetCellLightFast(num, num2, num3);
					float x3 = terrain.GetCellLightFast(num, num2, z);
					float x4 = terrain.GetCellLightFast(num, num4, num3);
					float x5 = terrain.GetCellLightFast(num, num4, z);
					float x6 = terrain.GetCellLightFast(x, num2, num3);
					float x7 = terrain.GetCellLightFast(x, num2, z);
					float x8 = terrain.GetCellLightFast(x, num4, num3);
					float x9 = terrain.GetCellLightFast(x, num4, z);
					float x10 = MathUtils.Lerp(x2, x6, f);
					float x11 = MathUtils.Lerp(x3, x7, f);
					float x12 = MathUtils.Lerp(x4, x8, f);
					float x13 = MathUtils.Lerp(x5, x9, f);
					float x14 = MathUtils.Lerp(x10, x12, f2);
					float x15 = MathUtils.Lerp(x11, x13, f2);
					float num5 = MathUtils.Lerp(x14, x15, f3);
					int num6 = (int)MathUtils.Floor(num5);
					int num7 = (int)MathUtils.Ceiling(num5);
					float f4 = num5 - (float)num6;
					return MathUtils.Lerp(LightIntensityByLightValue[num6], LightIntensityByLightValue[num7], f4);
				}
			}
			return null;
		}

		public static void CalculateLightingTables()
		{
			float x = MathUtils.Lerp(0f, 0.1f, SettingsManager.Brightness);
			for (int i = 0; i < 16; i++)
			{
				LightIntensityByLightValue[i] = MathUtils.Saturate(MathUtils.Lerp(x, 1f, MathUtils.Pow((float)i / 15f, 1.25f)));
			}
			for (int j = 0; j < 6; j++)
			{
				float num = CalculateLighting(CellFace.FaceToVector3(j));
				for (int k = 0; k < 16; k++)
				{
					LightIntensityByLightValueAndFace[k + j * 16] = LightIntensityByLightValue[k] * num;
				}
			}
		}
	}
}
