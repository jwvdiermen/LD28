/*
 *  EntityControllerBase.cs
 *  CalcuRush by Crealuz - LD28.Entity
 *
 *  © copyright 2010 Crealuz All rights reserved.
 *  The computer program(s) is the proprietary information of the original authors. 
 *  and provided under the relevant License Agreement containing restrictions 
 *  on use and disclosure. Use is subject to the License Agreement.
 *
 *  No part of this package may be reproduced and/or published by print, 
 *  photoprint, microfilm, audiotape, electronically, mechanically or any other means, 
 *  or stored in an information retrieval system, without prior permission from Crealuz.
 * 
 *  @original authors: jwvdiermen
 */

using LD28.Core;
using LD28.Scene;
using Microsoft.Xna.Framework;

namespace LD28.Entity
{
	/// <summary>
	/// This abstract class provides a base implementation of the <see cref="IEntityController" /> interface.
	/// </summary>
	public abstract class EntityControllerBase : DisposableObject, IEntityController
	{
		#region Methods

		/// <summary>
		/// This method is called when the controller is attached to the given entity.
		/// </summary>
		/// <param name="entity">The entity to which the controller was attached.</param>
		protected virtual void OnAttached(IEntity entity)
		{
		}

		/// <summary>
		/// This method is called when the controller has been detached from the current entity.
		/// </summary>
		/// <param name="entity">The entity from which the controller was detached.</param>
		protected virtual void OnDetached(IEntity entity)
		{
		}

		#endregion

		#region IEntityController Members

		public IEntity Entity
		{
			get;
			private set;
		}

		public void AttachTo(IEntity entity)
		{
			if (Entity != entity)
			{
				Detach();

				// Set the entity first, because calling Attach on the entity will also call
				// the AttachTo method on the controller. Setting this property first will prevent a
				// circular method call thanks to the previous if statement.
				Entity = entity;

				Entity.Attach(this);
				OnAttached(entity);
			}
		}

		public void Detach()
		{
			if (Entity != null)
			{
				// Reset the entity first, because calling Detach on the entity will also call
				// the Detach method on the controller. Setting this property to null first will prevent a
				// circular method call thanks to the previous if statement.
				var oldEntity = Entity;
				Entity = null;

				oldEntity.Detach(this);
				OnDetached(oldEntity);
			}
		}

		public virtual void Update(GameTime gameTime, ICamera camera)
		{
		}

		#endregion
	}
}
