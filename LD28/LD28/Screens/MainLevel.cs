using System.Runtime.InteropServices;
using FarseerPhysics.DebugViews;
using FarseerPhysics.Dynamics;
using LD28.Core;
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
using System;

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
		private bool _cameraFreeMode = false;

		private DynamicEntityWorld _world;
		private Terrain _terrain;

		private SpaceShip _playerEntity;
		private Payload _payloadEntity;
		private PayloadChain _payloadChain;

		private ISceneNode _dustCloud;

		private ITerrainBrush _circleBrush;
		private ISceneNode _circleBrushNode;

		private KeyboardState _oldKeyboardState;
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
			var terrainSize = new Vector2(4096.0f, 2048.0f);
			_terrain = new Terrain(terrainSize, 128.0f, 7)
			{
				DebugEnabled = false,
				Position = new Vector3(50.0f, -terrainSize.Y / 2.0f, 0.0f),
				TextureName = @"Textures\SpaceRock"
			};
			_world.Add(_terrain);

			GenerateTunnels(new Vector2(50.0f, 0));

			// Create the circle brush.
			_circleBrush = new CircleBrush(2.5f);

			_circleBrushNode = _world.Scene.CreateSceneNode();
			_circleBrushNode.Attach(new CircleRenderable(2.5f, 64)
			{
				Color = Vector3.One
			});

			// Dust cloud
			_dustCloud = _world.Scene.CreateSceneNode("DustCloud");
			_dustCloud.Position = new Vector3(0.0f, 0.0f, -1.0f);
			_dustCloud.Attach(new DustCloud());
			_world.Scene.Root.AddChild(_dustCloud);

			// Player
			_playerEntity = new SpaceShip();
			_playerEntity.Attach(new PlayerController());
			_playerEntity.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90.0f));
			_world.Add(_playerEntity);

			_payloadEntity = new Payload();
			_payloadEntity.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90.0f));
			_world.Add(_payloadEntity);

			_payloadChain = PayloadChain.Connect(_playerEntity, _payloadEntity, Vector2.UnitX * -20.0f);
		}

		public void UnloadContent()
		{
			_world.Dispose();
			_spriteBatch.Dispose();
			_debugView.Dispose();
		}

		private void GenerateTunnels(Vector2 start, float maxLength = 2000.0f, float turnStrength = 0.2f)
		{
			//1234
			//12345
			//498764867
			//582764
			var random = new FastRandom(498764867);
			const float step = 2.0f;

			var terrainHeight = _terrain.TerrainSize.Y / 2.0f;
			var maxSize = new Vector2(30.0f);

			var currentPosition = start;
			var currentDirection = Vector2.UnitX;

			var length = 0.0f;
			while (length < maxLength)
			{
				var currentSize = maxSize + new Vector2(Math.Max(0.0f, length + 200.0f - maxLength)) * 1.0f;

				var brushSize = new Vector2(random.NextFloat() * currentSize.X, random.NextFloat() * currentSize.Y);
				var brush = new CircleBrush(
					(brushSize.X + brushSize.Y) / 4.0f, 
					currentPosition + new Vector2(random.NextRangedFloat(), random.NextRangedFloat()));

				_terrain.SetQuads(brush, false, true);

				// Add rotation.
				var rotationDir = random.NextRangedFloat();

				// Prevent going outside terrain or going back.
				if (currentPosition.Y > terrainHeight - 200.0f ||
					(currentDirection.X < 0.0f && currentDirection.Y > 0.0f))
				{
					rotationDir = (rotationDir - 1.0f) * 0.1f;
				}
				else if (currentPosition.Y < -(terrainHeight + 200.0f) ||
					(currentDirection.X < 0.0f && currentDirection.Y < 0.0f))
				{
					rotationDir = (rotationDir + 1.0f) * 0.1f;
				}

				// Apply rotation.
				var rotation = rotationDir * turnStrength;
				currentDirection = new Vector2(
					currentDirection.X * MathCore.Cos(rotation) + -currentDirection.Y * MathCore.Sin(rotation),
					currentDirection.Y * MathCore.Cos(rotation) + currentDirection.X * MathCore.Sin(rotation));

				currentPosition += currentDirection * step;
				length += step;
			}

			_terrain.Refresh();
		}

		public void Update(GameTime gameTime, bool isActive, bool isOverlayed)
		{
			_world.Update(gameTime, _camera);

			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			var keyboardState = Keyboard.GetState();
			var mouseState = Mouse.GetState();

			if (_cameraFreeMode)
			{
				// Free mode when enabled.
				if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
				{
					_cameraNode.Position += new Vector3(-Vector2.UnitY, 0.0f) * 100.0f * dt;
				}
				if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
				{
					_cameraNode.Position += new Vector3(Vector2.UnitY, 0.0f) * 100.0f * dt;
				}

				if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
				{
					_cameraNode.Position += new Vector3(Vector2.UnitX, 0.0f) * 100.0f * dt;
				}
				if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
				{
					_cameraNode.Position += new Vector3(-Vector2.UnitX, 0.0f) * 100.0f * dt;
				}
			}
			else
			{
				// Follow player.
				var cameraVector = _playerEntity.Position - _cameraNode.Position;
				_cameraNode.Position += cameraVector * dt * 5.0f;
			}

			if (keyboardState.IsKeyDown(Keys.F) && _oldKeyboardState.IsKeyUp(Keys.F))
			{
				_cameraFreeMode = !_cameraFreeMode;
				_playerEntity.Controller<PlayerController>().Enabled = !_cameraFreeMode;
			}

			#region Circle brush

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

			#endregion

			_oldKeyboardState = keyboardState;
			_oldMouseState = mouseState;
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

			//Matrix projectionMatrix, viewMatrix;
			//_camera.GetMatrices(out projectionMatrix, out viewMatrix);
			//_debugView.RenderDebugData(ref projectionMatrix, ref viewMatrix);
		}
	}
}
