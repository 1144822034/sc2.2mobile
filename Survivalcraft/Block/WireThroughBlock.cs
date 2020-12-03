using Engine;

namespace Game
{
	public abstract class WireThroughBlock : CubeBlock, IElectricWireElementBlock, IElectricElementBlock
	{
		public int m_wiredTextureSlot;

		public int m_unwiredTextureSlot;

		public WireThroughBlock(int wiredTextureSlot, int unwiredTextureSlot)
		{
			m_wiredTextureSlot = wiredTextureSlot;
			m_unwiredTextureSlot = unwiredTextureSlot;
		}

		public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return null;
		}

		public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int wiredFace = GetWiredFace(Terrain.ExtractData(value));
			if ((face == wiredFace || face == CellFace.OppositeFace(wiredFace)) && connectorFace == CellFace.OppositeFace(face))
			{
				return ElectricConnectorType.InputOutput;
			}
			return null;
		}

		public int GetConnectionMask(int value)
		{
			return int.MaxValue;
		}

		public int GetConnectedWireFacesMask(int value, int face)
		{
			int wiredFace = GetWiredFace(Terrain.ExtractData(value));
			if (wiredFace == face || CellFace.OppositeFace(wiredFace) == face)
			{
				return (1 << wiredFace) | (1 << CellFace.OppositeFace(wiredFace));
			}
			return 0;
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			int wiredFace = GetWiredFace(Terrain.ExtractData(value));
			if (wiredFace == face || CellFace.OppositeFace(wiredFace) == face)
			{
				return m_wiredTextureSlot;
			}
			return m_unwiredTextureSlot;
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			float num = float.NegativeInfinity;
			int wiredFace = 0;
			for (int i = 0; i < 6; i++)
			{
				float num2 = Vector3.Dot(CellFace.FaceToVector3(i), forward);
				if (num2 > num)
				{
					num = num2;
					wiredFace = i;
				}
			}
			BlockPlacementData result = default(BlockPlacementData);
			result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetWiredFace(0, wiredFace));
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public static int GetWiredFace(int data)
		{
			if ((data & 3) == 0)
			{
				return 0;
			}
			if ((data & 3) == 1)
			{
				return 1;
			}
			return 4;
		}

		public static int SetWiredFace(int data, int wiredFace)
		{
			data &= -4;
			switch (wiredFace)
			{
			case 0:
			case 2:
				return data;
			case 1:
			case 3:
				return data | 1;
			default:
				return data | 2;
			}
		}
	}
}
