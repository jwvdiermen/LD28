using System.Collections.Generic;

namespace LD28.Core
{
	/// <summary>
	/// This class wraps a list, only exposing its enumerator.
	/// </summary>
	public class ReadOnlyList<T>
	{
		#region Constructors

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="list">The list to wrap.</param>
		public ReadOnlyList(List<T> list)
		{
			_list = list;
		}

		#endregion

		#region Fields

		private readonly List<T> _list;

		#endregion

		#region Methods

		/// <summary>
		/// This method returns the enumerator of the wrapped list.
		/// </summary>
		/// <returns>An enumerator for the wrapped list.</returns>
		public List<T>.Enumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion
	}
}
