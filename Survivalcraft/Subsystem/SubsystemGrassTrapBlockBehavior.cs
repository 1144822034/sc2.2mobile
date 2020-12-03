using Engine;
using System.Collections.Generic;

namespace Game
{
	public class SubsystemGrassTrapBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		public class TrapValue
		{
			public float Damage;
		}

		public Dictionary<Point3, TrapValue> m_trapValues = new Dictionary<Point3, TrapValue>();

		public List<Point3> m_toRemove = new List<Point3>();

		public override int[] HandledBlocks => new int[1]
		{
			87
		};

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
			if (cellFace.Face == 4 && componentBody.Mass > 20f)
			{
				Point3 key = new Point3(cellFace.X, cellFace.Y, cellFace.Z);
				if (!m_trapValues.TryGetValue(key, out TrapValue value))
				{
					value = new TrapValue();
					m_trapValues.Add(key, value);
				}
				value.Damage += 0f - velocity;
			}
		}

		public void Update(float dt)
		{
			foreach (KeyValuePair<Point3, TrapValue> trapValue in m_trapValues)
			{
				if (trapValue.Value.Damage > 1f)
				{
					for (int i = -1; i <= 1; i++)
					{
						for (int j = -1; j <= 1; j++)
						{
							if (MathUtils.Abs(i) + MathUtils.Abs(j) <= 1 && base.SubsystemTerrain.Terrain.GetCellContents(trapValue.Key.X + i, trapValue.Key.Y, trapValue.Key.Z + j) == 87)
							{
								base.SubsystemTerrain.DestroyCell(0, trapValue.Key.X + i, trapValue.Key.Y, trapValue.Key.Z + j, 0, noDrop: false, noParticleSystem: false);
							}
						}
					}
					trapValue.Value.Damage = 0f;
				}
				else
				{
					trapValue.Value.Damage -= 0.5f * dt;
				}
				if (trapValue.Value.Damage <= 0f)
				{
					m_toRemove.Add(trapValue.Key);
				}
			}
			foreach (Point3 item in m_toRemove)
			{
				m_trapValues.Remove(item);
			}
			m_toRemove.Clear();
		}
	}
}
