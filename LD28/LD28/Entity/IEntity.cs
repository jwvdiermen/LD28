using System;
using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This interface represents an entity. An entity is a three-dimensional object used to
	/// bind all the components (graphics, physics, logic) into a single object. The minimum requirement
	/// of an entity is a location in the world which is represented by an <see cref="IScene" />.
	/// </summary>
	public interface IEntity
	{
		#region Properties

		/// <summary>
		/// Gets the optional name.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets or sets the world the entity belongs to.
		/// Either use this property or the <see cref="M:IEntityWorld.Add" /> method.
		/// </summary>
		IEntityWorld World
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the scene node which represents the entity.
		/// </summary>
		ISceneNode SceneNode
		{
			get;
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		Vector3 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
		Quaternion Orientation
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a read-only list containing the controllers attached to the entity.
		/// </summary>
		ReadOnlyList<IEntityController> Controllers { get; }

		/// <summary>
		/// Gets or sets if the entity is active and should be updated.
		/// </summary>
		bool IsActive
		{
			get;
			set;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method is called when graphics resources need to be loaded.
		/// </summary>
		/// <param name="services">The service provider.</param>
		void LoadContent(IServiceProvider services);

		/// <summary>
		/// This method is called when graphics resources need to be unloaded.
		/// </summary>
		void UnloadContent();

		/// <summary>
		/// This method attaches the given controller to the current entity.
		/// </summary>
		/// <param name="controller">The controller to attach.</param>
		/// <remarks>If the given movable is already attached to another scene node, it is first detached.</remarks>
		void Attach(IEntityController controller);

		/// <summary>
		/// This method detaches the given controller from the current entity.
		/// </summary>
		/// <param name="controller">The controller to detach.</param>
		void Detach(IEntityController controller);
		
		/// <summary>
		/// Gets the first controller of the given type.
		/// </summary>
		/// <typeparam name="T">The type of the controller.</typeparam>
		/// <returns>The controller if found or null if not.</returns>
		T Controller<T>() where T : IEntityController;

		/// <summary>
		/// This method updates the entity.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Update(GameTime gameTime, ICamera camera);

		/// <summary>
		/// Destroys the entity.
		/// </summary>
		/// <remarks>If you are planning to reuse the entity, set the World property to null instead,
		/// or disable and hide the entity.</remarks>
		void Destroy();

		#endregion
	}
}
