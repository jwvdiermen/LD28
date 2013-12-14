using System;
using System.Collections.Generic;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Movables
{
	/// <summary>
	/// Renders a circle of the given radius. Use this for debugging.
	/// </summary>
	public class CircleRenderable : Movable, IRenderable
	{
		/// <summary>
		/// Gets or sets the color.
		/// </summary>
		public Vector3 Color
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the radius.
		/// </summary>
		public float Radius
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the segment count.
		/// </summary>
		public int Segments
		{
			get;
			private set;
		}

		private GraphicsDevice _graphics;
		private readonly RasterizerState _rasterizerState;

		private VertexBuffer _vertexBuffer;
		private BasicEffect _basicEffect;

		/// <summary>
		/// Creates a new movable with the given name.
		/// </summary>
		/// <param name="radius">The radius.</param>
		/// <param name="segments">The number of segments.</param>
		/// <param name="name">The optional name of the movable.</param>
		public CircleRenderable(float radius, int segments, string name = null)
			: base(name)
		{
			this.Color = Vector3.One;
			this.Radius = radius;
			this.Segments = segments;

			_rasterizerState = new RasterizerState
			{
				CullMode = CullMode.CullCounterClockwiseFace,
				FillMode = FillMode.WireFrame
			};
		}

		protected override void DisposeUnmanaged()
		{
			base.DisposeUnmanaged();
			this.UnloadContent();
		}

		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Generate the buffers.
			var vertices = new List<VertexPositionColor>();

			var step = Math.PI * 2 / (double)this.Segments;
			var current = 0.0;
			for (int i = 0; i < this.Segments; ++i)
			{
				vertices.Add(new VertexPositionColor()
				{
					Position = new Vector3((float)Math.Sin(current) * this.Radius, (float)Math.Cos(current) * this.Radius, 0.0f),
					Color = Microsoft.Xna.Framework.Color.White
				});
				current += step;
			}
			vertices.Add(new VertexPositionColor()
			{
				Position = new Vector3(0.0f, this.Radius, 0.0f),
				Color = Microsoft.Xna.Framework.Color.White
			});

			_vertexBuffer = new VertexBuffer(_graphics, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.None);
			_vertexBuffer.SetData(vertices.ToArray());

			// Create the effect.
			_basicEffect = new BasicEffect(_graphics);
		}

		public void UnloadContent()
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
		}

		public void Render(GameTime gameTime, ICamera camera)
		{
			if (_vertexBuffer != null)
			{
				// Set the effect's parameters.
				Matrix projectionMatrix, viewMatrix;
				camera.GetMatrices(out projectionMatrix, out viewMatrix);

				_basicEffect.Projection = projectionMatrix;
				_basicEffect.View = viewMatrix;
				_basicEffect.World = SceneNode.Transformation;

				_basicEffect.DiffuseColor = this.Color;

				// Set the rasterizer state.
				_graphics.RasterizerState = _rasterizerState;

				// Draw the vertex buffer.
				foreach (var effectPass in _basicEffect.CurrentTechnique.Passes)
				{
					effectPass.Apply();

					_graphics.SetVertexBuffer(_vertexBuffer);
					_graphics.DrawPrimitives(PrimitiveType.LineStrip, 0, _vertexBuffer.VertexCount - 1);
				}
			}
		}
	}
}
