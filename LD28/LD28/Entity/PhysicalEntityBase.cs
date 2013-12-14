using FarseerPhysics.Dynamics;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This abstract class provides a base implementation of the <see cref="IPhysicalEntity" /> interface.
	/// </summary>
	public abstract class PhysicalEntityBase : EntityBase, IPhysicalEntity
	{
		#region Constructors

		/// <summary>
		/// This constructor creates a new instance of a physical entity.
		/// </summary>
		/// <param name="name">The optional name.</param>
		protected PhysicalEntityBase(string name = null)
			: base(name)
		{
		}

		#endregion

		private Body _body;

		private Vector3? _position;
		private Quaternion? _orientation;

		public override Vector3 Position
		{
			get { return _body != null ? new Vector3(_body.Position, 0.0f) : _position.GetValueOrDefault(Vector3.Zero); }
			set
			{
				if (_body != null)
				{
					_body.Position = new Vector2(value.X, value.Y);
				}
				else
				{
					_position = value;
				}
			}
		}

		public override Quaternion Orientation
		{
			get { return _body != null ? Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _body.Rotation) : _orientation.GetValueOrDefault(Quaternion.Identity); }
			set 
			{
				if (_body != null)
				{
					var q = value;
					q.X = 0;
					q.Y = 0;
					q.Normalize();

					_body.Rotation = (float)(2.0 * System.Math.Acos(q.W));
				}
				else
				{
					_orientation = value;
				}
			}
		}

		public override bool IsActive
		{
			get { return _body.Awake; }
			set
			{
				if (_body != null)
				{
					_body.Awake = value;
				}
			}
		}

		public override void Update(GameTime gameTime, ICamera camera)
		{
			base.Update(gameTime, camera);

			var currentPosition = SceneNode.Position;
			var newPosition = Body.Position;
			currentPosition.X = newPosition.X;
			currentPosition.Y = newPosition.Y;
			SceneNode.Position = currentPosition;

			SceneNode.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _body.Rotation);
		}

		public Body Body
		{
			get { return _body; }
			protected set
			{
				_body = value;
				if (_position != null)
				{
					var position = _position.Value;
					_body.Position = new Vector2(position.X, position.Y);
					_position = null;
				}
				if (_orientation != null)
				{
					var q = _orientation.Value;
					q.X = 0;
					q.Y = 0;
					q.Normalize();

					_body.Rotation = (float)(2.0 * System.Math.Acos(q.W));

					_orientation = null;
				}
			}
		}

		public virtual void UpdatePhysics(GameTime gameTime, ICamera camera)
		{
		}
	}
}
