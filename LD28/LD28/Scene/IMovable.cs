using LD28.Core;

namespace LD28.Scene
{
	/// <summary>
	/// This interface represents a movable object which can be attached to an <see cref="ISceneNode" />.
	/// Optionally, the movable can be assigned a name.
	/// </summary>
	[UsedImplicitly]
	public interface IMovable : INamable
	{
		#region Properties

		/// <summary>
		/// Gets the scene node to which the movable is attached.
		/// </summary>
		ISceneNode SceneNode
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// This method attaches the movable to the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node to attach the movable to.</param>
		/// <remarks>If the movable is already attached to another scene node, it is first detached.</remarks>
		void AttachTo(ISceneNode sceneNode);

		/// <summary>
		/// This method detaches the movable from the current scene node.
		/// </summary>
		void Detach();

		#endregion
	}
}
