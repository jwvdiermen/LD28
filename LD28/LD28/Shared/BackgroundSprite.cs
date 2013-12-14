using System;
using LD28.Scene;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LD28.Shared
{
	/// <summary>
	/// A renderable that, when rendered, fills the complete viewport of the camera
	/// it is rendered to.
	/// </summary>
	public class BackgroundSprite : Movable, IRenderable
	{
		private readonly RasterizerState _rasterizerState;
		private GraphicsDevice _graphics;
		private BasicEffect _basicEffect;

		/// <summary>
		/// Creates a new instance of the Background class.
		/// </summary>
		/// <param name="texture">The texture.</param>
		/// <param name="scale">The scale.</param>
		public BackgroundSprite(Texture2D texture, float scale)
		{
			Texture = texture;
			Scale = scale;

			_rasterizerState = new RasterizerState
			{
				CullMode = CullMode.CullClockwiseFace,
				FillMode = FillMode.Solid
			};
		}

		/// <summary>
		/// Gets the scale. A scale of 1 means that the texture 
		/// </summary>
		public float Scale { get; private set; }

		/// <summary>
		/// Gets the texture.
		/// </summary>
		public Texture2D Texture { get; private set; }

		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Create the effect.
			_basicEffect = new BasicEffect(_graphics)
			{
				Texture = Texture,
				TextureEnabled = true,
				DiffuseColor = Vector3.One
			};
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
				// Generate the vertices and indices.
				var screenSize = camera.ScreenSize;
				var halfWidth = (screenSize.X / 2.0f) * 1.01f;
				var halfHeight = (screenSize.Y / 2.0f) * 1.01f;
				var uv = screenSize / Scale;

				var vertices = new[]
				{
					new VertexPositionColorTexture
					{
						Position = new Vector3(-halfWidth, -halfHeight, 0.0f),
						TextureCoordinate = new Vector2(0.0f, 0.0f),
						Color = Color.White
					},
					new VertexPositionColorTexture
					{
						Position = new Vector3(halfWidth, -halfHeight, 0.0f),
						TextureCoordinate = new Vector2(uv.X, 0.0f),
						Color = Color.White
					},
					new VertexPositionColorTexture
					{
						Position = new Vector3(halfWidth, halfHeight, 0.0f),
						TextureCoordinate = new Vector2(uv.X, uv.Y),
						Color = Color.White
					},
					new VertexPositionColorTexture
					{
						Position = new Vector3(-halfWidth, halfHeight, 0.0f),
						TextureCoordinate = new Vector2(0.0f, uv.Y),
						Color = Color.White
					}
				};

				var indices = new short[] { 0, 3, 1, 1, 3, 2 };

				// Set the effect's parameters.
				Matrix projectionMatrix, viewMatrix;
				camera.GetMatrices(out projectionMatrix, out viewMatrix);

				_basicEffect.Projection = projectionMatrix;
				_basicEffect.View = Matrix.Identity;
				_basicEffect.World = Matrix.Identity;

				// Set the device states.
				_graphics.BlendState = BlendState.Opaque;
				_graphics.RasterizerState = _rasterizerState;
				_graphics.SamplerStates[0] = SamplerState.LinearWrap;

				// Draw.
				foreach (var effectPass in _basicEffect.CurrentTechnique.Passes)
				{
					effectPass.Apply();
					_graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0,
					                                    indices.Length / 3);
				}
			}
		}
	}
}
