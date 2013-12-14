using System;
using System.Collections.Generic;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Movables
{
	/// <summary>
	/// Base class for simple geometric primitive models. This provides a vertex
	/// buffer, an index buffer, plus methods for drawing the model. Classes for
	/// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
	/// derived from this common base, and use the AddVertex and AddIndex methods
	/// to specify their geometry.
	/// </summary>
	public abstract class GeometricPrimitive : Movable, IRenderable
	{
		private readonly List<VertexPositionNormal> _vertices = new List<VertexPositionNormal>();
		private readonly List<ushort> _indices = new List<ushort>();

		private GraphicsDevice _graphics;
		private readonly RasterizerState _rasterizerState;

		private VertexBuffer _vertexBuffer;
		private IndexBuffer _indexBuffer;
		private BasicEffect _basicEffect;

		protected GeometricPrimitive()
		{
			_rasterizerState = new RasterizerState
			{
				CullMode = CullMode.CullCounterClockwiseFace,
				FillMode = FillMode.Solid
			};
		}

		/// <summary>
		/// Adds a new vertex to the primitive model. This should only be called
		/// during the initialization process, before InitializePrimitive.
		/// </summary>
		protected void AddVertex(Vector3 position, Vector3 normal)
		{
			_vertices.Add(new VertexPositionNormal(position, normal));
		}

		/// <summary>
		/// Adds a new index to the primitive model. This should only be called
		/// during the initialization process, before InitializePrimitive.
		/// </summary>
		protected void AddIndex(int index)
		{
			if (index > ushort.MaxValue)
				throw new ArgumentOutOfRangeException("index");

			_indices.Add((ushort)index);
		}

		/// <summary>
		/// Queries the index of the current vertex. This starts at
		/// zero, and increments every time AddVertex is called.
		/// </summary>
		protected int CurrentVertex
		{
			get { return _vertices.Count; }
		}

		protected override void DisposeUnmanaged()
		{
			base.DisposeUnmanaged();
			UnloadContent();
		}

		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Create a vertex declaration, describing the format of our vertex data.

			// Create a vertex buffer, and copy our vertex data into it.
			_vertexBuffer = new VertexBuffer(_graphics,
				typeof(VertexPositionNormal),
				_vertices.Count, BufferUsage.None);

			_vertexBuffer.SetData(_vertices.ToArray());

			// Create an index buffer, and copy our index data into it.
			_indexBuffer = new IndexBuffer(_graphics, typeof(ushort),
										  _indices.Count, BufferUsage.None);

			_indexBuffer.SetData(_indices.ToArray());

			// Create a BasicEffect, which will be used to render the primitive.
			_basicEffect = new BasicEffect(_graphics);

			_basicEffect.EnableDefaultLighting();
		}

		public void UnloadContent()
		{
			if (_vertexBuffer != null)
				_vertexBuffer.Dispose();

			if (_indexBuffer != null)
				_indexBuffer.Dispose();

			if (_basicEffect != null)
				_basicEffect.Dispose();
		}

		public void Render(GameTime gameTime, ICamera camera)
		{
			if (_vertexBuffer != null && _indexBuffer != null)
			{
				// Set the effect's parameters.
				Matrix projectionMatrix, viewMatrix;
				camera.GetMatrices(out projectionMatrix, out viewMatrix);

				_basicEffect.Projection = projectionMatrix;
				_basicEffect.View = viewMatrix;
				_basicEffect.World = SceneNode.Transformation;

				// Set the rasterizer state.
				_graphics.DepthStencilState = DepthStencilState.Default;
				_graphics.RasterizerState = _rasterizerState;

				// Draw the vertex buffer.
				foreach (var effectPass in _basicEffect.CurrentTechnique.Passes)
				{
					effectPass.Apply();

					int primitiveCount = _indices.Count / 3;

					_graphics.SetVertexBuffer(_vertexBuffer);
					_graphics.Indices = _indexBuffer;

					_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
						_vertices.Count, 0, primitiveCount);
				}
			}
		}
	}
}
