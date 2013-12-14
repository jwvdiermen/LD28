using System;
using System.Collections.Generic;
using System.Linq;
using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This abstract class provides a base implementation of the <see cref="IEntity" /> interface.
	/// </summary>
	public abstract class EntityBase : DisposableObject, IEntity
	{
		#region Constructors

		/// <summary>
		/// This constructor creates a new instance of an entity.
		/// </summary>
		protected EntityBase()
		{
			Controllers = new ReadOnlyList<IEntityController>(_controllerList);
			IsActive = true;
		}

		/// <summary>
		/// This constructor creates a new instance of an entity.
		/// </summary>
		/// <param name="name">The optional name.</param>
		protected EntityBase(string name)
			: this()
		{
			Name = name;
		}

		#endregion

		#region Fields

		private bool _isUpdating;

		private readonly List<IEntityController> _controllerAddQueue = new List<IEntityController>();
		private readonly List<IEntityController> _controllerList = new List<IEntityController>();
		private readonly List<IEntityController> _controllerRemoveQueue = new List<IEntityController>();

		private IEntityWorld _entityWorld;

		private Vector3? _position;
		private Quaternion? _orientation;
		private ISceneNode _sceneNode;

		#endregion

		#region Methods

		/// <summary>
		/// This method is called when a new world has been set for the entity.
		/// </summary>
		/// <param name="world">The new world. Can be null.</param>
		protected virtual void OnWorldChanged(IEntityWorld world)
		{
		}

		/// <summary>
		/// This method is called when a new scene node has been set for the entity.
		/// </summary>
		/// <param name="sceneNode">The new scene node. Can be null.</param>
		protected virtual void OnSceneNodeChanged(ISceneNode sceneNode)
		{
		}

		#endregion

		#region Properties

		public string Name { get; private set; }

		public IEntityWorld World
		{
			get { return _entityWorld; }
			set
			{
				if (_entityWorld == null && value != null)
				{
					_entityWorld = value;
					SceneNode = CreateSceneNode();
					_entityWorld.Add(this);

					OnWorldChanged(_entityWorld);
				}
				else if (_entityWorld != value && value != null)
				{
					var previousWorld = _entityWorld;
					_entityWorld = null;

					previousWorld.Remove(this);

					_entityWorld = value;
					SceneNode = CreateSceneNode();
					_entityWorld.Add(this);

					OnWorldChanged(_entityWorld);
				}
				else if (_entityWorld != null && value == null)
				{
					var previousWorld = _entityWorld;
					_entityWorld = null;

					SceneNode = null;

					previousWorld.Remove(this);

					OnWorldChanged(null);
				}
			}
		}

		public ISceneNode SceneNode
		{
			get { return _sceneNode; }
			private set
			{
				var oldSceneNode = _sceneNode;
				_sceneNode = value;
				if (_sceneNode != null)
				{
					if (_position != null)
					{
						_sceneNode.Position = (Vector3)_position;
						_position = null;
					}
					if (_orientation != null)
					{
						_sceneNode.Orientation = (Quaternion)_orientation;
						_orientation = null;
					}

					if (oldSceneNode != null)
					{
						_sceneNode.Transformation = oldSceneNode.Transformation;
					}
				}

				OnSceneNodeChanged(_sceneNode);
			}
		}

		public virtual Vector3 Position
		{
			get { return _sceneNode != null ? _sceneNode.Position : _position.GetValueOrDefault(Vector3.Zero); }
			set
			{
				if (_sceneNode != null)
				{
					_sceneNode.Position = value;
				}
				else
				{
					_position = value;
				}
			}
		}

		public virtual Quaternion Orientation
		{
			get { return _sceneNode != null ? _sceneNode.Orientation : _orientation.GetValueOrDefault(Quaternion.Identity); }
			set
			{
				if (_sceneNode != null)
				{
					_sceneNode.Orientation = value;
				}
				else
				{
					_orientation = value;
				}
			}
		}

		public ReadOnlyList<IEntityController> Controllers { get; private set; }

		public virtual bool IsActive { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Creates the scene node used by the entity.
		/// </summary>
		/// <returns>The scene node. Can be null for entities that don't use scene nodes.</returns>
		protected virtual ISceneNode CreateSceneNode()
		{
			return null;
		}

		public virtual void LoadContent(IServiceProvider services)
		{
		}

		public virtual void UnloadContent()
		{
			if (_sceneNode != null)
			{
				_sceneNode.Dispose();
				_sceneNode = null;
			}
		}

		public void Attach(IEntityController controller)
		{
			if (_controllerAddQueue.Contains(controller) == false && _controllerList.Contains(controller) == false)
			{
				if (_isUpdating)
				{
					lock (_controllerAddQueue)
					{
						if (_controllerAddQueue.Contains(controller) == false && _controllerList.Contains(controller) == false)
						{
							_controllerAddQueue.Add(controller);
							if (controller.Entity != this)
							{
								controller.AttachTo(this);
							}
						}
					}
				}
				else
				{
					lock (_controllerList)
					{
						if (_controllerAddQueue.Contains(controller) == false && _controllerList.Contains(controller) == false)
						{
							_controllerList.Add(controller);
							if (controller.Entity != this)
							{
								controller.AttachTo(this);
							}
						}
					}
				}
			}
		}

		public void Detach(IEntityController controller)
		{
			if (_controllerAddQueue.Contains(controller))
			{
				lock (_controllerAddQueue)
				{
					if (_controllerAddQueue.Contains(controller))
					{
						_controllerAddQueue.Remove(controller);
						if (controller.Entity == this)
						{
							controller.Detach();
						}
					}
				}
			}
			else if (_controllerRemoveQueue.Contains(controller))
			{
				// Do nothing.
			}
			else if (_controllerList.Contains(controller))
			{
				if (_isUpdating)
				{
					lock (_controllerRemoveQueue)
					{
						if (_controllerRemoveQueue.Contains(controller) == false)
						{
							_controllerRemoveQueue.Add(controller);
							if (controller.Entity == this)
							{
								controller.Detach();
							}
						}
					}
				}
				else
				{
					lock (_controllerList)
					{
						_controllerList.Remove(controller);
						if (controller.Entity == this)
						{
							controller.Detach();
						}
					}
				}
			}
		}

		public T Controller<T>() where T : IEntityController
		{
			return (T)_controllerList.FirstOrDefault(x => x is T);
		}

		public virtual void Update(GameTime gameTime, ICamera camera)
		{
			_isUpdating = true;

			// Update the controller list.
			lock (_controllerList)
			{
				if (_controllerList.Count > 0)
				{
					foreach (var controller in _controllerList)
					{
						controller.Update(gameTime, camera);
					}
				}

				// If any controllers where added or removed during the update, apply those changes now.
				if (_controllerRemoveQueue.Count > 0)
				{
					lock (_controllerRemoveQueue)
					{
						foreach (var controller in _controllerRemoveQueue)
						{
							_controllerList.Remove(controller);
						}
						_controllerRemoveQueue.Clear();
					}
				}

				if (_controllerAddQueue.Count > 0)
				{
					lock (_controllerAddQueue)
					{
						foreach (var controller in _controllerAddQueue)
						{
							_controllerList.Add(controller);
						}
						_controllerAddQueue.Clear();
					}
				}
			}

			_isUpdating = false;
		}

		public virtual void Destroy()
		{
			World = null;
			Dispose();
		}

		#endregion
	}
}