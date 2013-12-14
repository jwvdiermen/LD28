using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
		public Terrain(Vector2 terrainSize, float blockSize, int maxDepth, string name = "Terrain")
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

			_terrainSize = terrainSize;
			_blockSize = blockSize;
			_maxDepth = maxDepth;

			DebugEnabled = false;
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
			var gridWidth = (int)(_terrainSize.X / _blockSize);
			var gridHeight = (int)(_terrainSize.Y / _blockSize);

			_blockGrid = new QuadTree[gridWidth, gridHeight];
			for (var y = 0; y < gridHeight; ++y)
			{
				for (var x = 0; x < gridWidth; ++x)
				{
					var quadTree = new QuadTree(_maxDepth, new Vector2(_blockSize, _blockSize), Name + "_" + x + "x" + y)
					{
						Position = Position + new Vector3(x * _blockSize, y * _blockSize, 0.0f)
					};

					quadTree.LoadContent(services);

					_blockGrid[x, y] = quadTree;

					if (World != null)
					{
						quadTree.World = World;
					}
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

		protected override void OnWorldChanged(IEntityWorld world)
		{
			if (_blockGrid != null)
			{
				foreach (var quadTree in _blockGrid)
				{
					quadTree.World = world;
				}
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

			if (DebugEnabled)
			{
				// Draw the enabled quads.
				_debugEffect.DiffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
				foreach (var quadTree in _blockGrid)
				{
					_debugEffect.Projection = projectionMatrix;
					_debugEffect.View = viewMatrix;
					_debugEffect.World = quadTree.SceneNode.Transformation;
					foreach (var effectPass in _debugEffect.CurrentTechnique.Passes)
					{
						effectPass.Apply();
						quadTree.Draw(true);
					}
				}

				// Draw the disabled quads.
				_debugEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
				foreach (var quadTree in _blockGrid)
				{
					_debugEffect.Projection = projectionMatrix;
					_debugEffect.View = viewMatrix;
					_debugEffect.World = quadTree.SceneNode.Transformation;
					foreach (var effectPass in _debugEffect.CurrentTechnique.Passes)
					{
						effectPass.Apply();
						quadTree.Draw(false);
					}
				}
			}
		}

		/// <summary>
		/// Changes the state of the intersected quads.
		/// </summary>
		/// <param name="brush">The brush.</param>
		/// <param name="state">The state, either enabled or disabled.</param>
		/// <param name="suppressUpdate">If true, does not update if any quads changed state.</param>
		/// <returns>True if any quads were changed.</returns>
		public bool SetQuads(ITerrainBrush brush, bool state, bool suppressUpdate = false)
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

		/// <summary>
		/// 
		/// </summary>
		public void Refresh()
		{
			foreach (var quadTree in _blockGrid)
			{
				quadTree.Refresh();
			}
		}
	}
}
