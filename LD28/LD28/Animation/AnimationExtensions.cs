using System;
using System.Collections.Generic;
using LD28.Entity;
using LD28.Core;
using Microsoft.Xna.Framework;

namespace LD28.Animation
{
	/// <summary>
	/// Contains extension methods to fluently create animations.
	/// </summary>
	public static class AnimationExtensions
	{
		private static IAnimation Attach(IEntity entity, IAnimation animation)
		{
			var controller = entity.Controller<AnimationController>();
			if (controller != null)
			{
				controller.Detach();
			}
			
			var chain = new Animations.Chain();
			if (animation != null)
			{
				chain.Add(animation);
			}

			controller = new AnimationController(chain);
			controller.AttachTo(entity);

			return chain;
		}

		private static IAnimation Attach(IAnimation target, IAnimation animation)
		{
			// Check if we need to attach to an existing composite.
			var composite = target as Animations.Composite;
			if (composite != null && !composite.IsReadOnly)
			{
				composite.Add(animation);
				return composite;
			}
			
			// Check if we need to create a new chain.
			var chain = target as Animations.Chain;
			if (chain == null)
			{
				throw new InvalidOperationException("Expected an animation of type 'Chain'.");
			}

			chain.Add(animation);
			return chain;
		}

		#region Entity controll

		/// <summary>
		/// Calls the given callback when the animation has been finished.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns>The animation.</returns>
		public static IAnimation OnFinished(this IAnimation animation, AnimationFinished callback)
		{
			animation.Finished += callback;
			return animation;
		}

		/// <summary>
		/// Plays the animation if any is attached to the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public static void PlayAnimation(this IEntity entity)
		{
			var controller = entity.Controller<AnimationController>();
			if (controller != null)
			{
				controller.Play();
			}
		}

		/// <summary>
		/// Stops the animation if any is attached to the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public static void StopAnimation(this IEntity entity)
		{
			var controller = entity.Controller<AnimationController>();
			if (controller != null)
			{
				controller.Stop();
			}
		}

		#endregion

		#region Modifiers

		/// <summary>
		/// Sets the duration of the given animation.
		/// </summary>
		/// <param name="animation">The animation.</param>
		/// <param name="duration">The duration.</param>
		/// <returns>The animation.</returns>
		public static IAnimation In(this IAnimation animation, TimeSpan duration)
		{
			// If we are currently working with a chain or composite, apply this operation to the
			// last animation created.
			var list = animation as IList<IAnimation>;
			if (list != null)
			{
				list[list.Count - 1].Duration = duration;
			}
			else
			{
				animation.Duration = duration;
			}

			return animation;
		}

		/// <summary>
		/// Sets the Ease easing.
		/// </summary>
		/// <param name="animation">The animation.</param>
		/// <returns>The animation.</returns>
		public static IAnimation Ease(this IAnimation animation)
		{
			// If we are currently working with a chain or composite, apply this operation to the
			// last animation created.
			var list = animation as IList<IAnimation>;
			if (list != null)
			{
				list[list.Count - 1].Easing = new Easing.Ease();
			}
			else
			{
				animation.Easing = new Easing.Ease();
			}

			return animation;
		}

		#endregion

		#region Composite

		/// <summary>
		/// Creates a new composite animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="callback">The callback in which the animations can be attached.</param>
		/// <returns>The composite animation.</returns>
		public static IAnimation Composite(this IEntity entity, Action<IAnimation> callback)
		{
			var composite = new Animations.Composite();

			callback(composite);
			composite.Lock();

			return Attach(entity, composite);
		}

		/// <summary>
		/// Creates a new composite animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="callback">The callback in which the animations can be attached.</param>
		/// <returns>The composite animation.</returns>
		public static IAnimation Composite(this IAnimation animation, Action<IAnimation> callback)
		{
			var composite = new Animations.Composite();

			callback(composite);
			composite.Lock();

			return Attach(animation, composite);
		}

		#endregion

		#region Animations

		/// <summary>
		/// Creates a MoveTo animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="target">The target position.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation MoveTo(this IEntity entity, Vector3 target)
		{
			return Attach(entity, new Animations.MoveTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Chains a MoveTo animation to an existing animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="target">The target position.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation MoveTo(this IAnimation animation, Vector3 target)
		{
			return Attach(animation, new Animations.MoveTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Creates a LocalMoveTo animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="target">The target position.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation LocalMoveTo(this IEntity entity, Vector3 target)
		{
			return Attach(entity, new Animations.LocalMoveTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Chains a LocalMoveTo animation to an existing animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="target">The target position.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation LocalMoveTo(this IAnimation animation, Vector3 target)
		{
			return Attach(animation, new Animations.LocalMoveTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Creates a RotateTo animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="target">The target orientation.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation RotateTo(this IEntity entity, Quaternion target)
		{
			return Attach(entity, new Animations.RotateTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Chains a RotateTo animation to an existing animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="target">The target orientation.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation RotateTo(this IAnimation animation, Quaternion target)
		{
			return Attach(animation, new Animations.RotateTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Creates a LocalRotateTo animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="target">The target orientation.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation LocalRotateTo(this IEntity entity, Quaternion target)
		{
			return Attach(entity, new Animations.LocalRotateTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Chains a LocalRotateTo animation to an existing animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="target">The target orientation.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation LocalRotateTo(this IAnimation animation, Quaternion target)
		{
			return Attach(animation, new Animations.LocalRotateTo(target) { Duration = 1.Seconds() });
		}

		/// <summary>
		/// Creates a Wait animation.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="duration">The time to wait.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation Wait(this IEntity entity, TimeSpan duration)
		{
			return duration.TotalMilliseconds > 0.0
					   ? Attach(entity, new Animations.Wait { Duration = duration })
				       : Attach(entity, null);
		}

		/// <summary>
		/// Chains a Wait animation to an existing animation.
		/// </summary>
		/// <param name="animation">The previous animation.</param>
		/// <param name="duration">The time to wait.</param>
		/// <returns>The created animation.</returns>
		public static IAnimation Wait(this IAnimation animation, TimeSpan duration)
		{
			return duration.TotalMilliseconds > 0.0
				       ? Attach(animation, new Animations.Wait { Duration = duration })
				       : animation;
		}

		#endregion
	}
}
