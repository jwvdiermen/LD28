using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// This interface represents a brush used for modifying a <see cref="Terrain" />.
	/// </summary>
	public interface ITerrainBrush
	{
		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		Vector2 Position { get; set; }

		/// <summary>
		/// This method checks if the given bounding box is contained by the brush.
		/// </summary>
		/// <param name="boundingBox">The bounding box.</param>
		/// <returns>The containment type.</returns>
		ContainmentType Contains(ref BoundingBox boundingBox);
	}
}
