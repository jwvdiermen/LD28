using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Player
{
	public class Payload : PhysicalEntityBase
	{
		private World _physicsWorld;

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
			return World.Scene.CreateSceneNode(Name);
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
	}
}
