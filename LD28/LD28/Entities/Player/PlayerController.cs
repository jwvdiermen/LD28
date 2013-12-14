using System;
using LD28.Entity;
using LD28.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD28.Entities.Player
{
	public class PlayerController : EntityControllerBase
	{
		private SpaceShip _spaceShip;

		public bool Enabled { get; set; }

		public PlayerController()
		{
			Enabled = true;
		}

		protected override void OnAttached(IEntity entity)
		{
			_spaceShip = entity as SpaceShip;
			if (_spaceShip == null)
			{
				throw new Exception("PlayerController can only be attached to SpaceShip entity.");
			}
		}

		protected override void OnDetached(IEntity entity)
		{
			_spaceShip = null;
		}

		public override void Update(GameTime gameTime, ICamera camera)
		{
			if (_spaceShip == null || !Enabled)
			{
				return;
			}

			var keyboard = Keyboard.GetState();
			if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
			{
				_spaceShip.Thrust(Vector2.UnitY * -2500.0f);
			}
			if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
			{
				_spaceShip.Thrust(Vector2.UnitY * 2500.0f);
			}

			if (keyboard.IsKeyDown(Keys.E))
			{
				_spaceShip.Thrust(Vector2.UnitX * 2000.0f);
			}
			if (keyboard.IsKeyDown(Keys.Q))
			{
				_spaceShip.Thrust(Vector2.UnitX * -2000.0f);
			}

			if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
			{
				_spaceShip.Rotate(2500.0f);
			}
			if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
			{
				_spaceShip.Rotate(-2500.0f);
			}
		}
	}
}
