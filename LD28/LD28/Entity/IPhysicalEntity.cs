using FarseerPhysics.Dynamics;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This interface represents an entity with physical properties.
	/// </summary>
	public interface IPhysicalEntity : IEntity
	{
		/// <summary>
		/// Gets the physical body.
		/// </summary>
		Body Body
		{
			get;
		}

		/// <summary>
		/// This method updates the entity's physics.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		void UpdatePhysics(GameTime gameTime, ICamera camera);
	}
}
