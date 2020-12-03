using Engine;

namespace Game
{
	public class SwitchFurnitureElectricElement : FurnitureElectricElement
	{
		public float m_voltage;

		public SwitchFurnitureElectricElement(SubsystemElectricity subsystemElectricity, Point3 point, int value)
			: base(subsystemElectricity, point)
		{
			FurnitureDesign design = FurnitureBlock.GetDesign(subsystemElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior, value);
			if (design != null && design.LinkedDesign != null)
			{
				m_voltage = ((design.Index >= design.LinkedDesign.Index) ? 1 : 0);
			}
		}

		public override float GetOutputVoltage(int face)
		{
			return m_voltage;
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			CellFace cellFace = base.CellFaces[0];
			base.SubsystemElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior.SwitchToNextState(cellFace.X, cellFace.Y, cellFace.Z, playSound: false);
			base.SubsystemElectricity.SubsystemAudio.PlaySound("Audio/Click", 1f, 0f, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f, autoDelay: true);
			return true;
		}
	}
}
