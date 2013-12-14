using System;

namespace LD28.Core
{
	/// <summary>
	/// Represents a result which is not available directly.
	/// </summary>
	public interface IDelayedResult
	{
		/// <summary>
		/// Adds an callback delegate.
		/// </summary>
		/// <param name="callback">The callback.</param>
		void Callback(Action callback);
	}

	/// <summary>
	/// Represents a result which is not available directly.
	/// </summary>
	public interface IDelayedResult<T> : IDelayedResult
	{
		/// <summary>
		/// Adds an callback delegate.
		/// </summary>
		/// <param name="callback">The callback.</param>
		void Callback(Action<T> callback);
	}
}
