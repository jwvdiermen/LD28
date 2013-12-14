using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using LD28.Core;
using LD28.Scene;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// This class represents a quad tree, used by the <see cref="Terrain" /> entity.
	/// </summary>
	public class QuadTree : DisposableObject
	{
		private GraphicsDevice _graphics;

		private VertexBuffer _enabledVertexBuffer;
		private IndexBuffer _enabledIndexBuffer;

		private VertexBuffer _disabledVertexBuffer;
		private IndexBuffer _disabledIndexBuffer;

		private VertexBuffer _edgeVertexBuffer;
		private IndexBuffer _edgeIndexBuffer;

		/// <summary>
		/// Gets the root node.
		/// </summary>
		public QuadTreeNode Root { get; private set; }

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="maxDepth">The maximum depth.</param>
		/// <param name="boundingBox">The bounding box.</param>
		public QuadTree(int maxDepth, BoundingBox boundingBox)
		{
			this.Root = new QuadTreeNode(null, maxDepth, boundingBox, true);
		}

		/// <summary>
		/// This method is called when graphics resources need to be loaded.
		/// </summary>
		/// <param name="services">The service provider.</param>
		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Collect our geometry.
			CollectGeometry(true, ref _enabledVertexBuffer, ref _enabledIndexBuffer);
			CollectGeometry(false, ref _disabledVertexBuffer, ref _disabledIndexBuffer);
			//CollectEdgeGeometry(ref m_edgeVertexBuffer, ref m_edgeIndexBuffer);
		}

		/// <summary>
		/// This method is called when graphics resources need to be unloaded.
		/// </summary>
		public void UnloadContent()
		{
			if (_enabledVertexBuffer != null)
			{
				_enabledVertexBuffer.Dispose();
				_enabledVertexBuffer = null;
			}
			if (_enabledIndexBuffer != null)
			{
				_enabledIndexBuffer.Dispose();
				_enabledIndexBuffer = null;
			}

			if (_disabledVertexBuffer != null)
			{
				_disabledVertexBuffer.Dispose();
				_disabledVertexBuffer = null;
			}
			if (_disabledIndexBuffer != null)
			{
				_disabledIndexBuffer.Dispose();
				_disabledIndexBuffer = null;
			}

			if (_edgeVertexBuffer != null)
			{
				_edgeVertexBuffer.Dispose();
				_edgeVertexBuffer = null;
			}
			if (_edgeIndexBuffer != null)
			{
				_edgeIndexBuffer.Dispose();
				_edgeIndexBuffer = null;
			}
		}

		protected override void DisposeUnmanaged()
		{
			// Unload our content when disposed.
			UnloadContent();
		}

		/// <summary>
		/// This method updates the quad tree.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		public void Update(GameTime gameTime)
		{
		}

		private void CollectGeometry(bool enabled, ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
		{
			// Clear the buffers first.
			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
			}
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
				indexBuffer = null;
			}

			// Collect the vertices and indices.
			var vertices = new List<Vector2>();
			var indices = new List<ushort>();

			this.Root.CollectGeometry(vertices, indices, enabled);

			// Create the hardware resources.
			if (vertices.Count > 0 && indices.Count > 0)
			{
				vertexBuffer = new VertexBuffer(_graphics, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.None);
				vertexBuffer.SetData(vertices.Select(e => new VertexPositionColor
				{
					Position = new Vector3(e, 0.0f),
					Color = Color.White

				}).ToArray());

				indexBuffer = new IndexBuffer(_graphics, IndexElementSize.SixteenBits, indices.Count, BufferUsage.None);
				indexBuffer.SetData(indices.ToArray());
			}
		}

		//private void CollectEdgeGeometry(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer)
		//{
		//    // Clear the buffers first.
		//    if (vertexBuffer != null)
		//    {
		//        vertexBuffer.Dispose();
		//        vertexBuffer = null;
		//    }
		//    if (indexBuffer != null)
		//    {
		//        indexBuffer.Dispose();
		//        indexBuffer = null;
		//    }

		//    // Collect the vertices and indices.
		//    var vertices = new List<Vector2>();
		//    var indices = new List<ushort>();

		//    this.Root.CollectEdgeGeometry(vertices, indices);

		//    // Create the hardware resources.
		//    if (vertices.Count > 0 && indices.Count > 0)
		//    {
		//        vertexBuffer = new VertexBuffer(m_graphics, VertexPositionColor.VertexDeclaration, vertices.Count, BufferUsage.None);
		//        vertexBuffer.SetData(vertices.Select(e => new VertexPositionColor
		//        {
		//            Position = new Vector3(e, 0.0f),
		//            Color = Color.White

		//        }).ToArray());

		//        indexBuffer = new IndexBuffer(m_graphics, IndexElementSize.SixteenBits, indices.Count, BufferUsage.None);
		//        indexBuffer.SetData(indices.ToArray());
		//    }
		//}

		/// <summary>
		/// This method draws the quad tree.
		/// </summary>
		/// <param name="enabled">True to render the enabled buffers or false to render the disabled buffers.</param>
		public void Draw(bool enabled)
		{
			if (enabled == true && _enabledVertexBuffer != null && _enabledIndexBuffer != null)
			{
				_graphics.SetVertexBuffer(_enabledVertexBuffer);
				_graphics.Indices = _enabledIndexBuffer;

				_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _enabledVertexBuffer.VertexCount, 0, _enabledIndexBuffer.IndexCount / 3);
			}
			else if (enabled == false && _disabledVertexBuffer != null && _disabledIndexBuffer != null)
			{
				_graphics.SetVertexBuffer(_disabledVertexBuffer);
				_graphics.Indices = _disabledIndexBuffer;

				_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _disabledVertexBuffer.VertexCount, 0, _disabledIndexBuffer.IndexCount / 3);
			}
		}

		/// <summary>
		/// Draws the edge.
		/// </summary>
		public void DrawEdge()
		{
			if (_edgeVertexBuffer != null && _edgeIndexBuffer != null)
			{
				_graphics.SetVertexBuffer(_edgeVertexBuffer);
				_graphics.Indices = _edgeIndexBuffer;

				_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _edgeVertexBuffer.VertexCount, 0, _edgeIndexBuffer.IndexCount / 3);
			}
		}

		/// <summary>
		/// Changes the state of the intersected quads.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="state">The state, either enabled or disabled.</param>
		/// <returns>True if any quads were changed.</returns>
		public bool SetQuads(ITerrainBrush brush, bool state)
		{
			bool result = this.Root.SetQuads(brush, state);
			if (result == true)
			{
				CollectGeometry(true, ref _enabledVertexBuffer, ref _enabledIndexBuffer);
				CollectGeometry(false, ref _disabledVertexBuffer, ref _disabledIndexBuffer);
				//CollectEdgeGeometry(ref m_edgeVertexBuffer, ref m_edgeIndexBuffer);
			}

			return result;
		}
	}
}
