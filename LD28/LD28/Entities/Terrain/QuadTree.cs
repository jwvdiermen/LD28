using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// This class represents a quad tree, used by the <see cref="Terrain" /> entity.
	/// </summary>
	public class QuadTree : PhysicalEntityBase
	{
		private GraphicsDevice _graphics;
		private World _physicsWorld;

		private VertexBuffer _enabledVertexBuffer;
		private IndexBuffer _enabledIndexBuffer;

		private VertexBuffer _disabledVertexBuffer;
		private IndexBuffer _disabledIndexBuffer;

		/// <summary>
		/// Gets the root node.
		/// </summary>
		public QuadTreeNode Root { get; private set; }

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="maxDepth">The maximum depth.</param>
		/// <param name="size">The size.</param>
		/// <param name="name">The name.</param>
		public QuadTree(int maxDepth, Vector2 size, string name)
			: base(name)
		{
			Root = new QuadTreeNode(null, maxDepth, new BoundingBox(Vector3.Zero, new Vector3(size, 0.0f)), true);
		}

		/// <summary>
		/// This method is called when graphics resources need to be loaded.
		/// </summary>
		/// <param name="services">The service provider.</param>
		public override void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

			// Collect our geometry.
			CollectGeometry(true, ref _enabledVertexBuffer, ref _enabledIndexBuffer);
			CollectGeometry(false, ref _disabledVertexBuffer, ref _disabledIndexBuffer);
		}

		/// <summary>
		/// This method is called when graphics resources need to be unloaded.
		/// </summary>
		public override void UnloadContent()
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
		}

		protected override ISceneNode CreateSceneNode()
		{
			return World.Scene.CreateSceneNode(Name);
		}

		protected override void OnWorldChanged(IEntityWorld world)
		{
			DestroyPhysics();

			if (world != null)
			{
				_physicsWorld = ((DynamicEntityWorld)world).PhysicsWorld;
				InitializePhysics();
			}
			else
			{
				_physicsWorld = null;
			}
		}

		private void InitializePhysics()
		{
			var body = BodyFactory.CreateBody(_physicsWorld);
			body.BodyType = BodyType.Static;

			Root.UpdateFixtures(body);

			Body = body;
		}
		

		private void DestroyPhysics()
		{
			if (Body != null)
			{
				Body.Dispose();
				Body = null;
			}
		}

		protected override void DisposeUnmanaged()
		{
			UnloadContent();
			DestroyPhysics();
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

			Root.CollectGeometry(vertices, indices, enabled);

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

		/// <summary>
		/// This method draws the quad tree.
		/// </summary>
		/// <param name="enabled">True to render the enabled buffers or false to render the disabled buffers.</param>
		public void Draw(bool enabled)
		{
			if (enabled && _enabledVertexBuffer != null && _enabledIndexBuffer != null)
			{
				_graphics.SetVertexBuffer(_enabledVertexBuffer);
				_graphics.Indices = _enabledIndexBuffer;

				_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _enabledVertexBuffer.VertexCount, 0, _enabledIndexBuffer.IndexCount / 3);
			}
			else if (!enabled && _disabledVertexBuffer != null && _disabledIndexBuffer != null)
			{
				_graphics.SetVertexBuffer(_disabledVertexBuffer);
				_graphics.Indices = _disabledIndexBuffer;

				_graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _disabledVertexBuffer.VertexCount, 0, _disabledIndexBuffer.IndexCount / 3);
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
			var position = Position;
			var offset = new Vector2(position.X, position.Y);

			bool result = Root.SetQuads(brush, offset, state);
			if (result)
			{
				CollectGeometry(true, ref _enabledVertexBuffer, ref _enabledIndexBuffer);
				CollectGeometry(false, ref _disabledVertexBuffer, ref _disabledIndexBuffer);

				Root.UpdateFixtures(Body);
			}

			return result;
		}
	}
}
