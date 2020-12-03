using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemFenceGateBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemElectricity m_subsystemElectricity;

		public static Random m_random = new Random();

		public override int[] HandledBlocks => new int[0];

		public bool OpenCloseGate(int x, int y, int z, bool open)
		{
			int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			if (BlocksManager.Blocks[num] is FenceGateBlock)
			{
				int data = FenceGateBlock.SetOpen(Terrain.ExtractData(cellValue), open);
				int value = Terrain.ReplaceData(cellValue, data);
				base.SubsystemTerrain.ChangeCell(x, y, z, value);
				string name = open ? "Audio/Doors/DoorOpen" : "Audio/Doors/DoorClose";
				base.SubsystemTerrain.Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlaySound(name, 0.7f, m_random.Float(-0.1f, 0.1f), new Vector3(x, y, z), 4f, autoDelay: true);
				return true;
			}
			return false;
		}

		public bool IsGateElectricallyConnected(int x, int y, int z)
		{
			int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			if (BlocksManager.Blocks[num] is FenceGateBlock)
			{
				ElectricElement electricElement = m_subsystemElectricity.GetElectricElement(x, y, z, FenceGateBlock.GetHingeFace(data));
				if (electricElement != null && electricElement.Connections.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			CellFace cellFace = raycastResult.CellFace;
			int cellValue = base.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			if (num == 166 || !IsGateElectricallyConnected(cellFace.X, cellFace.Y, cellFace.Z))
			{
				bool open = FenceGateBlock.GetOpen(data);
				return OpenCloseGate(cellFace.X, cellFace.Y, cellFace.Z, !open);
			}
			return true;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemElectricity = base.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
		}
	}
}
