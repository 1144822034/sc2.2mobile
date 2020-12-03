using System;

namespace Game
{	
	public class Screen : CanvasWidget
	{
		public static Action Init;
		public Screen() {
			if (Init != null)Init() ;
		}
		public virtual void Enter(object[] parameters)
		{
		}

		public virtual void Leave()
		{
		}
	}
}
