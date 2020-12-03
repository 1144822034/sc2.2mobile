using Engine;
using System;
using System.Collections.Generic;

namespace Game
{
	public class TerrainBrush
	{
		public struct Cell : IComparable<Cell>
		{
			public sbyte X;

			public sbyte Y;

			public sbyte Z;

			public int Value;

			public int CompareTo(Cell other)
			{
				return Key(X, Y, Z) - Key(other.X, other.Y, other.Z);
			}
		}

		public class Brush
		{
			public int m_value;

			public Func<int?, int?> m_handler1;

			public Func<Point3, int?> m_handler2;

			public Brush()
			{
			}

			public static implicit operator Brush(int value)
			{
				return new Brush
				{
					m_value = value
				};
			}

			public static implicit operator Brush(Func<int?, int?> handler)
			{
				return new Brush
				{
					m_handler1 = handler
				};
			}

			public static implicit operator Brush(Func<Point3, int?> handler)
			{
				return new Brush
				{
					m_handler2 = handler
				};
			}

			public int? Paint(TerrainBrush terrainBrush, Point3 p)
			{
				if (m_handler1 != null)
				{
					return m_handler1(terrainBrush.GetValue(p.X, p.Y, p.Z));
				}
				if (m_handler2 != null)
				{
					return m_handler2(p);
				}
				return m_value;
			}
		}

		public class Counter
		{
			public int m_value;

			public Func<int?, int> m_handler1;

			public Func<Point3, int> m_handler2;

			public Counter()
			{
			}

			public static implicit operator Counter(int value)
			{
				return new Counter
				{
					m_value = value
				};
			}

			public static implicit operator Counter(Func<int?, int> handler)
			{
				return new Counter
				{
					m_handler1 = handler
				};
			}

			public static implicit operator Counter(Func<Point3, int> handler)
			{
				return new Counter
				{
					m_handler2 = handler
				};
			}

			public int Count(TerrainBrush terrainBrush, Point3 p)
			{
				if (m_handler1 != null)
				{
					return m_handler1(terrainBrush.GetValue(p));
				}
				if (m_handler2 != null)
				{
					return m_handler2(p);
				}
				if (terrainBrush.GetValue(p) != m_value)
				{
					return 0;
				}
				return 1;
			}
		}

		public Dictionary<int, Cell> m_cellsDictionary = new Dictionary<int, Cell>();

		public Cell[] m_cells;

		public Cell[] Cells => m_cells;

		public static int Key(int x, int y, int z)
		{
			return y + 128 + (x + 128 << 8) + (z + 128 << 16);
		}

		public void Compile()
		{
			m_cells = new Cell[m_cellsDictionary.Values.Count];
			int num = 0;
			foreach (Cell value in m_cellsDictionary.Values)
			{
				m_cells[num++] = value;
			}
			Array.Sort(m_cells);
			m_cellsDictionary = null;
		}

		public int CountNonDiagonalNeighbors(int x, int y, int z, Counter counter)
		{
			return 0 + counter.Count(this, new Point3(x - 1, y, z)) + counter.Count(this, new Point3(x + 1, y, z)) + counter.Count(this, new Point3(x, y - 1, z)) + counter.Count(this, new Point3(x, y + 1, z)) + counter.Count(this, new Point3(x, y, z - 1)) + counter.Count(this, new Point3(x, y, z + 1));
		}

		public int CountBox(int x, int y, int z, int sizeX, int sizeY, int sizeZ, Counter counter)
		{
			int num = 0;
			for (int i = x; i < x + sizeX; i++)
			{
				for (int j = y; j < y + sizeY; j++)
				{
					for (int k = z; k < z + sizeZ; k++)
					{
						num += counter.Count(this, new Point3(i, j, k));
					}
				}
			}
			return num;
		}

		public void Replace(int oldValue, int newValue)
		{
			Dictionary<int, Cell> dictionary = new Dictionary<int, Cell>();
			foreach (KeyValuePair<int, Cell> item in m_cellsDictionary)
			{
				Cell value = item.Value;
				if (value.Value == oldValue)
				{
					value.Value = newValue;
				}
				dictionary[item.Key] = value;
			}
			m_cellsDictionary = dictionary;
			m_cells = null;
		}

		public void CalculateBounds(out Point3 min, out Point3 max)
		{
			min = Point3.Zero;
			max = Point3.Zero;
			bool flag = true;
			foreach (Cell value in m_cellsDictionary.Values)
			{
				if (flag)
				{
					flag = false;
					min.X = (max.X = value.X);
					min.Y = (max.Y = value.Y);
					min.Z = (max.Z = value.Z);
				}
				else
				{
					min.X = MathUtils.Min(min.X, value.X);
					min.Y = MathUtils.Min(min.Y, value.Y);
					min.Z = MathUtils.Min(min.Z, value.Z);
					max.X = MathUtils.Max(max.X, value.X);
					max.Y = MathUtils.Max(max.Y, value.Y);
					max.Z = MathUtils.Max(max.Z, value.Z);
				}
			}
		}

		public int? GetValue(Point3 p)
		{
			return GetValue(p.X, p.Y, p.Z);
		}

		public int? GetValue(int x, int y, int z)
		{
			int key = Key(x, y, z);
			if (m_cellsDictionary.TryGetValue(key, out Cell value))
			{
				return value.Value;
			}
			return null;
		}

