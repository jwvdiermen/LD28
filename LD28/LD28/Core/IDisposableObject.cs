using System;

namespace LD28.Core
{
    /// <summary>
    /// An object that can report whether or not it is disposed.
    /// </summary>
    public interface IDisposableObject : IDisposable
	{
		#region Events

		/// <summary>
		/// Occurs when the object is disposed.
		/// </summary>
		event EventHandler Disposed;

		#endregion

		#region Properties

		/// <summary>
        /// Gets a value indicating whether the instance is disposed.
        /// </summary>
        bool IsDisposed 
		{ 
			get;
		}

		#endregion
	}
}