using System;
using LD28.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LD28
{
	/// <summary>
	/// Used for showing debug information.
	/// </summary>
	public class DebugInfo : DisposableObject
	{
		private GraphicsDevice _graphics;
		private ContentManager _content;
		private SpriteBatch _spriteBatch;
		private SpriteFont _font;

		public void LoadContent(IServiceProvider services)
		{
			// Get the necessary services.
			_graphics = ((IGraphicsDeviceService)services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
			_content = (ContentManager)services.GetService(typeof(ContentManager));

			// Load the content.
			_spriteBatch = new SpriteBatch(_graphics);
			_font = _content.Load<SpriteFont>(@"Fonts\Debug");
		}

		public void UnloadContent()
		{
			if (_spriteBatch != null)
			{
				_spriteBatch.Dispose();
				_spriteBatch = null;
			}
			_font = null;
		}

		public void Render(GameTime gameTime)
		{
			if (_spriteBatch != null)
			{
				var pp = _graphics.PresentationParameters;
				var linePosition = new Vector2(10.0f, 10.0f);

				_spriteBatch.Begin();
				_spriteBatch.DrawString(_font, String.Format("Width: {0}, Height: {1}", pp.BackBufferWidth, pp.BackBufferHeight), linePosition, Color.White);
				_spriteBatch.End();
			}
		}

		protected override void DisposeUnmanaged()
		{
			UnloadContent();
		}
	}
}