		public void AddCell(int x, int y, int z, Brush brush)
		{
			int? num = brush.Paint(this, new Point3(x, y, z));
			if (num.HasValue)
			{
				int key = Key(x, y, z);
				m_cellsDictionary[key] = new Cell
				{
					X = (sbyte)x,
					Y = (sbyte)y,
					Z = (sbyte)z,
					Value = num.Value
				};
				m_cells = null;
			}
		}

		public void AddBox(int x, int y, int z, int sizeX, int sizeY, int sizeZ, Brush brush)
		{
			for (int i = x; i < x + sizeX; i++)
			{
				for (int j = y; j < y + sizeY; j++)
				{
					for (int k = z; k < z + sizeZ; k++)
					{
						AddCell(i, j, k, brush);
					}
				}
			}
		}

		public void AddRay(int x1, int y1, int z1, int x2, int y2, int z2, int sizeX, int sizeY, int sizeZ, Brush brush)
		{
			Vector3 vector = new Vector3(x1, y1, z1) + new Vector3(0.5f);
			Vector3 vector2 = new Vector3(x2, y2, z2) + new Vector3(0.5f);
			Vector3 vector3 = 0.33f * Vector3.Normalize(vector2 - vector);
			int num = (int)MathUtils.Round(3f * Vector3.Distance(vector, vector2));
			Vector3 vector4 = vector;
			for (int i = 0; i < num; i++)
			{
				int x3 = Terrain.ToCell(vector4.X);
				int y3 = Terrain.ToCell(vector4.Y);
				int z3 = Terrain.ToCell(vector4.Z);
				AddBox(x3, y3, z3, sizeX, sizeY, sizeZ, brush);
				vector4 += vector3;
			}
		}

		public void PaintFastSelective(TerrainChunk chunk, int x, int y, int z, int onlyInValue)
		{
			x -= chunk.Origin.X;
			z -= chunk.Origin.Y;
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= 0 && num < 16 && num2 >= 0 && num2 < 256 && num3 >= 0 && num3 < 16)
				{
					int index = TerrainChunk.CalculateCellIndex(num, num2, num3);
					int cellValueFast = chunk.GetCellValueFast(index);
					if (onlyInValue == cellValueFast)
					{
						chunk.SetCellValueFast(index, cell.Value);
					}
				}
			}
		}

		public void PaintFastSelective(Terrain terrain, int x, int y, int z, int minX, int maxX, int minY, int maxY, int minZ, int maxZ, int onlyInValue)
		{
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= minX && num < maxX && num2 >= minY && num2 < maxY && num3 >= minZ && num3 < maxZ)
				{
					int cellValueFast = terrain.GetCellValueFast(num, num2, num3);
					if (onlyInValue == cellValueFast)
					{
						terrain.SetCellValueFast(num, num2, num3, cell.Value);
					}
				}
			}
		}

		public void PaintFastAvoidWater(TerrainChunk chunk, int x, int y, int z)
		{
			Terrain terrain = chunk.Terrain;
			x -= chunk.Origin.X;
			z -= chunk.Origin.Y;
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= 0 && num < 16 && num2 >= 0 && num2 < 255 && num3 >= 0 && num3 < 16)
				{
					int num4 = num + chunk.Origin.X;
					int y2 = num2;
					int num5 = num3 + chunk.Origin.Y;
					if (chunk.GetCellContentsFast(num, num2, num3) != 18 && terrain.GetCellContents(num4 - 1, y2, num5) != 18 && terrain.GetCellContents(num4 + 1, y2, num5) != 18 && terrain.GetCellContents(num4, y2, num5 - 1) != 18 && terrain.GetCellContents(num4, y2, num5 + 1) != 18 && chunk.GetCellContentsFast(num, num2 + 1, num3) != 18)
					{
						chunk.SetCellValueFast(num, num2, num3, cell.Value);
					}
				}
			}
		}

		public void PaintFastAvoidWater(Terrain terrain, int x, int y, int z, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
		{
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= minX && num < maxX && num2 >= minY && num2 < maxY && num3 >= minZ && num3 < maxZ && terrain.GetCellContentsFast(num, num2, num3) != 18 && terrain.GetCellContents(num - 1, num2, num3) != 18 && terrain.GetCellContents(num + 1, num2, num3) != 18 && terrain.GetCellContents(num, num2, num3 - 1) != 18 && terrain.GetCellContents(num, num2, num3 + 1) != 18 && terrain.GetCellContentsFast(num, num2 + 1, num3) != 18)
				{
					terrain.SetCellValueFast(num, num2, num3, cell.Value);
				}
			}
		}

		public void PaintFast(TerrainChunk chunk, int x, int y, int z)
		{
			x -= chunk.Origin.X;
			z -= chunk.Origin.Y;
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= 0 && num < 16 && num2 >= 0 && num2 < 256 && num3 >= 0 && num3 < 16)
				{
					chunk.SetCellValueFast(num, num2, num3, cell.Value);
				}
			}
		}

		public void PaintFast(Terrain terrain, int x, int y, int z, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
		{
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int num = cell.X + x;
				int num2 = cell.Y + y;
				int num3 = cell.Z + z;
				if (num >= minX && num < maxX && num2 >= minY && num2 < maxY && num3 >= minZ && num3 < maxZ)
				{
					terrain.SetCellValueFast(num, num2, num3, cell.Value);
				}
			}
		}

		public void Paint(SubsystemTerrain terrain, int x, int y, int z)
		{
			Cell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				Cell cell = cells[i];
				int x2 = cell.X + x;
				int y2 = cell.Y + y;
				int z2 = cell.Z + z;
				terrain.ChangeCell(x2, y2, z2, cell.Value);
			}
		}
	}
}
