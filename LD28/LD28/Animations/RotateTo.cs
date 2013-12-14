using LD28.Animation;
using Microsoft.Xna.Framework;

namespace LD28.Animations
{
	/// <summary>
	/// Rotates the entity to the given orientation.
	/// </summary>
	public sealed class RotateTo : AnimationBase
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
		public RotateTo(Quaternion target)
		{
			_target = target;
		}

		protected override void AnimationStarted()
		{
			_start = Entity.Orientation;
		}

		protected override void DoAnimation(float position, float easing)
		{
			Entity.Orientation = Quaternion.Slerp(_start, _target, easing);
		}
	}
}
