using Engine;
using System.Collections.Generic;

namespace Game
{
	public class WireDomainElectricElement : ElectricElement
	{
		public float m_voltage;

		public WireDomainElectricElement(SubsystemElectricity subsystemElectricity, IEnumerable<CellFace> cellFaces)
			: base(subsystemElectricity, cellFaces)
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
					num |= (int)MathUtils.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
				}
			}
			m_voltage = (float)num / 15f;
			return m_voltage != voltage;
		}

		public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ)
		{
			int cellValue = base.SubsystemElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			int num = Terrain.ExtractContents(cellValue);
			if (!(BlocksManager.Blocks[num] is WireBlock))
			{
				return;
			}
			int wireFacesBitmask = WireBlock.GetWireFacesBitmask(cellValue);
			int num2 = wireFacesBitmask;
			if (WireBlock.WireExistsOnFace(cellValue, cellFace.Face))
			{
				Point3 point = CellFace.FaceToPoint3(cellFace.Face);
				int cellValue2 = base.SubsystemElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X - point.X, cellFace.Y - point.Y, cellFace.Z - point.Z);
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
				if (!block.IsCollidable || block.IsTransparent)
				{
					num2 &= ~(1 << cellFace.Face);
				}
			}
			if (num2 == 0)
			{
				base.SubsystemElectricity.SubsystemTerrain.DestroyCell(0, cellFace.X, cellFace.Y, cellFace.Z, 0, noDrop: false, noParticleSystem: false);
			}
			else if (num2 != wireFacesBitmask)
			{
				int newValue = WireBlock.SetWireFacesBitmask(cellValue, num2);
				base.SubsystemElectricity.SubsystemTerrain.DestroyCell(0, cellFace.X, cellFace.Y, cellFace.Z, newValue, noDrop: false, noParticleSystem: false);
			}
		}
	}
}
