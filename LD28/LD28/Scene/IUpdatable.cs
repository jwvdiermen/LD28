using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Scene
{
	/// <summary>
	/// This interface represents an object which can be updated.
	/// </summary>
	[UsedImplicitly]
	public interface IUpdatable
	{
		#region Methods

		/// <summary>
		/// This method updates the object.
		/// </summary>
		/// <param name="time">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Update(GameTime time, ICamera camera);

		#endregion
	}
}
