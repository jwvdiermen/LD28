using System;
using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Movables
{
	/// <summary>
	/// A dust cloud entity for making the player's movement more visible.
	/// </summary>
	public class DustCloud : Movable, IUpdatable, IRenderable
	{
		private readonly FastRandom _random = new FastRandom();

		private GraphicsDevice _graphics;
		private BasicEffect _basicEffect;
		private RasterizerState _rasterizerState;
		
		private float _particleSize;
		private float _halfParticleSize;

		private Vector2 _lastCameraPosition;

		private bool _hasInitialized = false;

		private Particle[] _particles;
		private VertexPositionColor[] _particleVertices;

		private struct Particle
		{
			public Vector2 Position;
			public Color Color;
		}

		public DustCloud(int size = 100, string name = "DustCloud")
			: base(name)
		{
			_particles = new Particle[size];
			_particleVertices = new VertexPositionColor[size * 3];

			_rasterizerState = new RasterizerState
			{
				CullMode = CullMode.None,
				FillMode = FillMode.Solid
			};
		}

		private void Initialize(ICamera camera)
		{
			// Retrieve the necessary data.
			var vector3 = camera.SceneNode.Position;
			var cameraPosition = new Vector2(vector3.X, vector3.Y);
			var halfScreenSize = camera.ScreenSize / 2.0f;

			_lastCameraPosition = cameraPosition;

			// Use the viewport's and screen's height to determine our particle size since it's contstant.
			// Particle size = 2 pixels.
			_particleSize = camera.ScreenSize.Y / (float)camera.Viewport.Height * 2.0f;
			_halfParticleSize = _particleSize / 2.0f;

			// Initialize the particles.
			for (int i = 0; i < _particles.Length; ++i)
			{
				var particle = new Particle
				{
					Position = new Vector2(
						cameraPosition.X + halfScreenSize.X * _random.NextRangedFloat(),
						cameraPosition.Y + halfScreenSize.Y * _random.NextRangedFloat()),
					Color = new Color(
						1.0f - _random.NextFloat() * 0.2f,
						1.0f - _random.NextFloat() * 0.2f,
						1.0f - _random.NextFloat() * 0.2f,
						1.0f - _random.NextFloat() * 0.2f)
				};

				_particles[i] = particle;
				RebuildPartice(i, ref particle);
			}
			_hasInitialized = true;
		}

		private void RebuildPartice(int index, ref Particle particle)
		{
			index = index * 3;

			_particleVertices[index + 0] = new VertexPositionColor
			{
				Position = new Vector3(particle.Position.X, particle.Position.Y - _halfParticleSize, 0.0f),
				Color = particle.Color
			};
			_particleVertices[index + 1] = new VertexPositionColor
			{
				Position = new Vector3(particle.Position.X - _halfParticleSize, particle.Position.Y + _halfParticleSize, 0.0f),
				Color = particle.Color
			};
			_particleVertices[index + 2] = new VertexPositionColor
			{
				Position = new Vector3(particle.Position.X + _halfParticleSize, particle.Position.Y + _halfParticleSize, 0.0f),
				Color = particle.Color
			};
		}

		public void Update(GameTime time, ICamera camera)
		{
			// Check if we need to initialize.
			if (_hasInitialized == false)
			{
				Initialize(camera);
			}

			// Retrieve the necessary data.
			var vector3 = camera.SceneNode.Position;
			var cameraPosition = new Vector2(vector3.X, vector3.Y);
			var halfScreenSize = camera.ScreenSize / 2.0f;

			var cameraDirection = cameraPosition - _lastCameraPosition;

			var topLeft = cameraPosition - halfScreenSize;
			var bottomRight = cameraPosition + halfScreenSize;

			// Find any particles that moved of the screen.
			for (int i = 0; i < _particles.Length; ++i)
			{
				var particle = _particles[i];
				var position = particle.Position;

				// Check if and how the particle left the viewport.
				bool moved = false;
				if (position.X < topLeft.X && cameraDirection.X > 0.0f)
				{
					position = new Vector2(
						cameraPosition.X + halfScreenSize.X + (_random.NextFloat() * 20.0f),
						cameraPosition.Y + (halfScreenSize.Y + 40.0f) * _random.NextRangedFloat());

					moved = true;
				}
				else if (position.X > bottomRight.X && cameraDirection.X < 0.0f)
				{
					position = new Vector2(
						cameraPosition.X - halfScreenSize.X - (_random.NextFloat() * 20.0f),
						cameraPosition.Y + (halfScreenSize.Y + 40.0f) * _random.NextRangedFloat());

					moved = true;
				}

				if (position.Y < topLeft.Y && cameraDirection.Y > 0.0f)
				{
					position = new Vector2(
						cameraPosition.X + (halfScreenSize.X + 40.0f) * _random.NextRangedFloat(),
						cameraPosition.Y + halfScreenSize.Y + (_random.NextFloat() * 20.0f));

					moved = true;
				}
				else if (position.Y > bottomRight.Y && cameraDirection.Y < 0.0f)
				{
					position = new Vector2(
						cameraPosition.X + (halfScreenSize.X + 40.0f) * _random.NextRangedFloat(),
						cameraPosition.Y - halfScreenSize.Y - (_random.NextFloat() * 20.0f));

					moved = true;
				}

				if (moved)
				{
					particle = new Particle
					{
						Position = position,
						Color = new Color(
							1.0f - _random.NextFloat() * 0.2f,
							1.0f - _random.NextFloat() * 0.2f,
							1.0f - _random.NextFloat() * 0.2f,
							1.0f - _random.NextFloat() * 0.2f)
					};
					_particles[i] = particle;

					RebuildPartice(i, ref particle);
				}
			}

			_lastCameraPosition = cameraPosition;
		}

		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Create the effect.
			_basicEffect = new BasicEffect(_graphics);
			_basicEffect.DiffuseColor = Vector3.One;
			_basicEffect.VertexColorEnabled = true;
		}

		public void UnloadContent()
		{
			if (_basicEffect != null)
			{
				_basicEffect.Dispose();
				_basicEffect = null;
			}
		}

		public void Render(GameTime gameTime, ICamera camera)
		{
			if (_basicEffect != null)
			{
				// Set the effect's parameters.
				Matrix projectionMatrix, viewMatrix;
				camera.GetMatrices(out projectionMatrix, out viewMatrix);

				_basicEffect.Projection = projectionMatrix;
				_basicEffect.View = viewMatrix;
				_basicEffect.World = SceneNode.Transformation;

				// Set the rasterizer state.
				_graphics.BlendState = BlendState.AlphaBlend;
				_graphics.RasterizerState = _rasterizerState;

				// Draw the vertex buffer.
				foreach (var effectPass in _basicEffect.CurrentTechnique.Passes)
				{
					effectPass.Apply();
					_graphics.DrawUserPrimitives(PrimitiveType.TriangleList, _particleVertices, 0, _particles.Length, VertexPositionColor.VertexDeclaration);
				}
			}
		}
	}
}
