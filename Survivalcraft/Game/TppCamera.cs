using Engine;

namespace Game
{
	public class TppCamera : BasePerspectiveCamera
	{
		public Vector3 m_position;

		public override bool UsesMovementControls => false;

		public override bool IsEntityControlEnabled => true;

		public TppCamera(GameWidget gameWidget)
			: base(gameWidget)
		{
		}

		public override void Activate(Camera previousCamera)
		{
			m_position = previousCamera.ViewPosition;
			SetupPerspectiveCamera(m_position, previousCamera.ViewDirection, previousCamera.ViewUp);
		}

		public override void Update(float dt)
		{
			if (base.GameWidget.Target == null)
			{
				return;
			}
			Matrix matrix = Matrix.CreateFromQuaternion(base.GameWidget.Target.ComponentCreatureModel.EyeRotation);
			matrix.Translation = base.GameWidget.Target.ComponentBody.Position + 0.5f * base.GameWidget.Target.ComponentBody.BoxSize.Y * Vector3.UnitY;
			Vector3 v = -2.25f * matrix.Forward + 1.75f * matrix.Up;
			Vector3 vector = matrix.Translation + v;
			if (Vector3.Distance(vector, m_position) < 10f)
			{
				Vector3 v2 = vector - m_position;
				float s = 3f * dt;
				m_position += s * v2;
			}
			else
			{
				m_position = vector;
			}
			Vector3 vector2 = m_position - matrix.Translation;
			float? num = null;
			Vector3 vector3 = Vector3.Normalize(Vector3.Cross(vector2, Vector3.UnitY));
			Vector3 v3 = Vector3.Normalize(Vector3.Cross(vector2, vector3));
			for (int i = 0; i <= 0; i++)
			{
				for (int j = 0; j <= 0; j++)
				{
					Vector3 v4 = 0.5f * (vector3 * i + v3 * j);
					Vector3 vector4 = matrix.Translation + v4;
					Vector3 end = vector4 + vector2 + Vector3.Normalize(vector2) * 0.5f;
					TerrainRaycastResult? terrainRaycastResult = base.GameWidget.SubsystemGameWidgets.SubsystemTerrain.Raycast(vector4, end, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => !BlocksManager.Blocks[Terrain.ExtractContents(value)].IsTransparent);
					if (terrainRaycastResult.HasValue)
					{
						num = (num.HasValue ? MathUtils.Min(num.Value, terrainRaycastResult.Value.Distance) : terrainRaycastResult.Value.Distance);
					}
				}
			}
			Vector3 vector5 = (!num.HasValue) ? (matrix.Translation + vector2) : (matrix.Translation + Vector3.Normalize(vector2) * MathUtils.Max(num.Value - 0.5f, 0.2f));
			SetupPerspectiveCamera(vector5, matrix.Translation - vector5, Vector3.UnitY);
		}
	}
}
