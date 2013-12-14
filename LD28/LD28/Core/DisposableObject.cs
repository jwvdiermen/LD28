using System;

namespace LD28.Core
{
    /// <summary>
    /// An object that notifies when it is disposed.
    /// </summary>
    public abstract class DisposableObject : IDisposableObject
	{
		#region IDisposableObject Members

		public event EventHandler Disposed;

		private void OnDisposed()
		{
			if (this.Disposed != null)
			{
				this.Disposed(this, EventArgs.Empty);
			}
		}

	
        public bool IsDisposed 
		{ 
			get; 
			private set; 
		}

		#endregion

		#region Methods

		/// <summary>
        /// This method performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
			Dispose(true);
			GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method releases resources held by the object.
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            lock (this)
            {
				if (IsDisposed == false)
				{
					IsDisposed = true;

					if (disposing == true)
					{
						DisposeManaged();
					}

					DisposeUnmanaged();
					OnDisposed();				
				}
            }
        }

		/// <summary>
		/// This method should released managed objects.
		/// </summary>
		protected virtual void DisposeManaged()
		{
		}

		/// <summary>
		/// This method should released unmanaged objects.
		/// </summary>
		protected virtual void DisposeUnmanaged()
		{
		}

		/// <summary>
		/// This method checks if the current object is disposed and throws an exception if it is.
		/// </summary>
		/// <exception cref="ObjectDisposedException" />
		protected void CheckDisposed()
		{
			if (IsDisposed == true)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

        /// <summary>
        /// Releases resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~DisposableObject()
        {
            Dispose(false);
		}

		#endregion
	}
}