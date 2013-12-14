using System;
using System.Net.Mime;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Sprite
{
	/// <summary>
	/// This class implements the <see cref="IStaticSprite" /> and provides default functionality.
	/// </summary>
	public class StaticSprite : Movable, IStaticSprite
	{
		private BasicEffect _basicEffect;
		private GraphicsDevice _graphics;
		private IndexBuffer _indexBuffer;
		private VertexBuffer _vertexBuffer;

		public SpriteEffects Effects { get; set; }

		public BlendState BlendState { get; set; }

		public RasterizerState RasterizerState { get; set; }

		public SamplerState SamplerState { get; set; }

		public Vector2 Size { get; protected set; }

		public Vector2 Pivot { get; protected set; }

		public Texture2D Texture { get; protected set; }

		public Rectangle Source { get; protected set; }

		/// <summary> 
		/// Creates a new static sprite.
		/// </summary>
		protected StaticSprite()
			: this(null)
		{
		}

		/// <summary>
		/// Creates a new static sprite.
		/// </summary>
		/// <param name="name">The name.</param>
		protected StaticSprite(string name)
			: base(name)
		{
			RasterizerState = RasterizerState.CullClockwise;
			BlendState = BlendState.AlphaBlend;

			SamplerState = new SamplerState
			{
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				Filter = TextureFilter.LinearMipPoint
			};
		}

		/// <summary>
		/// Creates a new static sprite with the pivot in the center.
		/// </summary>
		/// <param name="size">The size of the sprite.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="name">The name.</param>
		public StaticSprite(Vector2 size, Texture2D texture, string name)
			: this(size, size / 2.0f, texture, name, null)
		{
		}

		/// <summary>
		/// Creates a new static sprite with the pivot in the center.
		/// </summary>
		/// <param name="size">The size of the sprite.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="name">The name.</param>
		/// <param name="source">The source.</param>
		public StaticSprite(Vector2 size, Texture2D texture, string name, Rectangle? source)
			: this(size, size / 2.0f, texture, name, source)
		{
		}

		/// <summary>
		/// Creates a new static sprite.
		/// </summary>
		/// <param name="size">The size of the sprite.</param>
		/// <param name="pivot">The pivot of the sprite.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="name">The name.</param>
		public StaticSprite(Vector2 size, Vector2 pivot, Texture2D texture, string name)
			: this(size, pivot, texture, name, null)
		{
		}

		/// <summary>
		/// Creates a new static sprite.
		/// </summary>
		/// <param name="size">The size of the sprite.</param>
		/// <param name="pivot">The pivot of the sprite.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="name">The name.</param>
		/// <param name="source">The source.</param>
		public StaticSprite(Vector2 size, Vector2 pivot, Texture2D texture, string name, Rectangle? source)
			: this(name)
		{
			Size = size;
			Pivot = pivot;
			Texture = texture;
			Source = source != null ? (Rectangle)source : new Rectangle(0, 0, texture.Width, texture.Height);
		}

		public void Update(GameTime time, ICamera camera)
		{
		}

		public virtual void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof (IGraphicsDeviceService))).GraphicsDevice;

			// Determine the texture coordinates.
			float sx = 1.0f / Texture.Width;
			float sy = 1.0f / Texture.Height;

			var uvTopLeft = new Vector2(sx * Source.Left, sy * Source.Top);
			var uvBottomRight = new Vector2(sx * Source.Right, sy * Source.Bottom);

			// Generate the vertex buffer.
			Vector2 topLeft = -Pivot;
			var vertices = new[]
			{
				new VertexPositionColorTexture
				{
					Position = new Vector3(topLeft.X, topLeft.Y, 0.0f),
					TextureCoordinate = new Vector2(uvTopLeft.X, uvTopLeft.Y),
					Color = Color.White
				},
				new VertexPositionColorTexture
				{
					Position = new Vector3(topLeft.X + Size.X, topLeft.Y, 0.0f),
					TextureCoordinate = new Vector2(uvBottomRight.X, uvTopLeft.Y),
					Color = Color.White
				},
				new VertexPositionColorTexture
				{
					Position = new Vector3(topLeft.X + Size.X, topLeft.Y + Size.Y, 0.0f),
					TextureCoordinate = new Vector2(uvBottomRight.X, uvBottomRight.Y),
					Color = Color.White
				},
				new VertexPositionColorTexture
				{
					Position = new Vector3(topLeft.X, topLeft.Y + Size.Y, 0.0f),
					TextureCoordinate = new Vector2(uvTopLeft.X, uvBottomRight.Y),
					Color = Color.White
				}
			};

			_vertexBuffer = new VertexBuffer(_graphics, VertexPositionColorTexture.VertexDeclaration, vertices.Length,
			                                  BufferUsage.None);
			_vertexBuffer.SetData(vertices);

			// Generate the index buffer.
			var indices = new ushort[] { 0, 3, 1, 1, 3, 2 };

			_indexBuffer = new IndexBuffer(_graphics, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			_indexBuffer.SetData(indices);

			// Create the effect.
			_basicEffect = new BasicEffect(_graphics)
			{
				Texture = Texture,
				TextureEnabled = true,
				DiffuseColor = Vector3.One
			};
		}

		public virtual void UnloadContent()
		{
			if (_basicEffect != null)
			{
				_basicEffect.Dispose();
				_basicEffect = null;
			}
			if (_vertexBuffer != null)
			{
				_vertexBuffer.Dispose();
				_vertexBuffer = null;
			}
			if (_indexBuffer != null)
			{
				_indexBuffer.Dispose();
				_indexBuffer = null;
			}
		}

		public void Render(GameTime gameTime, ICamera camera)
		{
			if (_vertexBuffer != null && _indexBuffer != null && _basicEffect != null)
			{
				// Set the effect's parameters.
				Matrix projectionMatrix, viewMatrix;
				camera.GetMatrices(out projectionMatrix, out viewMatrix);

				_basicEffect.Projection = projectionMatrix;
				_basicEffect.View = viewMatrix;
				_basicEffect.World = SceneNode.Transformation;

				// Set the rasterizer state.
				_graphics.DepthStencilState = DepthStencilState.Default;
				_graphics.BlendState = BlendState;
				_graphics.RasterizerState = RasterizerState;
				_graphics.SamplerStates[0] = SamplerState;

				// Draw the vertex buffer.
				foreach (var effectPass in _basicEffect.CurrentTechnique.Passes)
				{
					effectPass.Apply();

					_graphics.SetVertexBuffer(_vertexBuffer);
					_graphics.Indices = _indexBuffer;

					_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0,
					                                 _indexBuffer.IndexCount / 3);
				}
			}
		}
	}
}