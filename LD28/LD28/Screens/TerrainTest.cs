using LD28.Entities.Terrain;
using LD28.Entity;
using LD28.Movables;
using LD28.Scene;
using LD28.Screen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD28.Screens
{
	/// <summary>
	/// This screen shows the level.
	/// </summary>
	public class TerrainTest : IScreen
	{
		private readonly LD28Game _game;
		private readonly GraphicsDevice _graphics;

		private SpriteBatch _spriteBatch;

		private ICamera _camera;
		private ISceneNode _cameraNode;

		private ITerrainBrush _circleBrush;
		private ISceneNode _circleBrushNode;

		private IEntityWorld _world;
		private Terrain _terrain;

		private MouseState _oldMouseState;

		public bool IsPopup
		{
			get { return false; }
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		public TerrainTest(LD28Game game)
		{
			_game = game;
			_graphics = _game.GraphicsDevice;
		}

		public void LoadContent()
		{
			_spriteBatch = new SpriteBatch(_graphics);

			_world = new StaticEntityWorld(_game.Services);

			var pp = _graphics.PresentationParameters;
			_camera = new DefaultCamera(null, new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight));
			_cameraNode = _world.Scene.Root.CreateChild("Camera");
			_cameraNode.Position = new Vector3(_camera.ScreenSize / 2.0f, 0.0f);
			_cameraNode.Scale = new Vector3(.5f);
			_cameraNode.Attach(_camera);

			// Create the tile map.
			_terrain = new Terrain(new Vector2(102.4f, 102.4f), 51.2f, 9);
			_terrain.DebugEnabled = true;
			_world.Add(_terrain);		
	
			// Create the circle brush.
			_circleBrush = new CircleBrush(2.5f);

			_circleBrushNode = _world.Scene.CreateSceneNode();
			_circleBrushNode.Attach(new CircleRenderable(2.5f, 64)
			{
				Color = Vector3.One
			});
		}

		public void UnloadContent()
		{
			// Dispose entities.
			_world.Dispose();

			// Dispose graphics resources.
			_spriteBatch.Dispose();
		}

		public void Update(GameTime gameTime, bool isActive, bool isOverlayed)
		{
			_world.Update(gameTime, _camera);

			#region Camera
			// Move the camera.
			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			var keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.A))
			{
				_cameraNode.Move(new Vector3(-50.0f * dt, 0.0f, 0.0f));
			}
			if (keyboardState.IsKeyDown(Keys.D))
			{
				_cameraNode.Move(new Vector3(50.0f * dt, 0.0f, 0.0f));
			}
			if (keyboardState.IsKeyDown(Keys.S))
			{
				_cameraNode.Move(new Vector3(0.0f, 50.0f * dt, 0.0f));
			}
			if (keyboardState.IsKeyDown(Keys.W))
			{
				_cameraNode.Move(new Vector3(0.0f, -50.0f * dt, 0.0f));
			}

			if (keyboardState.IsKeyDown(Keys.E))
			{
				_cameraNode.Scale += Vector3.One * 1.0f * dt;
			}
			if (keyboardState.IsKeyDown(Keys.Q))
			{
				var scale = _cameraNode.Scale;

				scale -= Vector3.One * 1.0f * dt;
				if (scale.X < 0.0f)
				{
					scale.X = 0.0f;
				}
				if (scale.Y < 0.0f)
				{
					scale.Y = 0.0f;
				}

				_cameraNode.Scale = scale;
			}
			#endregion

			#region Circle brush

			var mouseState = Mouse.GetState();

			// Translate the mouse position to the scene.
			var mousePosition = new Vector3(mouseState.X, mouseState.Y, 0.0f);
			
			Matrix projectionMatrix, viewMatrix;
			_camera.GetMatrices(out projectionMatrix, out viewMatrix);

			mousePosition = _camera.Viewport.Unproject(mousePosition, projectionMatrix, viewMatrix, Matrix.Identity);

			// Position the brush.
			_circleBrushNode.Position = new Vector3(mousePosition.X, mousePosition.Y, 0.0f);

			// Handle mouse click.
			// Remove quads by setting their state to disabled using the left mouse button.
			if (mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
			{
				_circleBrushNode.FindMovable<CircleRenderable>().Color = new Vector3(1.0f, 0.0f, 0.0f);
			}
			else if (mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Pressed)
			{
				var position = _circleBrushNode.Position;
				_circleBrush.Position = new Vector2(position.X, position.Y);
				_terrain.SetQuads(_circleBrush, false);
			}
			else if (mouseState.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
			{
				_circleBrushNode.FindMovable<CircleRenderable>().Color = Vector3.One;
			}

			// Add quads by setting their state to enabled using the right mouse button.
			if (mouseState.RightButton == ButtonState.Pressed && _oldMouseState.RightButton == ButtonState.Released)
			{
				_circleBrushNode.FindMovable<CircleRenderable>().Color = new Vector3(0.0f, 1.0f, 0.0f);
			}
			else if (mouseState.RightButton == ButtonState.Pressed && _oldMouseState.RightButton == ButtonState.Pressed)
			{
				var position = _circleBrushNode.Position;
				_circleBrush.Position = new Vector2(position.X, position.Y);
				_terrain.SetQuads(_circleBrush, true);
			}
			else if (mouseState.RightButton == ButtonState.Released && _oldMouseState.RightButton == ButtonState.Pressed)
			{
				_circleBrushNode.FindMovable<CircleRenderable>().Color = Vector3.One;
			}

			// Store old mouse state.
			_oldMouseState = mouseState;

			#endregion
		}

		public void Draw(GameTime gameTime)
		{
			// Clear our screen texture.
			_camera.Apply(_graphics);
			_graphics.Clear(Color.CornflowerBlue);

			// Draw the terrain.
			_terrain.Draw(gameTime, _camera);		
	
			// Draw the brush.
			_circleBrushNode.FindMovable<CircleRenderable>().Render(gameTime, _camera);
		}
	}
}
