using Engine;
using System.Collections.Generic;

namespace Game
{
	public class SpawnChunk
	{
		public Point2 Point;

		public bool IsSpawned;

		public double? LastVisitedTime;

		public List<SpawnEntityData> SpawnsData = new List<SpawnEntityData>();
	}
}
