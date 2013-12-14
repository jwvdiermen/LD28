using FarseerPhysics.DebugViews;
using LD28.Entities.Player;
using LD28.Entities.Terrain;
using LD28.Entity;
using LD28.Movables;
using LD28.Scene;
using LD28.Screen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD28.Screens
{
	public class MainLevel : IScreen
	{
		private readonly LD28Game _game;
		private readonly GraphicsDevice _graphics;
		private readonly ContentManager _content;

		private SpriteBatch _spriteBatch;

		private DebugViewXNA _debugView;

		private ICamera _camera;
		private ISceneNode _cameraNode;

		private DynamicEntityWorld _world;
		private Terrain _terrain;

		private SpaceShip _playerEntity;
		private ISceneNode _dustCloud;

		private ITerrainBrush _circleBrush;
		private ISceneNode _circleBrushNode;
		private MouseState _oldMouseState;

		public bool IsPopup
		{
			get { return false; }
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		public MainLevel(LD28Game game)
		{
			_game = game;
			_graphics = _game.GraphicsDevice;
			_content = game.Content;
		}

		public void LoadContent()
		{
			_spriteBatch = new SpriteBatch(_graphics);
			_world = new DynamicEntityWorld(_game.Services);
			var pp = _graphics.PresentationParameters;

			_debugView = new DebugViewXNA(_world.PhysicsWorld);
			_debugView.LoadContent(_graphics, _content);

			_camera = new DefaultCamera(null, new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight));
			_cameraNode = _world.Scene.Root.CreateChild("Camera");
			_cameraNode.Attach(_camera);

			// Create the tile map.
			_terrain = new Terrain(new Vector2(256.0f, 256.0f), 128.0f, 6)
			{
				DebugEnabled = true,
				Position = new Vector3(50.0f, -128.0f, 0.0f)
			};
			_world.Add(_terrain);

			// Create the circle brush.
			_circleBrush = new TerrainCircleBrush(2.5f);

			_circleBrushNode = _world.Scene.CreateSceneNode();
			_circleBrushNode.Attach(new CircleRenderable(2.5f, 64)
			{
				Color = Vector3.One
			});

			// Dust cloud
			_dustCloud = _world.Scene.CreateSceneNode("DustCloud");
			_dustCloud.Attach(new DustCloud());
			_world.Scene.Root.AddChild(_dustCloud);

			// Player
			_playerEntity = new SpaceShip();
			_playerEntity.Attach(new PlayerController());
			_playerEntity.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90.0f));

			_world.Add(_playerEntity);
		}

		public void UnloadContent()
		{
			_world.Dispose();
			_spriteBatch.Dispose();
			_debugView.Dispose();
		}

		public void Update(GameTime gameTime, bool isActive, bool isOverlayed)
		{
			_world.Update(gameTime, _camera);

			// Follow player.
			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			var cameraVector = _playerEntity.Position - _cameraNode.Position;
			_cameraNode.Position += cameraVector * dt * 5.0f;

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
			_graphics.Clear(Color.Black);

			// Draw the terrain.
			_terrain.Draw(gameTime, _camera);

			foreach (IRenderable renderable in _world.Scene.Collect(typeof(IRenderable)))
			{
				renderable.Render(gameTime, _camera);
			}

			Matrix projectionMatrix, viewMatrix;
			_camera.GetMatrices(out projectionMatrix, out viewMatrix);
			_debugView.RenderDebugData(ref projectionMatrix, ref viewMatrix);
		}
	}
}
