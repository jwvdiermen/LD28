using Microsoft.Xna.Framework;

namespace LD28.Screen
{
	/// <summary>
	/// This interface represents the screen manager, responsible for
	/// the screens in a game, represented by the interface (see <see cref="IScreen" />). By activating
	/// screens, a state is either entered or left based on the screen's properties. The manager also 
	/// handles transisions between screens if necessary.
	/// </summary>
	public interface IScreenManager : IGameComponent
	{
		/// <summary>
		/// Gets the current active screen.
		/// </summary>
		IScreen ActiveScreen
		{
			get;
		}

		/// <summary>
		/// This method pushes a screen onto the stack and actives it.
		/// </summary>
		/// <param name="screen">The screen to push.</param>
		void PushScreen(IScreen screen);

		/// <summary>
		/// This method pops the current screen from the stack and activates the
		/// previous screen if there is one.
		/// </summary>
		/// <returns>The popped screen.</returns>
		IScreen PopScreen();

		/// <summary>
		/// This method actives the given screen, clearing the current stack.
		/// </summary>
		/// <param name="screen">The screen to activate.</param>
		void ActivateScreen(IScreen screen);
	}
}
