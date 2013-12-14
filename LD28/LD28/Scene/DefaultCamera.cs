using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD28.Scene
{
	/// <summary>
	/// This class implements the <see cref="ICamera" /> interface.
	/// </summary>
	public class DefaultCamera : Movable, ICamera
	{
		private Matrix _projectMatrix;
		private Viewport _viewport;

		public DefaultCamera(RenderTarget2D renderTarget, Viewport viewport)
		{
			RenderTarget = renderTarget;
			Viewport = viewport;
		}

		public RenderTarget2D RenderTarget { get; private set; }

		public Viewport Viewport
		{
			get { return _viewport;  }
			set
			{
				_viewport = value;

				float ratio = _viewport.Width / (float)_viewport.Height;
				ScreenSize = new Vector2(ratio * 100.0f, 100.0f);

				float halfWidth = ScreenSize.X / 2.0f;
				float halfHeight = ScreenSize.Y / 2.0f;

				Matrix.CreateOrthographicOffCenter(-halfWidth, halfWidth, halfHeight, -halfHeight, -100.0f, 100.0f, out _projectMatrix);
			}
		}

		public Vector2 ScreenSize { get; private set; }

		public Matrix Transformation
		{
			get
			{
				Matrix projectionMatrix, viewMatrix;
				GetMatrices(out projectionMatrix, out viewMatrix);

				return projectionMatrix * viewMatrix;
			}
		}

		public void GetMatrices(out Matrix projectionMatrix, out Matrix viewMatrix)
		{
			projectionMatrix = _projectMatrix;

			if (SceneNode != null)
			{
				var transformation = SceneNode.Transformation;

				viewMatrix = Matrix.Identity;
				viewMatrix.M11 = transformation.M11;
				viewMatrix.M12 = transformation.M21;
				viewMatrix.M13 = transformation.M31;

				viewMatrix.M21 = transformation.M12;
				viewMatrix.M22 = transformation.M22;
				viewMatrix.M23 = transformation.M32;

				viewMatrix.M31 = transformation.M13;
				viewMatrix.M32 = transformation.M23;
				viewMatrix.M33 = transformation.M33;

				var inverseW = 1.0f / (transformation.M14 + transformation.M24 + transformation.M34 + transformation.M44);
				viewMatrix.M41 = -((viewMatrix.M11 * transformation.M41) + (viewMatrix.M21 * transformation.M42) + (viewMatrix.M31 * transformation.M43)) * inverseW;
				viewMatrix.M42 = -((viewMatrix.M12 * transformation.M41) + (viewMatrix.M22 * transformation.M42) + (viewMatrix.M32 * transformation.M43)) * inverseW;
				viewMatrix.M43 = -((viewMatrix.M13 * transformation.M41) + (viewMatrix.M23 * transformation.M42) + (viewMatrix.M33 * transformation.M43)) * inverseW;
			}
			else
			{
				viewMatrix = Matrix.Identity;
			}
		}

		public void Apply(GraphicsDevice graphics)
		{
			graphics.SetRenderTarget(RenderTarget);
			graphics.Viewport = Viewport;
		}
	}
}