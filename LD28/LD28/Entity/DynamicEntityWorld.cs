using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This class implements the <see cref="IEntityWorld" /> interface and provides a dynamic entity world
	/// with physics using FarseerPhysics.
	/// </summary>
	public class DynamicEntityWorld : StaticEntityWorld
	{
		private readonly World _physicsWorld;
		private readonly List<IPhysicalEntity> _physicalEntityList = new List<IPhysicalEntity>();

		/// <summary>
		/// Gets the physics world.
		/// </summary>
		public World PhysicsWorld
		{
			get { return _physicsWorld; }
		}

		/// <summary>
		/// This constructor creates a new static entity world using a default scene (<see cref="DefaultScene" />).
		/// </summary>
		/// <param name="services">The service provider.</param>
		public DynamicEntityWorld(IServiceProvider services)
			: this(services, new DefaultScene())
		{
		}

		/// <summary>
		/// This constructor creates a new static entity world using the given scene.
		/// </summary>
		/// <param name="services">The service provider.</param>
		/// <param name="scene">The scene to use.</param>
		public DynamicEntityWorld(IServiceProvider services, IScene scene)
			: base(services, scene)
		{
			// Create the physics world.
			_physicsWorld = new World(Vector2.Zero);
		}

		public override void Add(IEntity entity)
		{
			base.Add(entity);

			lock (_physicalEntityList)
			{
				var physicalEntity = entity as IPhysicalEntity;
				if (physicalEntity != null && _physicalEntityList.Contains(physicalEntity) == false)
				{
					_physicalEntityList.Add(physicalEntity);
				}
			}
		}

		public override void Remove(IEntity entity)
		{
			base.Remove(entity);

			lock (_physicalEntityList)
			{
				var physicalEntity = entity as IPhysicalEntity;
				if (physicalEntity != null && _physicalEntityList.Contains(physicalEntity))
				{
					_physicalEntityList.Remove(physicalEntity);
				}
			}
		}

		public override void Update(GameTime time, ICamera camera)
		{
			// Update the physical properties of the entities.
			lock (_physicalEntityList)
			{
				foreach (var entity in _physicalEntityList)
				{
					entity.UpdatePhysics(time, camera);
				}
			}

			// Update the physics.
			_physicsWorld.Step((float)time.ElapsedGameTime.TotalSeconds);

			base.Update(time, camera);
		}
	}
}
