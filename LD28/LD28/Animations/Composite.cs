using System;
using System.Collections.Generic;
using LD28.Animation;
using System.Collections;
using System.Linq;
using LD28.Scene;
using Microsoft.Xna.Framework;
using LD28.Entity;

namespace LD28.Animations
{
	/// <summary>
	/// A composite animation of one or more other animations, executed in parallel.
	/// Finishes when all the contained animations are finished.
	/// </summary>
	public sealed class Composite : IAnimation, IList<IAnimation>
	{
		private readonly List<IAnimation> _animationList = new List<IAnimation>();

		private long _startTime;
		private long _endTime;
		private bool _isLocked;

		public IEasing Easing
		{
			get { return null; }
			set { }
		}

		public TimeSpan Duration
		{
			get { return _animationList.Max(x => x.Duration); }
			set { throw new InvalidOperationException(); }
		}

		public AnimationState State { get; private set; }
		public IEntity Entity { get; private set; }

		public float Position
		{
			get { return _animationList.Max(x => x.Position); }
		}

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

		/// <summary>
		/// Locks the composite, preventing any further changes.
		/// </summary>
		public void Lock()
		{
			_isLocked = true;
		}

		public void Play(IEntity _entity)
		{
			if (State == AnimationState.Stopped || State == AnimationState.Finished)
			{
				Entity = _entity;
				_startTime = -1;

				State = AnimationState.Playing;
				foreach (var animation in _animationList)
				{
					animation.Play(_entity);
				}
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
				}

				// Update all the contained animations.
				foreach (var animation in _animationList)
				{
					animation.Update(gameTime);
				}
				OnProgress(Position);

				// Check if the animation has been finished.
				if (currentTime >= _endTime)
				{
					State = AnimationState.Finished;
					foreach (var animation in _animationList)
					{
						animation.Stop();
					}

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
			get { return _isLocked || !(State == AnimationState.Stopped || State == AnimationState.Finished); }
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
