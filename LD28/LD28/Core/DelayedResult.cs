using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD28.Core
{
	/// <summary>
	/// Default implementation of the <see cref="IDelayedResult" /> interface.
	/// </summary>
	public class DelayedResult : IDelayedResult
	{
		private bool _hasCompleted;

		private Action _callback;
		public void Callback(Action callback)
		{
			lock (this)
			{
				_callback += callback;

				if (_hasCompleted)
				{
					callback();
				}
			}
		}

		/// <summary>
		/// Completes the result and reports it to the callback.
		/// </summary>
		/// <returns>This.</returns>
		public DelayedResult Complete()
		{
			lock (this)
			{
				if (_hasCompleted)
				{
					throw new InvalidOperationException("The operation has already completed.");
				}

				_hasCompleted = true;

				if (_callback != null)
				{
					_callback();
				}

				return this;
			}
		}

		/// <summary>
		/// Returns a completed delayed result.
		/// </summary>
		/// <returns>The completed delayed result.</returns>
		public static DelayedResult Completed()
		{
			return new DelayedResult().Complete();
		}
	}

	/// <summary>
	/// Default implementation of the <see cref="IDelayedResult" /> interface.
	/// </summary>
	public class DelayedResult<T> : DelayedResult, IDelayedResult<T>
	{
		private bool _hasCompleted;
		private T _result;

		private Action<T> _callback;
		public void Callback(Action<T> callback)
		{
			lock (this)
			{
				_callback += callback;

				if (_hasCompleted)
				{
					callback(_result);
				}
			}
		}

		/// <summary>
		/// Completes the result and reports it to the callback.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns>This.</returns>
		public DelayedResult<T> Complete(T result)
		{
			lock (this)
			{
				if (_hasCompleted)
				{
					throw new InvalidOperationException("The operation has already completed.");
				}

				_hasCompleted = true;
				_result = result;

				if (_callback != null)
				{
					_callback(_result);
				}

				return this;
			}
		}

		/// <summary>
		/// Returns a completed delayed result.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns>The completed delayed result.</returns>
		public static DelayedResult<T> Completed(T result)
		{
			return new DelayedResult<T>().Complete(result);
		}
	}
}
