using Microsoft.Xna.Framework;

namespace LD28.Screen
{
	/// <summary>
	/// This interface represents a screen.
	/// </summary>
	public interface IScreen
	{
		/// <summary>
		/// Gets if the screen is a popup.
		/// </summary>
		bool IsPopup
		{
			get;
		}

		/// <summary>
		/// This method is called when graphics resources need to be loaded.
		/// </summary>
		void LoadContent();

		/// <summary>
		/// This method is called when graphics resources need to be unloaded.
		/// </summary>
		void UnloadContent();

		/// <summary>
		/// This method updates the screen.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="isActive">True if the screen is active.</param>
		/// <param name="isOverlayed">True if the screen is overlayed with another screen, for example a popup.</param>
		void Update(GameTime gameTime, bool isActive, bool isOverlayed);

		/// <summary>
		/// This method draws the screen.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		void Draw(GameTime gameTime);
	}
}
