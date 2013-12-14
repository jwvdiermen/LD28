using System;

namespace LD28.Animation.Easing
{
	/// <summary>
	/// Eases movement by accelerating first and then slowing down.
	/// </summary>
	public class Ease : IEasing
	{
		#region IEasing Members

		public float Calculate(float value)
		{
			double q = 0.07813 - (double)value / 2.0,
			       Q = Math.Sqrt(0.0066 + q * q),
			       x = Q - q,
			       X = Math.Pow(Math.Abs(x), 1.0 / 3.0) * (x < 0.0 ? -1.0 : 1.0),
			       y = -Q - q,
			       Y = Math.Pow(Math.Abs(y), 1.0 / 3.0) * (y < 0.0 ? -1.0 : 1.0),
			       t = X + Y + 0.25,
			       result = Math.Pow(1.0 - t, 2.0) * 3.0 * t * 0.1 + (1.0 - t) * 3.0 * t * t + t * t * t;

			return (float)result;
		}

		#endregion
	}
}
