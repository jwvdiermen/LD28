using System;
using System.Collections.Generic;
using LD28.Animation;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using LD28.Entity;

namespace LD28.Animations
{
	/// <summary>
	/// A chain animation of one or more other animations, executed in sequence.
	/// </summary>
	public sealed class Chain : IAnimation, IList<IAnimation>
	{
		private readonly List<IAnimation> _animationList = new List<IAnimation>();

		private long _startTime;
		private long _endTime;
		private int _animationIndex;

		public IEasing Easing
		{
			get { return null; }
			set { }
		}

		public TimeSpan Duration
		{
			get { return TimeSpan.FromTicks(_animationList.Sum(x => x.Duration.Ticks)); }
			set { throw new InvalidOperationException(); }
		}

		public AnimationState State { get; private set; }
		public IEntity Entity { get; private set; }
		public float Position { get; private set; }

		#region Events

		public event AnimationStarted Started;
		private void OnStarted()
		{
			if (Started != null)
			{
				Started(this);
			}
		}

		public event AnimationProgressEventHandler Progress;
		private void OnProgress(float progress)
		{
			if (Progress != null)
			{
				Progress(this, progress);
			}
		}

		public event AnimationFinished Finished;
		private void OnFinished()
		{
			if (Finished != null)
			{
				Finished(this);
			}
		}

		#endregion

		public void Play(IEntity _entity)
		{
			if (State == AnimationState.Stopped || State == AnimationState.Finished)
			{
				Entity = _entity;

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
				foreach (var animation in _animationList)
				{
					animation.Stop();
				}
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

					OnStarted();

					// Start the first animation.
					_animationList[_animationIndex].Play(Entity);
				}

				// Calculate the current progress.
				Position = MathHelper.Clamp(1.0f - 1.0f / (_endTime - _startTime) * (_endTime - currentTime), 0.0f, 1.0f);
				OnProgress(Position);

				// Update the current animation.
				var animation = _animationList[_animationIndex];
				animation.Update(gameTime);

				if (animation.Position >= 1.0f && _animationIndex + 1 < _animationList.Count)
				{
					_animationList[++_animationIndex].Play(Entity);
				}

				// Check if the animation has been finished.
				if (currentTime >= _endTime)
				{
					State = AnimationState.Finished;
					OnFinished();
				}
			}
		}

		#region List Members

		public int IndexOf(IAnimation item)
		{
			return _animationList.IndexOf(item);
		}

		public void Insert(int index, IAnimation item)
		{
			_animationList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_animationList.RemoveAt(index);
		}

		public IAnimation this[int index]
		{
			get { return _animationList[index]; }
			set { _animationList[index] = value; }
		}

		public void Add(IAnimation item)
		{
			_animationList.Add(item);
		}

		public void Clear()
		{
			_animationList.Clear();
		}

		public bool Contains(IAnimation item)
		{
			return _animationList.Contains(item);
		}

		public void CopyTo(IAnimation[] array, int arrayIndex)
		{
			_animationList.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _animationList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(IAnimation item)
		{
			return _animationList.Remove(item);
		}

		public IEnumerator<IAnimation> GetEnumerator()
		{
			return _animationList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
