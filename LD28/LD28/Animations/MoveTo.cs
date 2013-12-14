using LD28.Animation;
using Microsoft.Xna.Framework;

namespace LD28.Animations
{
	/// <summary>
	/// Moves the entity to the given coordinates.
	/// </summary>
	public sealed class MoveTo : AnimationBase
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
		public MoveTo(Vector3 target)
		{
			_target = target;
		}

		protected override void AnimationStarted()
		{
			_start = Entity.Position;
		}

		protected override void DoAnimation(float position, float easing)
		{
			Entity.Position = new Vector3(
				MathHelper.Lerp(_start.X, _target.X, easing),
				MathHelper.Lerp(_start.Y, _target.Y, easing),
				MathHelper.Lerp(_start.Z, _target.Z, easing));
		}
	}
}
