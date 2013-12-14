using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This abstract class provides a base implementation of the <see cref="IEntity" /> interface.
	/// </summary>
	public abstract class EntityWithParentBase : EntityBase, IEntityWithParent
	{
		private IEntity _parent;
		private Vector3? _localPosition;
		private Quaternion? _localOrientation;

		public IEntity Parent
		{
			get { return _parent; }
			set
			{
				_parent = value;
				if (_parent != null)
				{
					if (_parent.World != null)
					{
						World = _parent.World;
					}
					if (_parent.SceneNode != null && SceneNode != null)
					{
						SceneNode.Parent = _parent.SceneNode;
					}
				}
			}
		}

		public virtual Vector3 LocalPosition
		{
			get { return SceneNode != null ? SceneNode.LocalPosition : _localPosition.GetValueOrDefault(Vector3.Zero); }
			set
			{
				if (SceneNode != null)
				{
					SceneNode.LocalPosition = value;
				}
				else
				{
					_localPosition = value;
				}
			}
		}

		public virtual Quaternion LocalOrientation
		{
			get { return SceneNode != null ? SceneNode.LocalOrientation : _localOrientation.GetValueOrDefault(Quaternion.Identity); }
			set
			{
				if (SceneNode != null)
				{
					SceneNode.LocalOrientation = value;
				}
				else
				{
					_localOrientation = value;
				}
			}
		}

		/// <summary>
		/// This constructor creates a new instance of an entity with a parent.
		/// </summary>
		protected EntityWithParentBase()
			: this(null)
		{
		}

		/// <summary>
		/// This constructor creates a new instance of an entity with a parent.
		/// </summary>
		/// <param name="name">The optional name.</param>
		protected EntityWithParentBase(string name)
			: base(name)
		{
		}

		protected override void OnSceneNodeChanged(ISceneNode sceneNode)
		{
			// Copy the position and orientation.
			if (_localPosition != null)
			{
				sceneNode.LocalPosition = (Vector3)_localPosition;
				_localPosition = null;
			}
			if (_localOrientation != null)
			{
				sceneNode.LocalOrientation = (Quaternion)_localOrientation;
				_localOrientation = null;
			}

			// Set the parent.
			if (sceneNode != null)
			{
				if (_parent != null && _parent.SceneNode != null)
				{
					sceneNode.Parent = _parent.SceneNode;
				}
				else
				{
					// Default the parent to the scene root.
					sceneNode.Parent = World.Scene.Root;
				}
			}
		}
	}
}