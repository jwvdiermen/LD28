using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LD28.Screen
{
	/// <summary>
	/// Implements the <see cref="IStateManager" /> interface and provides default functionality.
	/// </summary>
	public class ScreenManager : DrawableGameComponent, IScreenManager
	{
		public ScreenManager(Game game)
			: base(game)
		{
		}

		#region Fields

		private readonly Stack<ScreenContext> _screenStack = new Stack<ScreenContext>();
		private bool _contentLoaded = false;

		#endregion

		#region Methods

		protected override void LoadContent()
		{
			foreach (var screenContext in _screenStack)
			{
				screenContext.Screen.LoadContent();
			}

			_contentLoaded = true;
			base.LoadContent();
		}

		protected override void UnloadContent()
		{
			foreach (var screenContext in _screenStack)
			{
				screenContext.Screen.UnloadContent();
			}

			_contentLoaded = false;
			base.UnloadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (_screenStack.Count > 0)
			{
				var activeContext = _screenStack.Peek();

				foreach (var screen in activeContext.UnderlayingScreens)
				{
					screen.Update(gameTime, false, true);
				}

				activeContext.Screen.Update(gameTime, true, false);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (_screenStack.Count > 0)
			{
				var activeContext = _screenStack.Peek();

				// TODO: handle transisions.
				foreach (var screen in activeContext.UnderlayingScreens)
				{
					screen.Draw(gameTime);
				}

				activeContext.Screen.Draw(gameTime);
			}
			else
			{
				GraphicsDevice.Clear(Color.Black);
			}

			base.Draw(gameTime);
		}

		#endregion

		#region IScreenManager Members

		public IScreen ActiveScreen
		{
			get { return _screenStack.Count > 0 ? _screenStack.Peek().Screen : null; }
		}

		public void PushScreen(IScreen screen)
		{
			ScreenContext activeContext = null;
			if (_screenStack.Count > 0)
			{
				activeContext = _screenStack.Peek();
			}

			var underlayingScreens = new List<IScreen>();
			if (screen.IsPopup == true && activeContext != null)
			{
				underlayingScreens.AddRange(activeContext.UnderlayingScreens);
				underlayingScreens.Add(activeContext.Screen);
			}

			if (_contentLoaded)
			{
				screen.LoadContent();
			}

			_screenStack.Push(new ScreenContext(screen, underlayingScreens));
		}

		public IScreen PopScreen()
		{
			ScreenContext activeContext = null;
			if (_screenStack.Count > 0)
			{
				activeContext = _screenStack.Pop();

				if (_contentLoaded == true)
				{
					activeContext.Screen.UnloadContent();
				}
			}			

			return activeContext != null ? activeContext.Screen : null;
		}

		public void ActivateScreen(IScreen screen)
		{
			_screenStack.Clear();
			_screenStack.Push(new ScreenContext(screen, new IScreen[0]));
		}

		#endregion
	}
}
