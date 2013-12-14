using LD28.Animation;
using LD28.Entity;
using Microsoft.Xna.Framework;
using System;

namespace LD28.Animations
{
	/// <summary>
	/// Moves the entity to the given coordinates.
	/// </summary>
	public sealed class LocalMoveTo : AnimationBase
	{
		private readonly Vector3 _target;
		private Vector3 _start;

		/// <summary>
		/// Gets the target coordinates.
		/// </summary>
		public Vector3 Target
		{
			get { return _target; }
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="target">The target coordinates</param>
		public LocalMoveTo(Vector3 target)
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

			_start = entity.LocalPosition;
		}

		protected override void DoAnimation(float position, float easing)
		{
			((IEntityWithParent)Entity).LocalPosition = new Vector3(
				MathHelper.Lerp(_start.X, _target.X, easing),
				MathHelper.Lerp(_start.Y, _target.Y, easing),
				MathHelper.Lerp(_start.Z, _target.Z, easing));
		}
	}
}
