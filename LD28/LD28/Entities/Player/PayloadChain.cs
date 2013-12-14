using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using LD28.Core;
using LD28.Entity;
using Microsoft.Xna.Framework;

namespace LD28.Entities.Player
{
	public class PayloadChain : INamable
	{
		public string Name { get; private set; }
		public ChainElement[] Elements { get; private set; }

		private PayloadChain(string name)
		{
			Name = name;
		}

		public static PayloadChain Connect(SpaceShip spaceShip, Payload payload, Vector2 payloadTarget)
		{
			var dynamicWorld = (DynamicEntityWorld)spaceShip.World;
			var world = dynamicWorld.PhysicsWorld;

			AABB spaceShipSize;
			spaceShip.Body.FixtureList.First().GetAABB(out spaceShipSize, 0);

			AABB payloadSize;
			payload.Body.FixtureList.First().GetAABB(out payloadSize, 0);

			var chainSize = new Vector2(1.0f);
			var chainSizeSingle = Math.Max(chainSize.X, chainSize.Y);

			var start = new Vector2(spaceShip.Position.X - spaceShipSize.Extents.Y, spaceShip.Position.Y);
			var length = payloadTarget.Length();
			var targetVector = payloadTarget / length;
			var chainVector = targetVector * chainSizeSingle;
			start += chainVector;
			var chainCount = (int)Math.Ceiling(length / chainSizeSingle);

			var lastBody = spaceShip.Body;
			var chain = new PayloadChain(spaceShip.Name + "_Chain");
			var elements = new List<ChainElement>();

			for (var i = 0; i < chainCount; i++)
			{
				var chainBody = BodyFactory.CreateBody(world, start + i * chainVector);
				chainBody.BodyType = BodyType.Dynamic;

				var chainFixture = FixtureFactory.AttachRectangle(chainSize.X, chainSize.Y, 0.1f, Vector2.Zero, chainBody);
				chainFixture.CollidesWith = Category.Cat2;

				JointFactory.CreateRevoluteJoint(world, lastBody, chainBody, -chainVector / 2.0f);
				lastBody = chainBody;

				elements.Add(ChainElement.CreateFor(chain, elements.Count, dynamicWorld, chainBody));
			}

			payload.Position = new Vector3(start + chainCount * chainVector + targetVector * 2.5f, 0.0f);
			JointFactory.CreateRevoluteJoint(world, lastBody, payload.Body, new Vector2(0.0f, -2.5f));

			var ropeJoin = new RopeJoint(spaceShip.Body, payload.Body, new Vector2(0.0f, 3.0f), new Vector2(0.0f, -2.5f));
			ropeJoin.CollideConnected = true;
			world.AddJoint(ropeJoin);


			return chain;
		}
	}
}
