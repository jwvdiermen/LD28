using System;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Sprite
{
	/// <summary>
	/// This interface represents a sprite.
	/// </summary>
	public interface ISprite : IDisposable, IMovable, IUpdatable, IRenderable
	{
		#region Properties
		
		/// <summary>
		/// Gets the size of the sprite.
		/// </summary>
		Vector2 Size { get; }

		/// <summary>
		/// Gets the pivot of the sprite.
		/// </summary>
		Vector2 Pivot { get; }

		/// <summary>
		/// Gets or sets the sprite effects.
		/// </summary>
		SpriteEffects Effects { get; set; }

		/// <summary>
		/// Gets or sets the blend state to use when rendering.
		/// </summary>
		BlendState BlendState { get; set; }

		/// <summary>
		/// Gets or sets the rasterizer state to use when rendering.
		/// </summary>
		RasterizerState RasterizerState { get; set; }

		/// <summary>
		/// Gets or sets the sampler state to use when rendering.
		/// </summary>
		SamplerState SamplerState { get; set; }

		#endregion
	}
}
