using System.Collections.Generic;

namespace LD28.Screen
{
	internal class ScreenContext
	{
		public ScreenContext(IScreen screen, IEnumerable<IScreen> underlayingScreens)
		{
			Screen = screen;
			UnderlayingScreens = underlayingScreens;
		}

		public IScreen Screen
		{
			get;
			private set;
		}

		public IEnumerable<IScreen> UnderlayingScreens
		{
			get;
			private set;
		}
	}
}
