using System;
using LD28.Animation;
using LD28.Entity;
using Microsoft.Xna.Framework;

namespace LD28.Animations
{
	/// <summary>
	/// Rotates the entity to the given orientation.
	/// </summary>
	public sealed class LocalRotateTo : AnimationBase
	{
		private readonly Quaternion _target;
		private Quaternion _start;

		/// <summary>
		/// Gets the target orientation.
		/// </summary>
		public Quaternion Target
		{
			get { return _target; }
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="target">The target orientation.</param>
		public LocalRotateTo(Quaternion target)
		{
			_target = target;
		}

		protected override void AnimationStarted()
		{
			var entity = Entity as IEntityWithParent;
			if (entity == null)
			{
				throw new InvalidOperationException("Can't locally move an entity without a parent.");
			}

			_start = entity.LocalOrientation;
		}

		protected override void DoAnimation(float position, float easing)
		{
			((IEntityWithParent)Entity).LocalOrientation = Quaternion.Slerp(_start, _target, easing);
		}
	}
}
