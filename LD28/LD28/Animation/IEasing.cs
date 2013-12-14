namespace LD28.Animation
{
	/// <summary>
	/// Easing allows for different transition speeds based on duration. By implementing this
	/// interface, different easing techniques can be implemented.
	/// </summary>
	public interface IEasing
	{
		/// <summary>
		/// Calculate the easing of given value.
		/// </summary>
		/// <param name="value">A value ranging from 0 to 1 based on the animation time.</param>
		/// <returns>The resulted easing.</returns>
		float Calculate(float value);
	}
}
