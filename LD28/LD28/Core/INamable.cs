namespace LD28.Core
{
	/// <summary>
	/// Used by type which can have a name.
	/// </summary>
	[UsedImplicitly]
	public interface INamable
	{
		#region Properties

		/// <summary>
		/// Gets the name.
		/// </summary>
		string Name
		{
			get;
		}

		#endregion
	}
}
