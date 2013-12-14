using LD28.Screen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD28
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class LD28Game : Microsoft.Xna.Framework.Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private IScreenManager _screenManager;

#if DEBUG
		private DebugInfo _debugInfo;
#endif

		public LD28Game()
		{
			_graphics = new GraphicsDeviceManager(this);

			// Window options.
			Window.Title = "LD48";
			IsMouseVisible = true;

			// Set a default size for the window.
			_graphics.PreferredBackBufferWidth = 1280;
			_graphics.PreferredBackBufferHeight = 720;

			// We don't need a depth stencil in this game.
			_graphics.PreferredDepthStencilFormat = DepthFormat.None;

			// Initialize the content manager.
			Content.RootDirectory = "Content";
			Services.AddService(typeof(ContentManager), Content);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			_screenManager = new ScreenManager(this);
			_screenManager.ActivateScreen(new Screens.TerrainTest(this));

			Components.Add(_screenManager);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
#if DEBUG
			_debugInfo = new DebugInfo();
			_debugInfo.LoadContent(Services);
#endif
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
#if DEBUG
			if (_debugInfo != null)
			{
				_debugInfo.Dispose();
				_debugInfo = null;
			}
#endif
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

#if DEBUG
			_debugInfo.Render(gameTime);
#endif
		}
	}
}
