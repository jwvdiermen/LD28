using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LD28.Entity;
using LD28.Movables;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Player
{
	public class Payload : PhysicalEntityBase
	{
		private World _physicsWorld;

		private ISceneNode _outerRing;
		private float _outerRingRotation = 0.0f;
		private ISceneNode _innerRing;
		private float _innerRingRotation = 0.0f;

		public Payload(string name = "Payload")
			: base(name)
		{
		}

		public override void UpdatePhysics(GameTime gameTime, ICamera camera)
		{
			base.UpdatePhysics(gameTime, camera);
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
			var sceneNode = World.Scene.CreateSceneNode(Name);

			var innerSphere = sceneNode.CreateChild("InnerSphere");
			innerSphere.Attach(new SpherePrimitive(1.0f, 16));

			_outerRing = sceneNode.CreateChild("OuterRing");
			_outerRing.Attach(new TorusPrimitive(2.5f, 0.25f, 16));

			_innerRing = _outerRing.CreateChild("OuterRing");
			_innerRing.Attach(new TorusPrimitive(2.25f, 0.25f, 16));

			return sceneNode;
		}

		private void InitializePhysics()
		{
			Body = BodyFactory.CreateCircle(_physicsWorld, 2.5f, 1.0f);
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

		public override void Update(GameTime gameTime, ICamera camera)
		{
			base.Update(gameTime, camera);
			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			_innerRingRotation += 5.0f * dt;
			_innerRing.LocalOrientation = Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, _innerRingRotation);

			_outerRingRotation += 5.0f * dt;
			_outerRing.LocalOrientation = Quaternion.CreateFromYawPitchRoll(0.0f, _innerRingRotation, 0.0f);
		}
	}
}
