using System;
using System.Collections.Generic;
using System.Linq;
using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Scene
{
	/// <summary>
	/// This layer automatically collects any renderable from the scene that implements the given type.
	/// </summary>
	public class DefaultLayer : DisposableObject, ILayer
	{
		#region Constructors

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="depth">The depth.</param>
		/// <param name="matcher">The matcher.</param>
		public DefaultLayer(float depth, Func<IRenderable, bool> matcher)
		{
			Depth = depth;
			Matcher = matcher;

			_readOnlyRenderableList = new ReadOnlyList<IRenderable>(_renderableList);
		}

		#endregion

		#region Fields

		private readonly ReadOnlyList<IRenderable> _readOnlyRenderableList;
		private readonly List<IRenderable> _renderableList = new List<IRenderable>();
		private IScene _scene;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the function used for matching renderables.
		/// </summary>
		public Func<IRenderable, bool> Matcher { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Creates a matcher that looks if the renderable is an instance of any of the given type.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <returns>The created matcher.</returns>
		public static Func<IRenderable, bool> MatchesAnyType(params Type[] types)
		{
			return renderable => types.Any(e => e.IsInstanceOfType(renderable));
		}

		/// <summary>
		/// Creates a matcher that looks if the renderable is an instance of any of the given type.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <returns>The created matcher.</returns>
		public static Func<IRenderable, bool> MatchesAllTypes(params Type[] types)
		{
			return renderable => types.All(e => e.IsInstanceOfType(renderable));
		}

		/// <summary>
		/// Creates a matcher that only accepts renderables of which the name starts with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The created matcher.</returns>
		public static Func<IRenderable, bool> MatchesStartOfName(string value)
		{
			return renderable => 
			       {
				       var namable = renderable as INamable;
					   return namable != null && !String.IsNullOrEmpty(namable.Name) && namable.Name.StartsWith(value);
			       };
		}

		/// <summary>
		/// This method is called when a new scene has been set for the layer.
		/// </summary>
		/// <param name="scene">The new scene. Can be null.</param>
		protected virtual void OnSceneChanged(IScene scene)
		{
			_renderableList.Clear();

			if (scene != null)
			{
				foreach (IRenderable renderable in scene.Collect(typeof (IRenderable)))
				{
					if (MatchRenderable(renderable))
					{
						_renderableList.Add(renderable);
					}
				}
			}
		}

		/// <summary>
		/// This method is called when a movable is added to the scene.
		/// </summary>
		/// <param name="scene">The scene</param>
		/// <param name="movable">The movable.</param>
		protected virtual void OnMovableAdded(IScene scene, IMovable movable)
		{
			var renderable = movable as IRenderable;
			if (renderable != null && MatchRenderable(renderable))
			{
				_renderableList.Add((IRenderable)movable);
			}
		}

		/// <summary>
		/// This method checks if the given renderable should be handled by the layer.
		/// </summary>
		/// <param name="renderable">The renderable.</param>
		/// <returns>True if the renderable should be handled by the layer.</returns>
		protected virtual bool MatchRenderable(IRenderable renderable)
		{
			return Matcher != null ? Matcher(renderable) : true;
		}

		/// <summary>
		/// Manuall adds a renderable to the layer, even if it doesn't match.
		/// </summary>
		/// <param name="renderable">The renderable.</param>
		public void AddRenderable(IRenderable renderable)
		{
			if (_renderableList.Contains(renderable) == false)
			{
				_renderableList.Add(renderable);
			}
		}

		/// <summary>
		/// Removes a renderable from the layer.
		/// </summary>
		/// <param name="renderable">The renderable.</param>
		public void RemoveRenderable(IRenderable renderable)
		{
			_renderableList.Remove(renderable);
		}

		/// <summary>
		/// This methos is called when a movable is removed from the scene.
		/// </summary>
		/// <param name="scene">The scene.</param>
		/// <param name="movable">The movable.</param>
		protected virtual void OnMovableRemoved(IScene scene, IMovable movable)
		{
			if (movable is IRenderable)
			{
				_renderableList.Remove((IRenderable)movable);
			}
		}

		protected override void DisposeUnmanaged()
		{
			Scene = null;
		}

		#endregion

		#region ILayer Members

		public IScene Scene
		{
			get { return _scene; }
			set
			{
				if (_scene == null && value != null)
				{
					_scene = value;
					_scene.AddLayer(this);

					OnSceneChanged(_scene);

					_scene.MovableAdded += OnMovableAdded;
					_scene.MovableRemoved += OnMovableRemoved;
				}
				else if (_scene != value && value != null)
				{
					IScene previousScene = _scene;
					_scene = null;

					previousScene.RemoveLayer(this);

					previousScene.MovableAdded -= OnMovableAdded;
					previousScene.MovableRemoved -= OnMovableRemoved;

					_scene = value;
					_scene.AddLayer(this);

					_scene.MovableAdded += OnMovableAdded;
					_scene.MovableRemoved += OnMovableRemoved;

					OnSceneChanged(_scene);
				}
				else if (_scene != null && value == null)
				{
					IScene previousScene = _scene;
					_scene = null;

					previousScene.RemoveLayer(this);

					previousScene.MovableAdded -= OnMovableAdded;
					previousScene.MovableRemoved -= OnMovableRemoved;

					OnSceneChanged(_scene);
				}
			}
		}

		public float Depth { get; private set; }

		public ReadOnlyList<IRenderable> Renderables
		{
			get { return _readOnlyRenderableList; }
		}

		public void Draw(GameTime gameTime, ICamera camera)
		{
			_renderableList.Sort(CompareRenderables);

			foreach (var renderable in _renderableList)
			{
				renderable.Render(gameTime, camera);
			}
		}

		private int CompareRenderables(IRenderable a, IRenderable b)
		{
			if (a == b)
			{
				return 0;
			}

			var aa = a as IMovable;
			var bb = b as IMovable;

			if (aa == null && bb == null)
			{
				return 0;
			}
			if (aa != null && bb == null)
			{
				return 1;
			}
			if (aa == null && bb != null)
			{
				return 1;
			}

			return aa.SceneNode.Position.Z > bb.SceneNode.Position.Z ? 1 : -1;
		}

		#endregion
	}
}