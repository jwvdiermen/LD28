using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using LD28.Entity;
using LD28.Scene;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// This class represents a grid of tiles.
	/// </summary>
	public class Terrain : EntityBase
	{
		private Vector2 _terrainSize;
		private readonly float _blockSize;
		private readonly int _maxDepth;

		private QuadTree[,] _blockGrid;

		private GraphicsDevice _graphics;

		private readonly RasterizerState _debugRasterizerState;
		private BasicEffect _debugEffect;

		/// <summary>
		/// Gets the terrain size.
		/// </summary>
		public Vector2 TerrainSize
		{
			get { return _terrainSize; }
		}

		/// <summary>
		/// Gets the block size.
		/// </summary>
		public float BlockSize
		{
			get { return _blockSize; }
		}

		/// <summary>
		/// Gets or sets if debugging is enabled.
		/// </summary>
		public bool DebugEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="terrainSize">The size of the terrain.</param>
		/// <param name="blockSize">The size of a single block. The terrain size should be dividable by it.</param>
		/// <param name="maxDepth">The maximum division depth for the quad trees.</param>
		/// <param name="name">The name of the entity.</param>
		public Terrain(Vector2 terrainSize, float blockSize, int maxDepth, string name = null)
			: base(name)
		{
			if (_terrainSize.X % blockSize > 0.0f)
			{
				throw new Exception("The width of the terrain size is not dividable by the block size.");
			}
			if (_terrainSize.Y % blockSize > 0.0f)
			{
				throw new Exception("The height of the terrain size is not dividable by the block size.");
			}

			//var blockDepth = blockSize;
			//for (int i = 0; i < maxDepth; ++i)
			//{
			//    blockDepth /= 2.0f;
			//    if (blockDepth % 2 > 0)
			//    {
			//        throw new Exception("The given block size does not support the maximum depth of " + maxDepth + ".");
			//    }
			//}

			_terrainSize = terrainSize;
			_blockSize = blockSize;
			_maxDepth = maxDepth;

			this.DebugEnabled = false;
			_debugRasterizerState = new RasterizerState
			{
				CullMode = CullMode.CullCounterClockwiseFace,
				FillMode = FillMode.WireFrame
			};
		}

		protected override ISceneNode CreateSceneNode()
		{
			return World.Scene.Root.CreateChild(Name);
		}

		public override void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
			
			// Load our content.			
			_debugEffect = new BasicEffect(_graphics)
			{
				DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f)
			};

			// Create the quad trees.
			int gridWidth = (int)(_terrainSize.X / _blockSize);
			int gridHeight = (int)(_terrainSize.Y / _blockSize);

			_blockGrid = new QuadTree[gridWidth, gridHeight];
			for (int y = 0; y < gridHeight; ++y)
			{
				for (int x = 0; x < gridWidth; ++x)
				{
					var quadTree = new QuadTree(_maxDepth, new BoundingBox(
						new Vector3(x * _blockSize, y * _blockSize, 0.0f),
						new Vector3((x + 1) * _blockSize, (y + 1) * _blockSize, 0.0f)));

					quadTree.LoadContent(services);

					_blockGrid[x, y] = quadTree;
				}
			}
		}

		public override void UnloadContent()
		{
			// Dispose our content.
			_debugEffect.Dispose();

			// Dispose the quad trees.
			if (_blockGrid != null)
			{
				foreach (var quadTree in _blockGrid)
				{
					quadTree.Dispose();
				}
				_blockGrid = null;
			}
		}

		public override void Update(GameTime gameTime, ICamera camera)
		{
			base.Update(gameTime, camera);

			// Update the quad trees.
			foreach (var quadTree in _blockGrid)
			{
				quadTree.Update(gameTime);
			}
		}

		/// <summary>
		/// This method draws the terrain.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		public void Draw(GameTime gameTime, ICamera camera)
		{			
			_graphics.RasterizerState = _debugRasterizerState;

			// Set the debug effect's parameters.
			Matrix projectionMatrix, viewMatrix;
			camera.GetMatrices(out projectionMatrix, out viewMatrix);

			_debugEffect.Projection = projectionMatrix;
			_debugEffect.View = viewMatrix;
			_debugEffect.World = SceneNode.Transformation;

			// Draw the enabled quads.
			_debugEffect.DiffuseColor = new Vector3(0.0f, 1.0f, 0.0f);
			foreach (var effectPass in _debugEffect.CurrentTechnique.Passes)
			{
				effectPass.Apply();
				foreach (var quadTree in _blockGrid)
				{
					quadTree.Draw(true);
				}
			}

			// Draw the disabled quads.
			_debugEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
			foreach (var effectPass in _debugEffect.CurrentTechnique.Passes)
			{
				effectPass.Apply();
				foreach (var quadTree in _blockGrid)
				{
					quadTree.Draw(false);
				}
			}

			//// Draw the edge geometry.
			//_debugEffect.DiffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
			//foreach (var effectPass in _debugEffect.CurrentTechnique.Passes)
			//{
			//    effectPass.Apply();
			//    foreach (var quadTree in _blockGrid)
			//    {
			//        quadTree.DrawEdge();
			//    }
			//}
		}

		/// <summary>
		/// Changes the state of the intersected quads.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="state">The state, either enabled or disabled.</param>
		/// <returns>True if any quads were changed.</returns>
		public bool SetQuads(ITerrainBrush brush, bool state)
		{
			bool changed = false;
			foreach (var quadTree in _blockGrid)
			{
				if (quadTree.SetQuads(brush, state) == true)
				{
					changed = true;
				}
			}

			return changed;
		}
	}
}
