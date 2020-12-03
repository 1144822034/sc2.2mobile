namespace Game
{
	public class DetonatorElectricElement : MountedElectricElement
	{
		public DetonatorElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
		}

		public void Detonate()
		{
			CellFace cellFace = base.CellFaces[0];
			int value = Terrain.MakeBlockValue(147);
			base.SubsystemElectricity.Project.FindSubsystem<SubsystemExplosions>(throwOnError: true).TryExplodeBlock(cellFace.X, cellFace.Y, cellFace.Z, value);
		}

		public override bool Simulate()
		{
			if (CalculateHighInputsCount() > 0)
			{
				Detonate();
			}
			return false;
		}

		public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
			Detonate();
		}
	}
}
