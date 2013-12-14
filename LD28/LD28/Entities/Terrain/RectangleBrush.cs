using Microsoft.Xna.Framework;

namespace LD28.Entities.Terrain
{
	/// <summary>
	/// A circular terrain brush.
	/// </summary>
	public class RectangleBrush : ITerrainBrush
	{
		private BoundingBox _boundingBox;

		/// <summary>
		/// Gets the rectangle size.
		/// </summary>
		public Vector2 Size
		{
			get
			{
				var min = _boundingBox.Min;
				var max = _boundingBox.Max;
				var size = max - min;
				return new Vector2(size.X, size.Y);
			}
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="size">The size.</param>
		public RectangleBrush(Vector2 size)
		{
			_boundingBox = new BoundingBox(
				new Vector3(-size / 2.0f, 0.0f),
				new Vector3(size / 2.0f, 0.0f));
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <param name="position">The position.</param>
		public RectangleBrush(Vector2 size, Vector2 position)
		{
			_boundingBox = new BoundingBox(
				new Vector3(position - size / 2.0f, 0.0f),
				new Vector3(position + size / 2.0f, 0.0f));
		}

		public Vector2 Position
		{
			get
			{
				var min = _boundingBox.Min;
				var max = _boundingBox.Max;
				var center = min + ((max - min) / 2.0f);
				return new Vector2(center.X, center.Y);
			}
			set
			{
				var size = Size;
				_boundingBox = new BoundingBox(
					new Vector3(value - size / 2.0f, 0.0f),
					new Vector3(value + size / 2.0f, 0.0f));
			}
		}

		public ContainmentType Contains(ref BoundingBox boundingBox)
		{
			ContainmentType result;
			_boundingBox.Contains(ref boundingBox, out result);
			return result;
		}
	}
}
