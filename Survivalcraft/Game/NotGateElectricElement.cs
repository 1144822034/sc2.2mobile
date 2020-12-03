using Engine;

namespace Game
{
	public class NotGateElectricElement : RotateableElectricElement
	{
		public float m_voltage;

		public NotGateElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
		}

		public override float GetOutputVoltage(int face)
		{
			return m_voltage;
		}

		public override bool Simulate()
		{
			float voltage = m_voltage;
			int num = 0;
			foreach (ElectricConnection connection in base.Connections)
			{
				if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != 0)
				{
					num = (int)MathUtils.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					break;
				}
			}
			m_voltage = (float)(~num & 0xF) / 15f;
			return m_voltage != voltage;
		}
	}
}
