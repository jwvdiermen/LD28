using System;
using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Scene
{
	/// <summary>
	/// This interface represents an object which can be rendered.
	/// </summary>
	[UsedImplicitly]
	public interface IRenderable : IDisposable
	{
		#region Methods

		/// <summary>
		/// This method is called when graphics resources need to be loaded.
		/// </summary>
		/// <param name="services">The service provider.</param>
		void LoadContent(IServiceProvider services);

		/// <summary>
		/// This method is called when graphics resources need to be unloaded.
		/// </summary>
		void UnloadContent();

		/// <summary>
		/// This method renders the object to the given sprite batch.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Render(GameTime gameTime, ICamera camera);

		#endregion
	}
}
