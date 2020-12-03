using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemShadows : Subsystem, IDrawable
	{
		public SubsystemTerrain m_subsystemTerrain;

		public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

		public TexturedBatch3D m_batch;

		public static int[] m_drawOrders = new int[1]
		{
			200
		};

		public int[] DrawOrders => m_drawOrders;

		public void QueueShadow(Camera camera, Vector3 shadowPosition, float shadowDiameter, float alpha)
		{
			if (!SettingsManager.ObjectsShadowsEnabled)
			{
				return;
			}
			float num = Vector3.DistanceSquared(camera.ViewPosition, shadowPosition);
			if (!(num <= 1024f))
			{
				return;
			}
			float num2 = MathUtils.Sqrt(num);
			float num3 = MathUtils.Saturate(4f * (1f - num2 / 32f));
			float num4 = shadowDiameter / 2f;
			int num5 = Terrain.ToCell(shadowPosition.X - num4);
			int num6 = Terrain.ToCell(shadowPosition.Z - num4);
			int num7 = Terrain.ToCell(shadowPosition.X + num4);
			int num8 = Terrain.ToCell(shadowPosition.Z + num4);
			for (int i = num5; i <= num7; i++)
			{
				for (int j = num6; j <= num8; j++)
				{
					int num9 = MathUtils.Min(Terrain.ToCell(shadowPosition.Y), 255);
					int num10 = MathUtils.Max(num9 - 2, 0);
					for (int num11 = num9; num11 >= num10; num11--)
					{
						int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(i, num11, j);
						int num12 = Terrain.ExtractContents(cellValueFast);
						Block block = BlocksManager.Blocks[num12];
						if (block.ObjectShadowStrength > 0f)
						{
							BoundingBox[] customCollisionBoxes = block.GetCustomCollisionBoxes(m_subsystemTerrain, cellValueFast);
							for (int k = 0; k < customCollisionBoxes.Length; k++)
							{
								BoundingBox boundingBox = customCollisionBoxes[k];
								float num13 = boundingBox.Max.Y + (float)num11;
								if (shadowPosition.Y - num13 > -0.5f)
								{
									float num14 = camera.ViewPosition.Y - num13;
									if (num14 > 0f)
									{
										float num15 = MathUtils.Max(num14 * 0.01f, 0.005f);
										float num16 = MathUtils.Saturate(1f - (shadowPosition.Y - num13) / 2f);
										Vector3 p = new Vector3(boundingBox.Min.X + (float)i, num13 + num15, boundingBox.Min.Z + (float)j);
										Vector3 p2 = new Vector3(boundingBox.Max.X + (float)i, num13 + num15, boundingBox.Min.Z + (float)j);
										Vector3 p3 = new Vector3(boundingBox.Max.X + (float)i, num13 + num15, boundingBox.Max.Z + (float)j);
										Vector3 p4 = new Vector3(boundingBox.Min.X + (float)i, num13 + num15, boundingBox.Max.Z + (float)j);
										DrawShadowOverQuad(p, p2, p3, p4, shadowPosition, shadowDiameter, 0.45f * block.ObjectShadowStrength * alpha * num3 * num16);
									}
								}
							}
							break;
						}
						if (num12 == 18)
						{
							break;
						}
					}
				}
			}
		}

		public void Draw(Camera camera, int drawOrder)
		{
			m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_batch = m_primitivesRenderer.TexturedBatch(ContentManager.Get<Texture2D>("Textures/Shadow"), useAlphaTest: false, 0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.AlphaBlend, SamplerState.LinearClamp);
		}

		public void DrawShadowOverQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 shadowPosition, float shadowDiameter, float alpha)
		{
			if (alpha > 0.02f)
			{
				Vector2 texCoord = CalculateShadowTextureCoordinate(p1, shadowPosition, shadowDiameter);
				Vector2 texCoord2 = CalculateShadowTextureCoordinate(p2, shadowPosition, shadowDiameter);
				Vector2 texCoord3 = CalculateShadowTextureCoordinate(p3, shadowPosition, shadowDiameter);
				Vector2 texCoord4 = CalculateShadowTextureCoordinate(p4, shadowPosition, shadowDiameter);
				m_batch.QueueQuad(p1, p2, p3, p4, texCoord, texCoord2, texCoord3, texCoord4, new Color(0f, 0f, 0f, alpha));
			}
		}

		public static Vector2 CalculateShadowTextureCoordinate(Vector3 p, Vector3 shadowPosition, float shadowDiameter)
		{
			return new Vector2(0.5f + (p.X - shadowPosition.X) / shadowDiameter, 0.5f + (p.Z - shadowPosition.Z) / shadowDiameter);
		}
	}
}
