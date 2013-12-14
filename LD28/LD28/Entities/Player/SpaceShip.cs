using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LD28.Core;
using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Player
{
	public class SpaceShip : PhysicalEntityBase
	{
		private World _physicsWorld;

		private readonly float _maxSpeed;

		private Vector2 _force = Vector2.Zero;
		private float _torque = 0.0f;

		public SpaceShip(string name = "Player", float maxSpeed = 500.0f)
			: base(name)
		{
			_maxSpeed = maxSpeed;
		}

		public override void UpdatePhysics(GameTime gameTime, ICamera camera)
		{
			base.UpdatePhysics(gameTime, camera);

			// Apply the force and torque.
			Body.ApplyForce(ref _force);
			_force = Vector2.Zero;

			Body.ApplyTorque(_torque);
			_torque = 0.0f;
		}

		/// <summary>
		/// Adds thurst in the given direction.
		/// </summary>
		/// <param name="dir">The direction.</param>
		public void Thrust(Vector2 dir)
		{
			// Rotate the direction.
			var rotatedForce = new Vector2(
				dir.X * MathCore.Cos(Body.Rotation) + -dir.Y * MathCore.Sin(Body.Rotation),
				dir.Y * MathCore.Cos(Body.Rotation) + dir.X * MathCore.Sin(Body.Rotation));

			// Add the thrust as acceleration.
			var velocity = Body.LinearVelocity;

			if (velocity != Vector2.Zero)
			{
				var currentSpeed = velocity.Length();

				var velocityDir = Vector2.Normalize(velocity);
				var forceDir = Vector2.Normalize(rotatedForce);

				float dotProduct;
				Vector2.Dot(ref velocityDir, ref forceDir, out dotProduct);

				var damping = 1.0f - (1.0f / _maxSpeed * currentSpeed) * dotProduct;

				_force += rotatedForce * damping;
			}
			else
			{
				_force += rotatedForce;
			}
		}

		/// <summary>
		/// Adds rotation.
		/// </summary>
		/// <param name="acc">The angular acceleration.</param>
		public void Rotate(float acc)
		{
			var velocity = this.Body.AngularVelocity;

			if ((velocity < 0.0f && acc < 0.0f) ||
				(velocity > 0.0f && acc > 0.0f))
			{
				var currentSpeed = Math.Abs(velocity);
				var maxSpeed = MathHelper.PiOver2 * 1.5f;

				var damping = 1.0f - (1.0f / maxSpeed * currentSpeed);
				_torque += acc * damping;
			}
			else
			{
				_torque += acc;
			}
		}

		public override void LoadContent(IServiceProvider services)
		{
			base.LoadContent(services);
		}

		public override void UnloadContent()
		{
			base.UnloadContent();
		}

		protected override void OnWorldChanged(IEntityWorld world)
		{
			DestroyPhysics();

			if (world != null)
			{
				_physicsWorld = ((DynamicEntityWorld)world).PhysicsWorld;
				InitializePhysics();
			}
			else
			{
				_physicsWorld = null;
			}
		}

		protected override ISceneNode CreateSceneNode()
		{
			return World.Scene.CreateSceneNode(Name);
		}

		private void InitializePhysics()
		{
			Body = BodyFactory.CreateRectangle(_physicsWorld, 2.0f, 6.0f, 5.0f);
			Body.BodyType = BodyType.Dynamic;
		}

		private void DestroyPhysics()
		{
			if (Body != null)
			{
				Body.Dispose();
				Body = null;
			}
		}
	}
}
