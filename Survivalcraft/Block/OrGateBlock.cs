namespace Game
{
	public class OrGateBlock : RotateableMountedElectricElementBlock
	{
		public const int Index = 143;

		public OrGateBlock()
			: base("Models/Gates", "OrGate", 0.375f)
		{
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new OrGateElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			if (GetFace(value) == face)
			{
				ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(GetFace(value), RotateableMountedElectricElementBlock.GetRotation(data), connectorFace);
				if (connectorDirection == ElectricConnectorDirection.Right || connectorDirection == ElectricConnectorDirection.Left)
				{
					return ElectricConnectorType.Input;
				}
				if (connectorDirection == ElectricConnectorDirection.Top || connectorDirection == ElectricConnectorDirection.In)
				{
					return ElectricConnectorType.Output;
				}
			}
			return null;
		}
	}
}
