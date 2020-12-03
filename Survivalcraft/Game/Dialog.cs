using Engine;

namespace Game
{
	public class Dialog : CanvasWidget
	{
		public Dialog()
		{
			IsHitTestVisible = true;
			base.Size = new Vector2(float.PositiveInfinity);
		}
	}
}
