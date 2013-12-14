using System;
using System.Collections.Generic;
using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This class implements the <see cref="IEntityWorld" /> interface and provides a static entity world
	/// without physics.
	/// </summary>
	public class StaticEntityWorld : DisposableObject, IEntityWorld
	{
		#region Constructors

		/// <summary>
		/// This constructor creates a new static entity world using a default scene (<see cref="DefaultScene" />).
		/// </summary>
		/// <param name="services">The service provider.</param>
		public StaticEntityWorld(IServiceProvider services)
			: this(services, new DefaultScene())
		{
		}

		/// <summary>
		/// This constructor creates a new static entity world using the given scene.
		/// </summary>
		/// <param name="services">The service provider.</param>
		/// <param name="scene">The scene to use.</param>
		public StaticEntityWorld(IServiceProvider services, IScene scene)
		{
			_services = services;

			_scene = scene;
			_scene.MovableAdded += Scene_MovableAdded;
			_scene.MovableRemoved += Scene_MovableRemoved;

			foreach (IRenderable renderable in _scene.Collect(typeof(IRenderable)))
			{
				renderable.LoadContent(_services);
			}
		}

		#endregion

		#region Fields

		private readonly IServiceProvider _services;

		private IScene _scene;

		private volatile bool _isUpdating = false;

		private List<IEntity> _entityAddQueue = new List<IEntity>();
		private List<IEntity> _entityRemoveQueue = new List<IEntity>();
		private List<IEntity> _entityList = new List<IEntity>();

		#endregion

		#region Methods

		private void Scene_MovableAdded(IScene scene, IMovable movable)
		{
			var renderable = movable as IRenderable;
			if (renderable != null)
			{
				renderable.LoadContent(_services);
			}
		}

		private void Scene_MovableRemoved(IScene scene, IMovable movable)
		{
			var renderable = movable as IRenderable;
			if (renderable != null)
			{
				renderable.UnloadContent();
			}
		}

		protected override void DisposeManaged()
		{
			if (_scene != null)
			{
				_scene.Dispose();
				_scene = null;
			}

			if (_entityList.Count > 0)
			{
				lock (_entityList)
				{
					if (_entityList.Count > 0)
					{
						for (int i = _entityList.Count - 1; i >= 0; --i)
						{
							var entity = _entityList[i];

							var disposable = entity as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
						_entityList.Clear();
					}
				}
			}
		}

		#endregion

		#region IEntityWorld Members

		#region Properties

		public IScene Scene
		{
			get { return _scene; }
		}

		public IEnumerable<IEntity> Entities
		{
			get { return _entityList; }
		}

		#endregion

		#region Methods

		public void Add(IEntity entity)
		{
			if (_entityAddQueue.Contains(entity) == false && _entityList.Contains(entity) == false)
			{
				if (_isUpdating == true)
				{
					lock (_entityAddQueue)
					{
						if (_entityAddQueue.Contains(entity) == false && _entityList.Contains(entity) == false)
						{
							_entityAddQueue.Add(entity);
							if (entity.World != this)
							{
								entity.World = this;
							}
						}
					}
				}
				else
				{
					lock (_entityList)
					{
						if (_entityAddQueue.Contains(entity) == false && _entityList.Contains(entity) == false)
						{
							_entityList.Add(entity);
							if (entity.World != this)
							{
								entity.World = this;
							}
							entity.LoadContent(_services);
						}
					}
				}
			}
		}

		public void Remove(IEntity entity)
		{
			if (_entityAddQueue.Contains(entity) == true)
			{
				lock (_entityAddQueue)
				{
					if (_entityAddQueue.Contains(entity) == true)
					{
						_entityAddQueue.Remove(entity);
						if (entity.World == this)
						{
							entity.World = null;
						}
					}
				}
			}
			else if (_entityRemoveQueue.Contains(entity) == true)
			{
				// Do nothing.
			}
			else if (_entityList.Contains(entity) == true)
			{
				if (_isUpdating == true)
				{
					lock (_entityRemoveQueue)
					{
						if (_entityRemoveQueue.Contains(entity) == false)
						{
							_entityRemoveQueue.Add(entity);
							if (entity.World == this)
							{
								entity.World = null;
							}
						}
					}
				}
				else
				{
					lock (_entityList)
					{
						_entityList.Remove(entity);
						entity.UnloadContent();
						if (entity.World == this)
						{
							entity.World = null;
						}
					}
				}
			}
		}

		public void Update(GameTime time, ICamera camera)
		{
			_isUpdating = true;

			// Update the entities.
			lock (_entityList)
			{
				ApplyEntityQueues();

				if (_entityList.Count > 0)
				{
					foreach (var entity in _entityList)
					{
						entity.Update(time, camera);
					}
				}

				ApplyEntityQueues();
			}

			// Update the scene.
			_scene.Update(time, camera);

			_isUpdating = false;
		}

		private void ApplyEntityQueues()
		{
			// If any entities where added or removed during the update, apply those changes now.
			if (_entityRemoveQueue.Count > 0)
			{
				lock (_entityRemoveQueue)
				{
					foreach (var entity in _entityRemoveQueue)
					{
						_entityList.Remove(entity);
						entity.UnloadContent();
					}
					_entityRemoveQueue.Clear();
				}
			}

			if (_entityAddQueue.Count > 0)
			{
				lock (_entityAddQueue)
				{
					foreach (var entity in _entityAddQueue)
					{
						_entityList.Add(entity);
						entity.LoadContent(_services);
					}
					_entityAddQueue.Clear();
				}
			}
		}

		#endregion

		#endregion
	}
}
