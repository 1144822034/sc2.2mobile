using Engine;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemCrossbowBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemTime m_subsystemTime;

		public SubsystemProjectiles m_subsystemProjectiles;

		public SubsystemAudio m_subsystemAudio;

		public Random m_random = new Random();

		public Dictionary<ComponentMiner, double> m_aimStartTimes = new Dictionary<ComponentMiner, double>();

		public ArrowBlock.ArrowType[] m_supportedArrowTypes = new ArrowBlock.ArrowType[3]
		{
			ArrowBlock.ArrowType.IronBolt,
			ArrowBlock.ArrowType.DiamondBolt,
			ArrowBlock.ArrowType.ExplosiveBolt
		};

		public override int[] HandledBlocks => new int[0];

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			componentPlayer.ComponentGui.ModalPanelWidget = ((componentPlayer.ComponentGui.ModalPanelWidget == null) ? new CrossbowWidget(inventory, slotIndex) : null);
			return true;
		}

		public override bool OnAim(Ray3 aim, ComponentMiner componentMiner, AimState state)
		{
			IInventory inventory = componentMiner.Inventory;
			if (inventory != null)
			{
				int activeSlotIndex = inventory.ActiveSlotIndex;
				if (activeSlotIndex >= 0)
				{
					int slotValue = inventory.GetSlotValue(activeSlotIndex);
					int slotCount = inventory.GetSlotCount(activeSlotIndex);
					int num = Terrain.ExtractContents(slotValue);
					int data = Terrain.ExtractData(slotValue);
					if (num == 200 && slotCount > 0)
					{
						int draw = CrossbowBlock.GetDraw(data);
						if (!m_aimStartTimes.TryGetValue(componentMiner, out double value))
						{
							value = m_subsystemTime.GameTime;
							m_aimStartTimes[componentMiner] = value;
						}
						float num2 = (float)(m_subsystemTime.GameTime - value);
						float num3 = (float)MathUtils.Remainder(m_subsystemTime.GameTime, 1000.0);
						Vector3 v = ((componentMiner.ComponentCreature.ComponentBody.IsSneaking ? 0.01f : 0.03f) + 0.15f * MathUtils.Saturate((num2 - 2.5f) / 6f)) * new Vector3
						{
							X = SimplexNoise.OctavedNoise(num3, 2f, 3, 2f, 0.5f),
							Y = SimplexNoise.OctavedNoise(num3 + 100f, 2f, 3, 2f, 0.5f),
							Z = SimplexNoise.OctavedNoise(num3 + 200f, 2f, 3, 2f, 0.5f)
						};
						aim.Direction = Vector3.Normalize(aim.Direction + v);
						switch (state)
						{
						case AimState.InProgress:
						{
							if (num2 >= 10f)
							{
								componentMiner.ComponentCreature.ComponentCreatureSounds.PlayMoanSound();
								return true;
							}
							ComponentFirstPersonModel componentFirstPersonModel = componentMiner.Entity.FindComponent<ComponentFirstPersonModel>();
							if (componentFirstPersonModel != null)
							{
								componentMiner.ComponentPlayer?.ComponentAimingSights.ShowAimingSights(aim.Position, aim.Direction);
								componentFirstPersonModel.ItemOffsetOrder = new Vector3(-0.22f, 0.15f, 0.1f);
								componentFirstPersonModel.ItemRotationOrder = new Vector3(-0.7f, 0f, 0f);
							}
							componentMiner.ComponentCreature.ComponentCreatureModel.AimHandAngleOrder = 1.3f;
							componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemOffsetOrder = new Vector3(-0.08f, -0.1f, 0.07f);
							componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemRotationOrder = new Vector3(-1.55f, 0f, 0f);
							break;
						}
						case AimState.Cancelled:
							m_aimStartTimes.Remove(componentMiner);
							break;
						case AimState.Completed:
						{
							ArrowBlock.ArrowType? arrowType = CrossbowBlock.GetArrowType(data);
							if (draw != 15)
							{
								componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage("Draw the crossbow first", Color.White, blinking: true, playNotificationSound: false);
							}
							else if (!arrowType.HasValue)
							{
								componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage("Load a bolt first", Color.White, blinking: true, playNotificationSound: false);
							}
							else
							{
								Vector3 vector = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition + componentMiner.ComponentCreature.ComponentBody.Matrix.Right * 0.3f - componentMiner.ComponentCreature.ComponentBody.Matrix.Up * 0.2f;
								Vector3 v2 = Vector3.Normalize(vector + aim.Direction * 10f - vector);
								int value2 = Terrain.MakeBlockValue(192, 0, ArrowBlock.SetArrowType(0, arrowType.Value));
								float s = 38f;
								if (m_subsystemProjectiles.FireProjectile(value2, vector, s * v2, Vector3.Zero, componentMiner.ComponentCreature) != null)
								{
									data = CrossbowBlock.SetArrowType(data, null);
									m_subsystemAudio.PlaySound("Audio/Bow", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 3f, 0.05f);
								}
							}
							inventory.RemoveSlotItems(activeSlotIndex, 1);
							int value3 = Terrain.MakeBlockValue(num, 0, CrossbowBlock.SetDraw(data, 0));
							inventory.AddSlotItems(activeSlotIndex, value3, 1);
							if (draw > 0)
							{
								componentMiner.DamageActiveTool(1);
								m_subsystemAudio.PlaySound("Audio/CrossbowBoing", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 3f, 0f);
							}
							m_aimStartTimes.Remove(componentMiner);
							break;
						}
						}
					}
				}
			}
			return false;
		}

		public override int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
		{
			int num = Terrain.ExtractContents(value);
			ArrowBlock.ArrowType arrowType = ArrowBlock.GetArrowType(Terrain.ExtractData(value));
			if (num == 192 && m_supportedArrowTypes.Contains(arrowType))
			{
				int data = Terrain.ExtractData(inventory.GetSlotValue(slotIndex));
				ArrowBlock.ArrowType? arrowType2 = CrossbowBlock.GetArrowType(data);
				int draw = CrossbowBlock.GetDraw(data);
				if (!arrowType2.HasValue && draw == 15)
				{
					return 1;
				}
				return 0;
			}
			return 0;
		}

		public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
		{
			if (processCount == 1)
			{
				ArrowBlock.ArrowType arrowType = ArrowBlock.GetArrowType(Terrain.ExtractData(value));
				int data = Terrain.ExtractData(inventory.GetSlotValue(slotIndex));
				processedValue = 0;
				processedCount = 0;
				inventory.RemoveSlotItems(slotIndex, 1);
				inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(200, 0, CrossbowBlock.SetArrowType(data, arrowType)), 1);
			}
			else
			{
				processedValue = value;
				processedCount = count;
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemProjectiles = base.Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			base.Load(valuesDictionary);
		}
	}
}
