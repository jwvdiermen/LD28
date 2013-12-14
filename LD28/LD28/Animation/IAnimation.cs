using LD28.Entity;
using System;
using Microsoft.Xna.Framework;

namespace LD28.Animation
{
	/// <summary>
	/// An animation which can be used to animate any entity.
	/// </summary>
	public interface IAnimation
	{
		/// <summary>
		/// Gets or sets the easing. Can be null.
		/// </summary>
		IEasing Easing { get; set; }

		/// <summary>
		/// Gets or sets the duration of the animation.
		/// </summary>
		TimeSpan Duration { get; set; }

		/// <summary>
		/// Gets the state of the animation.
		/// </summary>
		AnimationState State { get; }

		/// <summary>
		/// Gets the position in the timeline ranging from 0 to 1.
		/// </summary>
		float Position { get; }

		/// <summary>
		/// Fired when the animation has started.
		/// </summary>
		event AnimationStarted Started;

		/// <summary>
		/// Fired when the animation has been updated.
		/// </summary>
		event AnimationProgressEventHandler Progress;

		/// <summary>
		/// Fired when the animation has been finished.
		/// </summary>
		event AnimationFinished Finished;

		/// <summary>
		/// Plays the animation.
		/// </summary>
		/// <param name="entity">The entity to play the animation for.</param>
		void Play(IEntity entity);

		/// <summary>
		/// Stops the animation.
		/// </summary>
		void Stop();

		/// <summary>
		/// Updates the animation.
		/// </summary>
		/// <param name="gameTime">The game time.</param>
		void Update(GameTime gameTime);
	}
}
