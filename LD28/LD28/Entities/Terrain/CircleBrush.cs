using Microsoft.Xna.Framework;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// A circular terrain brush.
	/// </summary>
	public class CircleBrush : ITerrainBrush
	{
		private BoundingSphere _boundingSphere;

		/// <summary>
		/// Gets the circle radius.
		/// </summary>
		public float Radius
		{
			get { return _boundingSphere.Radius; }
		}
		
		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="radius">The circel radius.</param>
		public CircleBrush(float radius)
		{
			_boundingSphere = new BoundingSphere(Vector3.Zero, radius);
		}

		public Vector2 Position
		{
			get { return new Vector2(_boundingSphere.Center.X, _boundingSphere.Center.Y); }
			set { _boundingSphere.Center.X = value.X; _boundingSphere.Center.Y = value.Y; }
		}

		public ContainmentType Contains(ref BoundingBox boundingBox)
		{
			ContainmentType result;
			_boundingSphere.Contains(ref boundingBox, out result);

			return result;
		}
	}
}
