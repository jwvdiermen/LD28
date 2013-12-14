using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// Represents an entity with a parent.
	/// </summary>
	interface IEntityWithParent : IEntity
	{
		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		IEntity Parent { get; set; }

		/// <summary>
		/// Gets or sets the local position.
		/// </summary>
		Vector3 LocalPosition
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the local orientation.
		/// </summary>
		Quaternion LocalOrientation
		{
			get;
			set;
		}
	}
}
