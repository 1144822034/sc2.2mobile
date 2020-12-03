using Engine;
using Engine.Graphics;
using Engine.Input;

namespace Game
{
	public class DebugCamera : BasePerspectiveCamera
	{
		public static string AmbientParameters = string.Empty;

		public static string PlantParameters = string.Empty;

		public Vector3 m_position;

		public Vector3 m_direction;

		public PrimitivesRenderer2D PrimitivesRenderer2D = new PrimitivesRenderer2D();

		public override bool UsesMovementControls => true;

		public override bool IsEntityControlEnabled => false;

		public DebugCamera(GameWidget gameWidget)
			: base(gameWidget)
		{
		}

		public override void Activate(Camera previousCamera)
		{
			m_position = previousCamera.ViewPosition;
			m_direction = previousCamera.ViewDirection;
			SetupPerspectiveCamera(m_position, m_direction, Vector3.UnitY);
		}

		public override void Update(float dt)
		{
			dt = MathUtils.Min(dt, 0.1f);
			Vector3 zero = Vector3.Zero;
			if (Keyboard.IsKeyDown(Key.A))
			{
				zero.X = -1f;
			}
			if (Keyboard.IsKeyDown(Key.D))
			{
				zero.X = 1f;
			}
			if (Keyboard.IsKeyDown(Key.W))
			{
				zero.Z = 1f;
			}
			if (Keyboard.IsKeyDown(Key.S))
			{
				zero.Z = -1f;
			}
			Vector2 vector = 0.03f * new Vector2(Mouse.MouseMovement.X, -Mouse.MouseMovement.Y);
			bool num = Keyboard.IsKeyDown(Key.Shift);
			bool flag = Keyboard.IsKeyDown(Key.Control);
			Vector3 direction = m_direction;
			Vector3 unitY = Vector3.UnitY;
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(direction, unitY));
			float num2 = 8f;
			if (num)
			{
				num2 *= 10f;
			}
			if (flag)
			{
				num2 /= 10f;
			}
			Vector3 zero2 = Vector3.Zero;
			zero2 += num2 * zero.X * vector2;
			zero2 += num2 * zero.Y * unitY;
			zero2 += num2 * zero.Z * direction;
			m_position += zero2 * dt;
			m_direction = Vector3.Transform(m_direction, Matrix.CreateFromAxisAngle(unitY, -4f * vector.X * dt));
			m_direction = Vector3.Transform(m_direction, Matrix.CreateFromAxisAngle(vector2, 4f * vector.Y * dt));
			SetupPerspectiveCamera(m_position, m_direction, Vector3.UnitY);
			Vector2 v = ViewportSize / 2f;
			FlatBatch2D flatBatch2D = PrimitivesRenderer2D.FlatBatch(0, DepthStencilState.None);
			int count = flatBatch2D.LineVertices.Count;
			flatBatch2D.QueueLine(v - new Vector2(5f, 0f), v + new Vector2(5f, 0f), 0f, Color.White);
			flatBatch2D.QueueLine(v - new Vector2(0f, 5f), v + new Vector2(0f, 5f), 0f, Color.White);
			flatBatch2D.TransformLines(ViewportMatrix, count);
			PrimitivesRenderer2D.Flush();
		}
	}
}
