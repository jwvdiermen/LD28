using System.Linq;
using LD28.Core;
using System.Collections.Generic;
using System;

namespace LD28.Animation
{
	/// <summary>
	/// A delayed result which is completed when all assign animations are finished.
	/// </summary>
	public class AnimationResult : DelayedResult, IDisposable
	{
		private int _uncompletedAnimationCount;
		private bool _isStarted = false;
		private readonly List<IAnimation> _animationList = new List<IAnimation>();

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="animations">The animations to wait for.</param>
		public AnimationResult(params IAnimation[] animations)
		{
			if (animations != null)
			{
				_animationList.AddRange(animations);
			}
			_uncompletedAnimationCount += _animationList.Count(x => x.State != AnimationState.Finished);

			foreach (var animation in _animationList)
			{
				animation.Finished += AnimationOnFinished;
			}
		}

		~AnimationResult()
		{
			Dispose();
		}

		/// <summary>
		/// Adds the given animation.
		/// </summary>
		/// <param name="animation">The animation.</param>
		public void Add(IAnimation animation)
		{
			lock (this)
			{
				animation.Finished += AnimationOnFinished;
				_animationList.Add(animation);

				if (animation.State != AnimationState.Finished)
				{
					_uncompletedAnimationCount++;
				}
			}
		}

		/// <summary>
		/// Starts listening to the animations.
		/// </summary>
		/// <returns>This.</returns>
		public AnimationResult Start()
		{
			lock (this)
			{
				_isStarted = true;
				if (_uncompletedAnimationCount <= 0)
				{
					Complete();
				}

				return this;
			}
		}

		private void AnimationOnFinished(IAnimation animation)
		{
			lock (this)
			{
				_uncompletedAnimationCount--;
				if (_uncompletedAnimationCount <= 0 && _isStarted)
				{
					Complete();
				}
			}
		}

		public void Dispose()
		{
			foreach (var animation in _animationList)
			{
				animation.Finished -= AnimationOnFinished;
			}
			_animationList.Clear();
		}
	}
}
