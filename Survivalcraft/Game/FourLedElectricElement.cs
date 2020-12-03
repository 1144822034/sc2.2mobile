using Engine;

namespace Game
{
	public class FourLedElectricElement : MountedElectricElement
	{
		public SubsystemGlow m_subsystemGlow;

		public float m_voltage;

		public Color m_color;

		public GlowPoint[] m_glowPoints = new GlowPoint[4];

		public FourLedElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
			m_subsystemGlow = subsystemElectricity.Project.FindSubsystem<SubsystemGlow>(throwOnError: true);
		}

		public override void OnAdded()
		{
			CellFace cellFace = base.CellFaces[0];
			int data = Terrain.ExtractData(base.SubsystemElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
			int mountingFace = FourLedBlock.GetMountingFace(data);
			m_color = LedBlock.LedColors[FourLedBlock.GetColor(data)];
			for (int i = 0; i < 4; i++)
			{
				int num = (i % 2 == 0) ? 1 : (-1);
				int num2 = (i / 2 == 0) ? 1 : (-1);
				Vector3 v = new Vector3((float)cellFace.X + 0.5f, (float)cellFace.Y + 0.5f, (float)cellFace.Z + 0.5f);
				Vector3 vector = CellFace.FaceToVector3(mountingFace);
				Vector3 vector2 = (mountingFace < 4) ? Vector3.UnitY : Vector3.UnitX;
				Vector3 vector3 = Vector3.Cross(vector, vector2);
				m_glowPoints[i] = m_subsystemGlow.AddGlowPoint();
				m_glowPoints[i].Position = v - 0.4375f * CellFace.FaceToVector3(mountingFace) + 0.25f * vector3 * num + 0.25f * vector2 * num2;
				m_glowPoints[i].Forward = vector;
				m_glowPoints[i].Up = vector2;
				m_glowPoints[i].Right = vector3;
				m_glowPoints[i].Color = Color.Transparent;
				m_glowPoints[i].Size = 0.26f;
				m_glowPoints[i].FarSize = 0.26f;
				m_glowPoints[i].FarDistance = 1f;
				m_glowPoints[i].Type = GlowPointType.Square;
			}
		}

		public override void OnRemoved()
		{
			for (int i = 0; i < 4; i++)
			{
				m_subsystemGlow.RemoveGlowPoint(m_glowPoints[i]);
			}
		}

		public override bool Simulate()
		{
			float voltage = m_voltage;
			m_voltage = 0f;
			foreach (ElectricConnection connection in base.Connections)
			{
				if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != 0)
				{
					m_voltage = MathUtils.Max(m_voltage, connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
				}
			}
			if (m_voltage != voltage)
			{
				int num = (int)MathUtils.Round(m_voltage * 15f);
				for (int i = 0; i < 4; i++)
				{
					if ((num & (1 << i)) != 0)
					{
						m_glowPoints[i].Color = m_color;
					}
					else
					{
						m_glowPoints[i].Color = Color.Transparent;
					}
				}
			}
			return false;
		}
	}
}
