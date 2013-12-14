using System;
using LD28.Entity;
using Microsoft.Xna.Framework;
using LD28.Scene;

namespace LD28.Animation
{
	/// <summary>
	/// An abstract implementation of the <see cref="IAnimation" /> interface.
	/// </summary>
	public abstract class AnimationBase : IAnimation
	{
		private long _startTime;
		private long _endTime;

		public IEasing Easing { get; set; }
		public TimeSpan Duration { get; set; }
		public AnimationState State { get; private set; }
		public IEntity Entity { get; private set; }
		public float Position { get; private set; }

		#region Events

		public event AnimationStarted Started;

		/// <summary>
		/// Fires the "Started" event.
		/// </summary>
		protected void OnStarted()
		{
			if (Started != null)
			{
				Started(this);
			}
		}

		public event AnimationProgressEventHandler Progress;

		/// <summary>
		/// Fires the "Progress" event.
		/// </summary>
		/// <param name="progress">The progress.</param>
		protected void OnProgress(float progress)
		{
			if (Progress != null)
			{
				Progress(this, progress);
			}
		}

		public event AnimationFinished Finished;

		/// <summary>
		/// Fires the "Finished" event.
		/// </summary>
		protected void OnFinished()
		{
			if (Finished != null)
			{
				Finished(this);
			}
		}

		#endregion

		public void Play(IEntity entity)
		{
			if (State == AnimationState.Stopped || State == AnimationState.Finished)
			{
				Entity = entity;
				_startTime = -1;
				State = AnimationState.Playing;
			}
		}

		public void Stop()
		{
			if (State != AnimationState.Finished)
			{
				Entity = null;
				_startTime = -1;
				State = AnimationState.Stopped;
			}
		}

		public void Update(GameTime gameTime)
		{
			if (State == AnimationState.Playing)
			{
				var currentTime = gameTime.TotalGameTime.Ticks;

				// Initialize the star time if necessary.
				if (_startTime < 0)
				{
					_startTime = currentTime;
					_endTime = _startTime + Duration.Ticks;

					AnimationStarted();
					OnStarted();
				}

				// Calculate the current progress.
				Position = MathHelper.Clamp(1.0f - 1.0f / (_endTime - _startTime) * (_endTime - currentTime), 0.0f, 1.0f);
				OnProgress(Position);

				// Calculate the easing.
				var easing = Easing != null ? Easing.Calculate(Position) : Position;
				DoAnimation(Position, easing);

				// Check if the animation has been finished.
				if (currentTime >= _endTime)
				{
					State = AnimationState.Finished;

					AnimationFinished();
					OnFinished();
				}
			}
		}

		/// <summary>
		/// Called when the animation has been started.
		/// </summary>
		protected virtual void AnimationStarted()
		{
		}

		/// <summary>
		/// When overriden, should progress the animation.
		/// </summary>
		/// <param name="position">The current position in the animation.</param>
		/// <param name="easing">The easing value calculated from the position.</param>
		protected abstract void DoAnimation(float position, float easing);

		protected virtual void AnimationFinished()
		{
		}
	}
}
