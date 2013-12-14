using System.Collections.Generic;
using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This interface represents a world in which entities (see <see cref="IEntity" />) are managed.
	/// To visualize the entities, the world also contains an <see cref="IScene" /> containing scene nodes
	/// which are coupled with entities.
	/// </summary>
	public interface IEntityWorld : IDisposableObject
	{
		#region Properties

		/// <summary>
		/// Gets the scene.
		/// </summary>
		IScene Scene
		{
			get;
		}

		/// <summary>
		/// Gets an enumerable containing all the entities in the world.
		/// </summary>
		IEnumerable<IEntity> Entities
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method adds the given entity to the world.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		void Add(IEntity entity);
		
		/// <summary>
		/// This method removes the given entity from the world.
		/// </summary>
		/// <param name="entity">The entity to remove.</param>
		void Remove(IEntity entity);

		/// <summary>
		/// This method updates the world.
		/// </summary>
		/// <param name="time">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Update(GameTime time, ICamera camera);

		#endregion
	}
}
