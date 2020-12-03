using Engine;

namespace Game
{
	public class NandGateElectricElement : RotateableElectricElement
	{
		public float m_voltage;

		public NandGateElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
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
			int num2 = 15;
			foreach (ElectricConnection connection in base.Connections)
			{
				if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != 0)
				{
					num2 &= (int)MathUtils.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					num++;
				}
			}
			m_voltage = ((num == 2) ? ((float)(~num2 & 0xF) / 15f) : 0f);
			return m_voltage != voltage;
		}
	}
}
