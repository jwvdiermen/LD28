using System;
using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Scene
{
	/// <summary>
	/// This interface represents a layer to which <see cref="IRenderable" /> objects are rendered.
	/// </summary>
	public interface ILayer : IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets or sets the scene.
		/// </summary>
		IScene Scene
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the depth.
		/// </summary>
		float Depth
		{
			get;
		}

		/// <summary>
		/// Gets a read-only list containing the renderable in the layeer.
		/// </summary>
		ReadOnlyList<IRenderable> Renderables
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Draws the renderables contained in the layer.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		/// <param name="camera">The camera.</param>
		void Draw(GameTime gameTime, ICamera camera);

		#endregion
	}
}
