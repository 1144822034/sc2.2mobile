namespace Game
{
	public class DelayGateBlock : RotateableMountedElectricElementBlock
	{
		public const int Index = 145;

		public DelayGateBlock()
			: base("Models/Gates", "DelayGate", 0.375f)
		{
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new DelayGateElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			if (GetFace(value) == face)
			{
				ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(GetFace(value), RotateableMountedElectricElementBlock.GetRotation(data), connectorFace);
				if (connectorDirection == ElectricConnectorDirection.Bottom)
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
