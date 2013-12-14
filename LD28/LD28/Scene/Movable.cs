using LD28.Core;

namespace LD28.Scene
{
	/// <summary>
	/// This class provides an abstract implementation of the <see cref="IMovable" /> interface.
	/// </summary>
	public abstract class Movable : DisposableObject, IMovable
	{
		#region Constructors

		/// <summary>
		/// This constructor creates a new movable with the given name.
		/// </summary>
		protected Movable()
		{
		}

		/// <summary>
		/// This constructor creates a new movable with the given name.
		/// </summary>
		/// <param name="name">The optional name of the movable.</param>
		protected Movable(string name)
		{
			Name = name;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method is called when the movable is attached to the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node to which the movable was attached.</param>
		protected virtual void OnAttached(ISceneNode sceneNode)
		{
		}

		/// <summary>
		/// This method is called when the movable has been detached from the current scene node.
		/// </summary>
		protected virtual void OnDetached()
		{
		}

		protected override void DisposeUnmanaged()
		{
			Detach();

			// We should unload the content if we're a renderable. 
			var renderable = this as IRenderable;
			if (renderable != null)
			{
				renderable.UnloadContent();
			}
		}

		#endregion

		#region Properties

		public string Name { get; private set; }

		public ISceneNode SceneNode { get; private set; }

		#endregion

		#region Methods

		public void AttachTo(ISceneNode sceneNode)
		{
			if (SceneNode != sceneNode)
			{
				Detach();

				// Set the scene node first, because calling Attach on the scene node will also call
				// the AttachTo method on the movable. Setting this property first will prevent a
				// circular method call thanks to the previous if statement.
				SceneNode = sceneNode;

				SceneNode.Attach(this);
				OnAttached(sceneNode);
			}
		}

		public void Detach()
		{
			if (SceneNode != null)
			{
				// Reset the scene node first, because calling Detach on the scene node will also call
				// the Detach method on the movable. Setting this property to null first will prevent a
				// circular method call thanks to the previous if statement.
				ISceneNode previous = SceneNode;
				SceneNode = null;

				previous.Detach(this);
				OnDetached();
			}
		}

		#endregion
	}
}