using Engine;
using GameEntitySystem;
using System;
using TemplatesDatabase;

namespace Game
{
	public abstract class SubsystemBlockBehavior : Subsystem
	{
		public abstract int[] HandledBlocks
		{
			get;
		}

		public SubsystemTerrain SubsystemTerrain
		{
			get;
			set;
		}

		public virtual void OnChunkInitialized(TerrainChunk chunk)
		{
		}

		public virtual void OnChunkDiscarding(TerrainChunk chunk)
		{
		}

		public virtual void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
		}

		public virtual void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
		}

		public virtual void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
		}

		public virtual void OnBlockModified(int value, int oldValue, int x, int y, int z)
		{
		}

		public virtual void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
		}

		public virtual bool OnUse(Ray3 ray, ComponentMiner componentMiner)
		{
			return false;
		}

		public virtual bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			return false;
		}

		public virtual bool OnAim(Ray3 aim, ComponentMiner componentMiner, AimState state)
		{
			return false;
		}

		public virtual bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			return false;
		}

		public virtual bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			return false;
		}

		public virtual void OnItemPlaced(int x, int y, int z, ref BlockPlacementData placementData, int itemValue)
		{
		}

		public virtual void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
		{
		}

		public virtual void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
		}

		public virtual void OnExplosion(int value, int x, int y, int z, float damage)
		{
		}

		public virtual void OnFiredAsProjectile(Projectile projectile)
		{
		}

		public virtual bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
		{
			return false;
		}

		public virtual void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
		}

		public virtual int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
		{
			return 0;
		}

		public virtual void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
		{
			throw new InvalidOperationException("Cannot process items.");
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			SubsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		}
	}
}
