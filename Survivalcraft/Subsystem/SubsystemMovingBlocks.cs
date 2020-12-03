using Engine;
using Engine.Graphics;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemMovingBlocks : Subsystem, IUpdateable, IDrawable
	{
		public class MovingBlockSet : IMovingBlockSet
		{
			public string Id;

			public object Tag;

			public bool Stop;

			public int RemainCounter;

			public Vector3 Position;

			public Vector3 StartPosition;

			public Vector3 TargetPosition;

			public float Speed;

			public float Acceleration;

			public float Drag;

			public Vector2 Smoothness;

			public List<MovingBlock> Blocks;

			public Box Box;

			public Vector3 CurrentVelocity;

			public TerrainGeometry Geometry;

			public DynamicArray<TerrainVertex> Vertices = new DynamicArray<TerrainVertex>();

			public DynamicArray<ushort> Indices = new DynamicArray<ushort>();

			public Vector3 GeometryOffset;

			public Point3 GeometryGenerationPosition = new Point3(int.MaxValue);

			Vector3 IMovingBlockSet.Position => Position;

			string IMovingBlockSet.Id => Id;

			object IMovingBlockSet.Tag => Tag;

			Vector3 IMovingBlockSet.CurrentVelocity => CurrentVelocity;

			ReadOnlyList<MovingBlock> IMovingBlockSet.Blocks => new ReadOnlyList<MovingBlock>(Blocks);

			public MovingBlockSet()
			{
				TerrainGeometrySubset terrainGeometrySubset = new TerrainGeometrySubset(Vertices, Indices);
				Geometry = new TerrainGeometry();
				Geometry.SubsetOpaque = terrainGeometrySubset;
				Geometry.SubsetAlphaTest = terrainGeometrySubset;
				Geometry.SubsetTransparent = terrainGeometrySubset;
				Geometry.OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
				Geometry.AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
				Geometry.TransparentSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
			}

			public void UpdateBox()
			{
				Point3? point = null;
				Point3? point2 = null;
				foreach (MovingBlock block in Blocks)
				{
					point = (point.HasValue ? Point3.Min(point.Value, block.Offset) : block.Offset);
					point2 = (point2.HasValue ? Point3.Max(point2.Value, block.Offset) : block.Offset);
				}
				if (point.HasValue)
				{
					Box = new Box(point.Value.X, point.Value.Y, point.Value.Z, point2.Value.X - point.Value.X + 1, point2.Value.Y - point.Value.Y + 1, point2.Value.Z - point.Value.Z + 1);
				}
				else
				{
					Box = default(Box);
				}
			}

			public BoundingBox BoundingBox(bool extendToFillCells)
			{
				Vector3 min = new Vector3(Position.X + (float)Box.Left, Position.Y + (float)Box.Top, Position.Z + (float)Box.Near);
				Vector3 max = new Vector3(Position.X + (float)Box.Right, Position.Y + (float)Box.Bottom, Position.Z + (float)Box.Far);
				if (extendToFillCells)
				{
					min.X = MathUtils.Floor(min.X);
					min.Y = MathUtils.Floor(min.Y);
					min.Z = MathUtils.Floor(min.Z);
					max.X = MathUtils.Ceiling(max.X);
					max.Y = MathUtils.Ceiling(max.Y);
					max.Z = MathUtils.Ceiling(max.Z);
				}
				return new BoundingBox(min, max);
			}

			void IMovingBlockSet.SetBlock(Point3 offset, int value)
			{
				Blocks.RemoveAll((MovingBlock b) => b.Offset == offset);
				if (value != 0)
				{
					Blocks.Add(new MovingBlock
					{
						Offset = offset,
						Value = value
					});
				}
				UpdateBox();
				GeometryGenerationPosition = new Point3(int.MaxValue);
			}

			void IMovingBlockSet.Stop()
			{
				Stop = true;
			}
		}

		public SubsystemTime m_subsystemTime;

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

		public List<MovingBlockSet> m_movingBlockSets = new List<MovingBlockSet>();

		public List<MovingBlockSet> m_stopped = new List<MovingBlockSet>();

		public List<MovingBlockSet> m_removing = new List<MovingBlockSet>();

		public DynamicArray<TerrainVertex> m_vertices = new DynamicArray<TerrainVertex>();

		public DynamicArray<ushort> m_indices = new DynamicArray<ushort>();

		public DynamicArray<IMovingBlockSet> m_result = new DynamicArray<IMovingBlockSet>();

		public Shader m_shader;

		public BlockGeometryGenerator m_blockGeometryGenerator;

		public bool m_canGenerateGeometry;

		public static bool DebugDrawMovingBlocks = false;

		public static int[] m_drawOrders = new int[1]
		{
			10
		};

		public IReadOnlyList<IMovingBlockSet> MovingBlockSets => m_movingBlockSets;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public int[] DrawOrders => m_drawOrders;

		public event Action<IMovingBlockSet, Point3> CollidedWithTerrain;

		public event Action<IMovingBlockSet> Stopped;

		public IMovingBlockSet AddMovingBlockSet(Vector3 position, Vector3 targetPosition, float speed, float acceleration, float drag, Vector2 smoothness, IEnumerable<MovingBlock> blocks, string id, object tag, bool testCollision)
		{
			MovingBlockSet movingBlockSet = new MovingBlockSet
			{
				Position = position,
				StartPosition = position,
				TargetPosition = targetPosition,
				Speed = speed,
				Acceleration = acceleration,
				Drag = drag,
				Smoothness = smoothness,
				Id = id,
				Tag = tag,
				Blocks = blocks.ToList()
			};
			movingBlockSet.UpdateBox();
			if (testCollision)
			{
				MovingBlocksCollision(movingBlockSet);
				if (movingBlockSet.Stop)
				{
					return null;
				}
			}
			if (m_canGenerateGeometry)
			{
				GenerateGeometry(movingBlockSet);
			}
			m_movingBlockSets.Add(movingBlockSet);
			return movingBlockSet;
		}

		public void RemoveMovingBlockSet(IMovingBlockSet movingBlockSet)
		{
			MovingBlockSet movingBlockSet2 = (MovingBlockSet)movingBlockSet;
			if (m_movingBlockSets.Remove(movingBlockSet2))
			{
				m_removing.Add(movingBlockSet2);
				movingBlockSet2.RemainCounter = 4;
			}
		}

		public void FindMovingBlocks(BoundingBox boundingBox, bool extendToFillCells, DynamicArray<IMovingBlockSet> result)
		{
			foreach (MovingBlockSet movingBlockSet in m_movingBlockSets)
			{
				if (ExclusiveBoxIntersection(boundingBox, movingBlockSet.BoundingBox(extendToFillCells)))
				{
					result.Add(movingBlockSet);
				}
			}
		}

		public IMovingBlockSet FindMovingBlocks(string id, object tag)
		{
			foreach (MovingBlockSet movingBlockSet in m_movingBlockSets)
			{
				if (movingBlockSet.Id == id && object.Equals(movingBlockSet.Tag, tag))
				{
					return movingBlockSet;
				}
			}
			return null;
		}

		public MovingBlocksRaycastResult? Raycast(Vector3 start, Vector3 end, bool extendToFillCells)
		{
			Ray3 ray = new Ray3(start, Vector3.Normalize(end - start));
			BoundingBox boundingBox = new BoundingBox(Vector3.Min(start, end), Vector3.Max(start, end));
			m_result.Clear();
			FindMovingBlocks(boundingBox, extendToFillCells, m_result);
			float num = float.MaxValue;
			MovingBlockSet movingBlockSet = null;
			foreach (MovingBlockSet item in m_result)
			{
				BoundingBox box = item.BoundingBox(extendToFillCells);
				float? num2 = ray.Intersection(box);
				if (num2.HasValue && num2.Value < num)
				{
					num = num2.Value;
					movingBlockSet = item;
				}
			}
			if (movingBlockSet != null)
			{
				MovingBlocksRaycastResult value = default(MovingBlocksRaycastResult);
				value.Ray = ray;
				value.Distance = num;
				value.MovingBlockSet = movingBlockSet;
				return value;
			}
			return null;
		}

		public void Update(float dt)
		{
			m_canGenerateGeometry = true;
			foreach (MovingBlockSet movingBlockSet in m_movingBlockSets)
			{
				TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(movingBlockSet.Position.X), Terrain.ToCell(movingBlockSet.Position.Z));
				if (chunkAtCell != null && chunkAtCell.State > TerrainChunkState.InvalidContents4)
				{
					movingBlockSet.Speed += movingBlockSet.Acceleration * m_subsystemTime.GameTimeDelta;
					if (movingBlockSet.Drag != 0f)
					{
						movingBlockSet.Speed *= MathUtils.Pow(1f - movingBlockSet.Drag, m_subsystemTime.GameTimeDelta);
					}
					float x = Vector3.Distance(movingBlockSet.StartPosition, movingBlockSet.Position);
					float num = Vector3.Distance(movingBlockSet.TargetPosition, movingBlockSet.Position);
					float num2 = (movingBlockSet.Smoothness.X > 0f) ? MathUtils.Saturate((MathUtils.Sqrt(x) + 0.05f) / movingBlockSet.Smoothness.X) : 1f;
					float num3 = (movingBlockSet.Smoothness.Y > 0f) ? MathUtils.Saturate((num + 0.05f) / movingBlockSet.Smoothness.Y) : 1f;
					float num4 = num2 * num3;
					bool flag = false;
					Vector3 vector = (num > 0f) ? ((movingBlockSet.TargetPosition - movingBlockSet.Position) / num) : Vector3.Zero;
					float x2 = (m_subsystemTime.GameTimeDelta > 0f) ? (0.95f / m_subsystemTime.GameTimeDelta) : 0f;
					float num5 = MathUtils.Min(movingBlockSet.Speed * num4, x2);
					if (num5 * m_subsystemTime.GameTimeDelta >= num)
					{
						movingBlockSet.Position = movingBlockSet.TargetPosition;
						movingBlockSet.CurrentVelocity = Vector3.Zero;
						flag = true;
					}
					else
					{
						movingBlockSet.CurrentVelocity = num5 / num * (movingBlockSet.TargetPosition - movingBlockSet.Position);
						movingBlockSet.Position += movingBlockSet.CurrentVelocity * m_subsystemTime.GameTimeDelta;
					}
					movingBlockSet.Stop = false;
					MovingBlocksCollision(movingBlockSet);
					TerrainCollision(movingBlockSet);
					if (movingBlockSet.Stop)
					{
						if (vector.X < 0f)
						{
							movingBlockSet.Position.X = MathUtils.Ceiling(movingBlockSet.Position.X);
						}
						else if (vector.X > 0f)
						{
							movingBlockSet.Position.X = MathUtils.Floor(movingBlockSet.Position.X);
						}
						if (vector.Y < 0f)
						{
							movingBlockSet.Position.Y = MathUtils.Ceiling(movingBlockSet.Position.Y);
						}
						else if (vector.Y > 0f)
						{
							movingBlockSet.Position.Y = MathUtils.Floor(movingBlockSet.Position.Y);
						}
						if (vector.Z < 0f)
						{
							movingBlockSet.Position.Z = MathUtils.Ceiling(movingBlockSet.Position.Z);
						}
						else if (vector.Z > 0f)
						{
							movingBlockSet.Position.Z = MathUtils.Floor(movingBlockSet.Position.Z);
						}
					}
					if (movingBlockSet.Stop | flag)
					{
						m_stopped.Add(movingBlockSet);
					}
				}
			}
			foreach (MovingBlockSet item in m_stopped)
			{
				this.Stopped?.Invoke(item);
			}
			m_stopped.Clear();
		}

		public void Draw(Camera camera, int drawOrder)
		{
			m_vertices.Clear();
			m_indices.Clear();
			foreach (MovingBlockSet movingBlockSet2 in m_movingBlockSets)
			{
				DrawMovingBlockSet(camera, movingBlockSet2);
			}
			int num = 0;
			while (num < m_removing.Count)
			{
				MovingBlockSet movingBlockSet = m_removing[num];
				if (movingBlockSet.RemainCounter-- > 0)
				{
					DrawMovingBlockSet(camera, movingBlockSet);
					num++;
				}
				else
				{
					m_removing.RemoveAt(num);
				}
			}
			if (m_vertices.Count > 0)
			{
				Vector3 viewPosition = camera.ViewPosition;
				Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
				Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
				Display.BlendState = BlendState.Opaque;
				Display.DepthStencilState = DepthStencilState.Default;
				Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
				m_shader.GetParameter("u_origin").SetValue(v.XZ);
				m_shader.GetParameter("u_viewProjectionMatrix").SetValue(value);
				m_shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
				m_shader.GetParameter("u_texture").SetValue(m_subsystemAnimatedTextures.AnimatedBlocksTexture);
				m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointClamp);
				m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
				m_shader.GetParameter("u_fogStartInvLength").SetValue(new Vector2(m_subsystemSky.ViewFogRange.X, 1f / (m_subsystemSky.ViewFogRange.Y - m_subsystemSky.ViewFogRange.X)));
				Display.DrawUserIndexed(PrimitiveType.TriangleList, m_shader, TerrainVertex.VertexDeclaration, m_vertices.Array, 0, m_vertices.Count, m_indices.Array, 0, m_indices.Count);
			}
			if (DebugDrawMovingBlocks)
			{
				DebugDraw();
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemAnimatedTextures = base.Project.FindSubsystem<SubsystemAnimatedTextures>(throwOnError: true);
			m_shader = ContentManager.Get<Shader>("Shaders/AlphaTested");
			foreach (ValuesDictionary value9 in valuesDictionary.GetValue<ValuesDictionary>("MovingBlockSets").Values)
			{
				Vector3 value = value9.GetValue<Vector3>("Position");
				Vector3 value2 = value9.GetValue<Vector3>("TargetPosition");
				float value3 = value9.GetValue<float>("Speed");
				float value4 = value9.GetValue<float>("Acceleration");
				float value5 = value9.GetValue<float>("Drag");
				Vector2 value6 = value9.GetValue("Smoothness", Vector2.Zero);
				string value7 = value9.GetValue<string>("Id", null);
				object value8 = value9.GetValue<object>("Tag", null);
				List<MovingBlock> list = new List<MovingBlock>();
				string[] array = value9.GetValue<string>("Blocks").Split(new char[1]
				{
					';'
				}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string obj2 in array)
				{
					MovingBlock item = default(MovingBlock);
					string[] array2 = obj2.Split(new char[1]
					{
						','
					}, StringSplitOptions.RemoveEmptyEntries);
					item.Value = HumanReadableConverter.ConvertFromString<int>(array2[0]);
					item.Offset.X = HumanReadableConverter.ConvertFromString<int>(array2[1]);
					item.Offset.Y = HumanReadableConverter.ConvertFromString<int>(array2[2]);
					item.Offset.Z = HumanReadableConverter.ConvertFromString<int>(array2[3]);
					list.Add(item);
				}
				AddMovingBlockSet(value, value2, value3, value4, value5, value6, list, value7, value8, testCollision: false);
			}
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			ValuesDictionary valuesDictionary2 = new ValuesDictionary();
			valuesDictionary.SetValue("MovingBlockSets", valuesDictionary2);
			int num = 0;
			foreach (MovingBlockSet movingBlockSet in m_movingBlockSets)
			{
				ValuesDictionary valuesDictionary3 = new ValuesDictionary();
				valuesDictionary2.SetValue(num.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
				valuesDictionary3.SetValue("Position", movingBlockSet.Position);
				valuesDictionary3.SetValue("TargetPosition", movingBlockSet.TargetPosition);
				valuesDictionary3.SetValue("Speed", movingBlockSet.Speed);
				valuesDictionary3.SetValue("Acceleration", movingBlockSet.Acceleration);
				valuesDictionary3.SetValue("Drag", movingBlockSet.Drag);
				if (movingBlockSet.Smoothness != Vector2.Zero)
				{
					valuesDictionary3.SetValue("Smoothness", movingBlockSet.Smoothness);
				}
				if (movingBlockSet.Id != null)
				{
					valuesDictionary3.SetValue("Id", movingBlockSet.Id);
				}
				if (movingBlockSet.Tag != null)
				{
					valuesDictionary3.SetValue("Tag", movingBlockSet.Tag);
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (MovingBlock block in movingBlockSet.Blocks)
				{
					stringBuilder.Append(HumanReadableConverter.ConvertToString(block.Value));
					stringBuilder.Append(',');
					stringBuilder.Append(HumanReadableConverter.ConvertToString(block.Offset.X));
					stringBuilder.Append(',');
					stringBuilder.Append(HumanReadableConverter.ConvertToString(block.Offset.Y));
					stringBuilder.Append(',');
					stringBuilder.Append(HumanReadableConverter.ConvertToString(block.Offset.Z));
					stringBuilder.Append(';');
				}
				valuesDictionary3.SetValue("Blocks", stringBuilder.ToString());
				num++;
			}
		}

		public override void Dispose()
		{
			if (m_blockGeometryGenerator != null && m_blockGeometryGenerator.Terrain != null)
			{
				m_blockGeometryGenerator.Terrain.Dispose();
			}
		}

		public void DebugDraw()
		{
		}

		public void MovingBlocksCollision(MovingBlockSet movingBlockSet)
		{
			BoundingBox boundingBox = movingBlockSet.BoundingBox(extendToFillCells: true);
			m_result.Clear();
			FindMovingBlocks(boundingBox, extendToFillCells: true, m_result);
			int num = 0;
			while (true)
			{
				if (num < m_result.Count)
				{
					if (m_result.Array[num] != movingBlockSet)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			movingBlockSet.Stop = true;
		}

		public void TerrainCollision(MovingBlockSet movingBlockSet)
		{
			Point3 point = default(Point3);
			point.X = (int)MathUtils.Floor((float)movingBlockSet.Box.Left + movingBlockSet.Position.X);
			point.Y = (int)MathUtils.Floor((float)movingBlockSet.Box.Top + movingBlockSet.Position.Y);
			point.Z = (int)MathUtils.Floor((float)movingBlockSet.Box.Near + movingBlockSet.Position.Z);
			Point3 point2 = default(Point3);
			point2.X = (int)MathUtils.Ceiling((float)movingBlockSet.Box.Right + movingBlockSet.Position.X);
			point2.Y = (int)MathUtils.Ceiling((float)movingBlockSet.Box.Bottom + movingBlockSet.Position.Y);
			point2.Z = (int)MathUtils.Ceiling((float)movingBlockSet.Box.Far + movingBlockSet.Position.Z);
			for (int i = point.X; i < point2.X; i++)
			{
				for (int j = point.Z; j < point2.Z; j++)
				{
					for (int k = point.Y; k < point2.Y; k++)
					{
						if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(i, k, j)) != 0)
						{
							this.CollidedWithTerrain?.Invoke(movingBlockSet, new Point3(i, k, j));
						}
					}
				}
			}
		}

		public void GenerateGeometry(MovingBlockSet movingBlockSet)
		{
			Point3 point = default(Point3);
			point.X = ((movingBlockSet.CurrentVelocity.X > 0f) ? ((int)MathUtils.Floor(movingBlockSet.Position.X)) : (point.X = (int)MathUtils.Ceiling(movingBlockSet.Position.X)));
			point.Y = ((movingBlockSet.CurrentVelocity.Y > 0f) ? ((int)MathUtils.Floor(movingBlockSet.Position.Y)) : (point.Y = (int)MathUtils.Ceiling(movingBlockSet.Position.Y)));
			point.Z = ((movingBlockSet.CurrentVelocity.Z > 0f) ? ((int)MathUtils.Floor(movingBlockSet.Position.Z)) : (point.Z = (int)MathUtils.Ceiling(movingBlockSet.Position.Z)));
			if (!(point != movingBlockSet.GeometryGenerationPosition))
			{
				return;
			}
			Point3 p = new Point3(movingBlockSet.Box.Left, movingBlockSet.Box.Top, movingBlockSet.Box.Near);
			Point3 point2 = new Point3(movingBlockSet.Box.Width, movingBlockSet.Box.Height, movingBlockSet.Box.Depth);
			point2.Y = MathUtils.Min(point2.Y, 254);
			if (m_blockGeometryGenerator == null)
			{
				int x = 2;
				x = (int)MathUtils.NextPowerOf2((uint)x);
				m_blockGeometryGenerator = new BlockGeometryGenerator(new Terrain(), m_subsystemTerrain, null, base.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(throwOnError: true), null, base.Project.FindSubsystem<SubsystemPalette>(throwOnError: true));
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < x; j++)
					{
						m_blockGeometryGenerator.Terrain.AllocateChunk(i, j);
					}
				}
			}
			Terrain terrain = m_subsystemTerrain.Terrain;
			for (int k = 0; k < point2.X + 2; k++)
			{
				for (int l = 0; l < point2.Z + 2; l++)
				{
					int x2 = k + p.X + point.X - 1;
					int z = l + p.Z + point.Z - 1;
					int shaftValue = terrain.GetShaftValue(x2, z);
					m_blockGeometryGenerator.Terrain.SetTemperature(k, l, Terrain.ExtractTemperature(shaftValue));
					m_blockGeometryGenerator.Terrain.SetHumidity(k, l, Terrain.ExtractHumidity(shaftValue));
					for (int m = 0; m < point2.Y + 2; m++)
					{
						int y = m + p.Y + point.Y - 1;
						int light = Terrain.ExtractLight(terrain.GetCellValue(x2, y, z));
						m_blockGeometryGenerator.Terrain.SetCellValueFast(k, m, l, Terrain.MakeBlockValue(0, light, 0));
					}
				}
			}
			m_blockGeometryGenerator.Terrain.SeasonTemperature = terrain.SeasonTemperature;
			m_blockGeometryGenerator.Terrain.SeasonHumidity = terrain.SeasonHumidity;
			foreach (MovingBlock block in movingBlockSet.Blocks)
			{
				int x3 = block.Offset.X - p.X + 1;
				int y2 = block.Offset.Y - p.Y + 1;
				int z2 = block.Offset.Z - p.Z + 1;
				int value = Terrain.ReplaceLight(light: m_blockGeometryGenerator.Terrain.GetCellLightFast(x3, y2, z2), value: block.Value);
				m_blockGeometryGenerator.Terrain.SetCellValueFast(x3, y2, z2, value);
			}
			m_blockGeometryGenerator.ResetCache();
			movingBlockSet.Vertices.Clear();
			movingBlockSet.Indices.Clear();
			for (int n = 1; n < point2.X + 1; n++)
			{
				for (int num = 1; num < point2.Y + 1; num++)
				{
					for (int num2 = 1; num2 < point2.Z + 1; num2++)
					{
						int cellValueFast = m_blockGeometryGenerator.Terrain.GetCellValueFast(n, num, num2);
						int num3 = Terrain.ExtractContents(cellValueFast);
						if (num3 != 0)
						{
							BlocksManager.Blocks[num3].GenerateTerrainVertices(m_blockGeometryGenerator, movingBlockSet.Geometry, cellValueFast, n, num, num2);
						}
					}
				}
			}
			movingBlockSet.GeometryOffset = new Vector3(p) - new Vector3(1f);
			movingBlockSet.GeometryGenerationPosition = point;
		}

		public void DrawMovingBlockSet(Camera camera, MovingBlockSet movingBlockSet)
		{
			if (m_vertices.Count <= 20000 && camera.ViewFrustum.Intersection(movingBlockSet.BoundingBox(extendToFillCells: false)))
			{
				GenerateGeometry(movingBlockSet);
				int count = m_vertices.Count;
				ushort[] array = movingBlockSet.Indices.Array;
				_ = movingBlockSet.Indices.Count;
				Vector3 vector = movingBlockSet.Position + movingBlockSet.GeometryOffset;
				TerrainVertex[] array2 = movingBlockSet.Vertices.Array;
				int count2 = movingBlockSet.Vertices.Count;
				for (int i = 0; i < count2; i++)
				{
					TerrainVertex item = array2[i];
					item.X += vector.X;
					item.Y += vector.Y;
					item.Z += vector.Z;
					m_vertices.Add(item);
				}
				for (int j = 0; j < movingBlockSet.Indices.Count; j++)
				{
					m_indices.Add((ushort)(array[j] + count));
				}
			}
		}

		public static bool ExclusiveBoxIntersection(BoundingBox b1, BoundingBox b2)
		{
			if (b1.Max.X > b2.Min.X && b1.Min.X < b2.Max.X && b1.Max.Y > b2.Min.Y && b1.Min.Y < b2.Max.Y && b1.Max.Z > b2.Min.Z)
			{
				return b1.Min.Z < b2.Max.Z;
			}
			return false;
		}
	}
}
