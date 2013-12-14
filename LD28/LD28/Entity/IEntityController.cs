using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This interface represents a controller which can be attached to an <see cref="IEntity" /> which
	/// is then automatically updated by the entity. A controller is used for controlling the behaviour
	/// of an entity, like handling player input.
	/// </summary>
	public interface IEntityController
	{
		#region Properties

		/// <summary>
		/// Gets the entity to which the controller is attached.
		/// </summary>
		IEntity Entity
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method attaches the controller to the given entity.
		/// </summary>
		/// <param name="entity">The entity to attach the controller to.</param>
		/// <remarks>If the controller is already attached to another entity, it is first detached.</remarks>
		void AttachTo(IEntity entity);

		/// <summary>
		/// This method detaches the controller from the current entity.
		/// </summary>
		void Detach();

		/// <summary>
		/// This method updates the entity controller.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Update(GameTime gameTime, ICamera camera);

		#endregion
	}
}
