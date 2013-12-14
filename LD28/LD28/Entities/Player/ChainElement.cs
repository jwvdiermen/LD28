using System;
using FarseerPhysics.Dynamics;
using LD28.Entity;
using LD28.Scene;
using LD28.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Entities.Player
{
	public class ChainElement : PhysicalEntityBase
	{
		private World _physicsWorld;

		private PayloadChain _payloadChain;
		private int _index;

		private ChainElement(string name)
			: base(name)
		{
		}

		public static ChainElement CreateFor(PayloadChain payloadChain, int index, DynamicEntityWorld world, Body body)
		{
			var el = new ChainElement(payloadChain.Name + "_" + index);
			el._payloadChain = payloadChain;
			el._index = index;
			el.World = world;
			el.Body = body;
			return el;
		}

		public override void LoadContent(IServiceProvider services)
		{
			base.LoadContent(services);
			var content = (ContentManager)services.GetService(typeof(ContentManager));

			var texture = content.Load<Texture2D>(@"Sprites\Chain");
			var sprite = new StaticSprite(new Vector2(1.0f), texture, Name + "_Sprite");

			SceneNode.Attach(sprite);
		}

		protected override void OnWorldChanged(IEntityWorld world)
		{
			DestroyPhysics();
		}

		protected override ISceneNode CreateSceneNode()
		{
			return World.Scene.CreateSceneNode(Name);
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
