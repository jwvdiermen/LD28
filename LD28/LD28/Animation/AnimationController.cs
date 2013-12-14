using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Animation
{
	/// <summary>
	/// Used for attaching an animation to an entity.
	/// </summary>
	public class AnimationController : EntityControllerBase
	{
		/// <summary>
		/// Gets the animation.
		/// </summary>
		public IAnimation Animation { get; private set; }

		/// <summary>
		/// Gets or sets if the controller should be removed when the animation has been finished.
		/// </summary>
		public bool RemoveWhenFinished { get; set; }

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="animation">The animation.</param>
		public AnimationController(IAnimation animation)
		{
			RemoveWhenFinished = true;

			Animation = animation;
			Animation.Finished += AnimationOnFinished;
		}

		/// <summary>
		/// When attached to an entity, plays the animation.
		/// </summary>
		public void Play()
		{
			if (Entity != null)
			{
				Animation.Play(Entity);
			}
		}

		/// <summary>
		/// Stops the animation.
		/// </summary>
		public void Stop()
		{
			Animation.Stop();
		}

		private void AnimationOnFinished(IAnimation animation)
		{
			if (RemoveWhenFinished)
			{
				Animation.Finished -= AnimationOnFinished;
				Detach();
			}
		}

		protected override void OnDetached(IEntity entity)
		{
			// Ensure the animation has been stopped when we're detached.
			Animation.Stop();
		}

		public override void Update(GameTime gameTime, ICamera camera)
		{
			Animation.Update(gameTime);
		}

		protected override void DisposeUnmanaged()
		{
			Animation.Finished -= AnimationOnFinished;
		}
	}
}
