namespace LD28.Animation
{
	/// <summary>
	/// The different animation states.
	/// </summary>
	public enum AnimationState
	{
		/// <summary>
		/// The animation is not playing.
		/// </summary>
		Stopped,

		/// <summary>
		/// The animation is playing.
		/// </summary>
		Playing,

		/// <summary>
		/// The animation is playing, but has been paused.
		/// </summary>
		Paused,

		/// <summary>
		/// The animation has finished playing.
		/// </summary>
		Finished
	}
}
