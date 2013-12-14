using System;
using System.Collections.Generic;
using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Scene
{
	public sealed class SceneNode : DisposableObject, ISceneNode
	{
		#region Constructors

		public SceneNode(IScene scene, string name)
		{
			Name = name;
			Scene = scene;
		}

		#endregion

		#region Fields

		private readonly List<ISceneNode> _childAddQueue = new List<ISceneNode>();
		private readonly List<ISceneNode> _childList = new List<ISceneNode>();
		private readonly List<ISceneNode> _childRemoveQueue = new List<ISceneNode>();

		private readonly List<IMovable> _movableAddQueue = new List<IMovable>();
		private readonly List<IMovable> _movableList = new List<IMovable>();
		private readonly List<IMovable> _movableRemoveQueue = new List<IMovable>();

		private Vector3? _globalPosition;
		private Quaternion? _globalOrientation;
		private Vector3? _globalScale;
		private Matrix? _globalTransformation = null;

		private volatile bool _isUpdating;

		private Vector3? _localPosition = Vector3.Zero;
		private Quaternion? _localOrientation = Quaternion.Identity;
		private Vector3? _localScale = Vector3.One;
		private ISceneNode _parent;

		#endregion

		#region Properties

		public Matrix Transformation
		{
			get
			{
				if (_globalTransformation == null)
				{
					EnsureGlobalTransformation();
				}
				return (Matrix)_globalTransformation;
			}
			set
			{
				_globalTransformation = value;

				_globalPosition = null;
				_globalOrientation = null;
				_globalScale = null;

				_localPosition = null;
				_localOrientation = null;
				_localScale = null;

				NotifyChildsOfTransform();
			}
		}

		public Vector3 Position
		{
			get
			{
				EnsureGlobalTransformation();
				return (Vector3)_globalPosition;
			}
			set
			{
				_globalPosition = value;
				_globalOrientation = _globalOrientation == null ?
					(_parent != null ? _parent.Orientation * _localOrientation.GetValueOrDefault(Quaternion.Identity) : _localOrientation.GetValueOrDefault(Quaternion.Identity)) :
					_globalOrientation;

				_localPosition = null; // Reset local position.
				_localOrientation = null; // Reset local rotation.
				_localScale = null; // Reset local scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public Quaternion Orientation
		{
			get
			{
				EnsureGlobalTransformation();
				return (Quaternion)_globalOrientation;
			}
			set
			{
				_globalOrientation = value;
				_globalPosition = _globalPosition == null ?
					(_parent != null ? _localPosition.GetValueOrDefault(Vector3.Zero) + _parent.Position : _localPosition.GetValueOrDefault(Vector3.Zero)) :
					_globalPosition;

				_localPosition = null; // Reset local position.
				_localOrientation = null; // Reset local rotation.
				_localScale = null; // Reset local scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public Vector3 Scale
		{
			get
			{
				EnsureGlobalTransformation();
				return (Vector3)_globalScale;
			}
			set
			{
				_globalScale = value;

				_localPosition = null; // Reset local position.
				_localOrientation = null; // Reset local rotation.
				_localScale = null; // Reset local scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public Vector3 LocalPosition
		{
			get
			{
				EnsureLocalTransformation();
				return (Vector3)_localPosition;
			}
			set
			{
				_localPosition = value;
				_localOrientation = _localOrientation == null ?
					(_parent != null ? _globalOrientation.GetValueOrDefault(Quaternion.Identity) * _parent.Orientation : _globalOrientation.GetValueOrDefault(Quaternion.Identity)) :
					_localOrientation;

				_globalPosition = null; // Reset global position.
				_globalOrientation = null; // Reset global rotation.
				_globalScale = null; // Reset global scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public Quaternion LocalOrientation
		{
			get
			{
				EnsureLocalTransformation();
				return (Quaternion)_localOrientation;
			}
			set
			{
				_localOrientation = value;
				_localPosition = _localPosition == null ?
					(_parent != null ? _globalPosition.GetValueOrDefault(Vector3.Zero) - _parent.Position : _globalPosition.GetValueOrDefault(Vector3.Zero)) :
					_localPosition;

				_globalPosition = null; // Reset global position.
				_globalOrientation = null; // Reset global rotation.
				_globalScale = null; // Reset global scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public Vector3 LocalScale
		{
			get
			{
				EnsureLocalTransformation();
				return (Vector3)_localScale;
			}
			set
			{
				_localScale = value;

				_globalPosition = null; // Reset global position.
				_globalOrientation = null; // Reset global rotation.
				_globalScale = null; // Reset global scale.

				_globalTransformation = null; // Invalidate the transformation.

				NotifyChildsOfTransform();
			}
		}

		public string Name { get; private set; }

		public IScene Scene { get; private set; }

		public ISceneNode Parent
		{
			get { return _parent; }
			set
			{
				// When the parent changes, the scene node should remain on the old position.
				// To realize this, we need that the global transformations are known.
				//TransformLocalToGlobal();

				if (_parent == null && value != null)
				{
					_parent = value;
					_parent.AddChild(this);
				}
				else if (_parent != value && value != null)
				{
					var previousParent = _parent;
					_parent = null;

					previousParent.RemoveChild(this);

					_parent = value;
					_parent.AddChild(this);
				}
				else if (_parent != null && value == null)
				{
					var previousParent = _parent;
					_parent = null;

					previousParent.RemoveChild(this);
				}
			}
		}

		public IEnumerable<ISceneNode> Childs
		{
			get
			{
				IEnumerable<ISceneNode> result;
				lock (_childList)
				{
					result = _childList.ToArray();
				}

				return result;
			}
		}

		public IEnumerable<IMovable> Movables
		{
			get
			{
				IEnumerable<IMovable> result;
				lock (_movableList)
				{
					result = _movableList.ToArray();
				}

				return result;
			}
		}

		#endregion

		#region Methods

		internal void Update(GameTime gameTime, ICamera camera)
		{
			_isUpdating = true;

			// Update the child list.
			lock (_childList)
			{
				if (_childList.Count > 0)
				{
					foreach (SceneNode child in _childList)
					{
						child.Update(gameTime, camera);
					}
				}

				// If any childs where added or removed during the update, apply those changes now.
				if (_childRemoveQueue.Count > 0)
				{
					lock (_childRemoveQueue)
					{
						foreach (var child in _childRemoveQueue)
						{
							_childList.Remove(child);
						}
						_childRemoveQueue.Clear();
					}
				}

				if (_childAddQueue.Count > 0)
				{
					lock (_childAddQueue)
					{
						foreach (var child in _childAddQueue)
						{
							_childList.Add(child);
						}
						_childAddQueue.Clear();
					}
				}
			}

			// Update the movable list.
			lock (_movableList)
			{
				var scene = Scene;

				if (_movableList.Count > 0)
				{
					foreach (var movable in _movableList)
					{
						if (movable is IUpdatable)
						{
							((IUpdatable)movable).Update(gameTime, camera);
						}
					}
				}

				// If any movables where added or removed during the update, apply those changes now.
				if (_movableRemoveQueue.Count > 0)
				{
					lock (_movableRemoveQueue)
					{
						foreach (var movable in _movableRemoveQueue)
						{
							_movableList.Remove(movable);
							scene.RemoveMovable(movable);
						}
						_movableRemoveQueue.Clear();
					}
				}

				if (_movableAddQueue.Count > 0)
				{
					lock (_movableAddQueue)
					{
						foreach (var movable in _movableAddQueue)
						{
							_movableList.Add(movable);
							scene.AddMovable(movable);
						}
						_movableAddQueue.Clear();
					}
				}
			}

			_isUpdating = false;
		}

		private void NotifyChildsOfTransform()
		{
			lock (_childList)
			{
				foreach (var child in _childList)
				{
					child.ParentTransformationChanged();
				}
			}
		}

		protected override void DisposeUnmanaged()
		{
			Remove();

			lock (_movableList)
			{
				var scene = Scene;

				for (var i = _movableList.Count - 1; i >= 0; --i)
				{
					var movable = _movableList[i];
					if (movable is IDisposable)
					{
						((IDisposable)movable).Dispose();
					}

					scene.RemoveMovable(movable);
				}

				_movableList.Clear();
			}

			lock (_movableAddQueue)
			{
				_movableAddQueue.Clear();
			}
			lock (_movableRemoveQueue)
			{
				_movableRemoveQueue.Clear();
			}

			lock (_childList)
			{
				for (var i = _childList.Count - 1; i >= 0; --i)
				{
					_childList[i].Remove();
				}

				_childList.Clear();
			}

			lock (_childAddQueue)
			{
				_childAddQueue.Clear();
			}
			lock (_childRemoveQueue)
			{
				_childRemoveQueue.Clear();
			}
		}

		public ISceneNode CreateChild()
		{
			return CreateChild(null);
		}

		public ISceneNode CreateChild(string name)
		{
			var child = new SceneNode(Scene, name);
			AddChild(child);

			return child;
		}

		public void AddChild(ISceneNode child)
		{
			if (_childAddQueue.Contains(child) == false && _childList.Contains(child) == false)
			{
				if (_isUpdating)
				{
					lock (_childAddQueue)
					{
						if (_childAddQueue.Contains(child) == false && _childList.Contains(child) == false)
						{
							_childAddQueue.Add(child);
							if (child.Parent != this)
							{
								child.Parent = this;
							}
						}
					}
				}
				else
				{
					lock (_childList)
					{
						if (_childAddQueue.Contains(child) == false && _childList.Contains(child) == false)
						{
							_childList.Add(child);
							if (child.Parent != this)
							{
								child.Parent = this;
							}
						}
					}
				}
			}
		}

		public void RemoveChild(ISceneNode child)
		{
			if (_childAddQueue.Contains(child))
			{
				lock (_childAddQueue)
				{
					if (_childAddQueue.Contains(child))
					{
						_childAddQueue.Remove(child);
						if (child.Parent == this)
						{
							child.Parent = null;
						}
					}
				}
			}
			else if (_childRemoveQueue.Contains(child))
			{
				// Do nothing.
			}
			else if (_childList.Contains(child))
			{
				if (_isUpdating)
				{
					lock (_childRemoveQueue)
					{
						if (_childRemoveQueue.Contains(child) == false)
						{
							_childRemoveQueue.Add(child);
							if (child.Parent == this)
							{
								child.Parent = null;
							}
						}
					}
				}
				else
				{
					lock (_childList)
					{
						_childList.Remove(child);
						if (child.Parent == this)
						{
							child.Parent = null;
						}
					}
				}
			}
		}

		public void Remove()
		{
			Parent = null;
		}

		public void Attach(IMovable movable)
		{
			if (_movableAddQueue.Contains(movable) == false && _movableList.Contains(movable) == false)
			{
				if (_isUpdating)
				{
					lock (_movableAddQueue)
					{
						if (_movableAddQueue.Contains(movable) == false && _movableList.Contains(movable) == false)
						{
							_movableAddQueue.Add(movable);
							if (movable.SceneNode != this)
							{
								movable.AttachTo(this);
							}
						}
					}
				}
				else
				{
					lock (_movableList)
					{
						if (_movableAddQueue.Contains(movable) == false && _movableList.Contains(movable) == false)
						{
							_movableList.Add(movable);
							if (movable.SceneNode != this)
							{
								movable.AttachTo(this);
							}
							Scene.AddMovable(movable);
						}
					}
				}
			}
		}

		public void Detach(IMovable movable)
		{
			if (_movableAddQueue.Contains(movable))
			{
				lock (_movableAddQueue)
				{
					if (_movableAddQueue.Contains(movable))
					{
						_movableAddQueue.Remove(movable);
						if (movable.SceneNode == this)
						{
							movable.Detach();
						}
					}
				}
			}
			else if (_movableRemoveQueue.Contains(movable))
			{
				// Do nothing.
			}
			else if (_movableList.Contains(movable))
			{
				if (_isUpdating)
				{
					lock (_movableRemoveQueue)
					{
						if (_movableRemoveQueue.Contains(movable) == false)
						{
							_movableRemoveQueue.Add(movable);
							if (movable.SceneNode == this)
							{
								movable.Detach();
							}
						}
					}
				}
				else
				{
					lock (_movableList)
					{
						_movableList.Remove(movable);
						if (movable.SceneNode == this)
						{
							movable.Detach();
						}
						Scene.RemoveMovable(movable);
					}
				}
			}
		}

		public T FindMovable<T>()
			where T : IMovable
		{
			IMovable result = null;

			lock (_movableList)
			{
				foreach (var movable in _movableList)
				{
					if (movable is T)
					{
						result = movable;
						break;
					}
				}
			}

			if (result == null)
			{
				lock (_movableAddQueue)
				{
					foreach (var movable in _movableAddQueue)
					{
						if (movable is T)
						{
							result = movable;
							break;
						}
					}
				}
			}

			return (T)result;
		}

		public void ParentTransformationChanged()
		{
			// When our parent moves, make sure we only know our local transformation, so we can
			// calculate our global transformation again when it's needed.

			// Ensure the local variables.
			EnsureLocalTransformation();

			// Clear the globals.
			_globalTransformation = null;

			_globalPosition = null;
			_globalOrientation = null;
			_globalScale = null;

			// Also notify our childs.
			NotifyChildsOfTransform();
		}

		public void TransformLocalToGlobal()
		{
			EnsureGlobalTransformation();

			_localPosition = null; // Reset local position.
			_localOrientation = null; // Reset local rotation.
			_localScale = null; // Reset local scale.
		}

		public void Move(Vector3 distance)
		{
			//if (_globalPosition != null)
			//{
			//    var position = _globalPosition.Value;
			//    position += distance;
			//    _globalPosition = position;
			//}
			//else if (_localPosition != null)
			//{
			//    var position = _localPosition.Value;
			//    position += distance;
			//    _localPosition = position;
			//}

			Position += distance;
			NotifyChildsOfTransform();
		}

		private void EnsureGlobalTransformation()
		{
			if (Parent != null)
			{
				if (_globalPosition == null)
				{
					_globalPosition = Parent.Position + _localPosition.GetValueOrDefault(Vector3.Zero);
				}
				
				if (_globalOrientation == null)
				{
					_globalOrientation = Parent.Orientation * _localOrientation.GetValueOrDefault(Quaternion.Identity);
				}

				if (_globalScale == null)
				{
					_globalScale = Parent.Scale * _localScale.GetValueOrDefault(Vector3.One);
				}
			}
			else
			{
				if (_globalPosition == null)
				{
					_globalPosition = _localPosition.GetValueOrDefault(Vector3.Zero);
				}

				if (_globalOrientation == null)
				{
					_globalOrientation = _localOrientation.GetValueOrDefault(Quaternion.Identity);
				}

				if (_globalScale == null)
				{
					_globalScale = _localScale.GetValueOrDefault(Vector3.One);
				}
			}

			if (_globalTransformation == null)
			{
				_globalTransformation = Matrix.CreateFromQuaternion(_globalOrientation.GetValueOrDefault(Quaternion.Identity)) *
				                        Matrix.CreateTranslation(_globalPosition.GetValueOrDefault(Vector3.Zero)) *
				                        Matrix.CreateScale(_globalScale.GetValueOrDefault(Vector3.Zero));
			}
		}

		private void EnsureLocalTransformation()
		{
			if (Parent != null)
			{
				if (_localPosition == null || _localOrientation == null)
				{
					var ma = Matrix.CreateFromQuaternion((Quaternion)_globalOrientation) * Matrix.CreateTranslation((Vector3)_globalPosition);
					var mb = Matrix.Invert(Parent.Transformation);
					var mab = ma * mb;

					_localPosition = mab.Translation;
					_localOrientation = Quaternion.CreateFromRotationMatrix(mab);
				}

				if (_localScale == null)
				{
					_localScale = _globalScale.GetValueOrDefault(Vector3.One) / Parent.Scale;
				}
			}
			else
			{
				if (_localPosition == null)
				{
					_localPosition = _globalPosition.GetValueOrDefault(Vector3.Zero);
				}

				if (_localOrientation == null)
				{
					_localOrientation = _globalOrientation.GetValueOrDefault(Quaternion.Identity);
				}

				if (_localScale == null)
				{
					_localScale = _globalScale.GetValueOrDefault(Vector3.One);
				}
			}
		}

		#endregion
	}
}