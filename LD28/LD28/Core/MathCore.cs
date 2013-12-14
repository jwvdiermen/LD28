using System;
using Microsoft.Xna.Framework;

namespace LD28.Core
{
	/// <summary>
	/// Helper methods with mathh.
	/// </summary>
	public class MathCore
	{
		#region Methods

		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		/// <param name="a">An angle, measured in radians.</param>
		/// <returns>An angle, measured in degrees.</returns>
		public static float Sin(float a)
		{
			return (float)Math.Sin(a);
		}

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		/// <param name="d">An angle, measured in radians.</param>
		/// <returns>An angle, measured in degrees.</returns>
		public static float Cos(float d)
		{
			return (float)Math.Cos(d);
		}

		/// <summary>
		/// Returns a specified number raised to the specified power.
		/// </summary>
		/// <param name="x">A floating-point number to be raised to a power.</param>
		/// <param name="y">A floating-point number that specifies a power.</param>
		/// <returns>The number <paramref name="x"/> raised to the power <paramref name="y"/>.</returns>
		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		/// <summary>
		/// Wraps the given angle between 180 and -180 degrees.
		/// </summary>
		/// <param name="a">The angle.</param>
		/// <returns>The wrapped angle.</returns>
		public static float WrapAngle(float a)
		{
			while (a > 180.0f)
			{
				a = -180.0f + (a - 180.0f);
			}
			while (a < -180.0f)
			{
				a = 180.0f + (a + 180.0f);
			}
			return a;
		}

		#endregion
	}
}
