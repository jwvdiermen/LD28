namespace LD28.Animation
{
	/// <summary>
	/// Used for reporting when an animation has been started.
	/// </summary>
	/// <param name="animation">The animation.</param>
	public delegate void AnimationStarted(IAnimation animation);

	/// <summary>
	/// Used for reporting animation progress.
	/// </summary>
	/// <param name="animation">The animation.</param>
	/// <param name="progress">The progress of the animation, a value between 0 and 1.</param>
	public delegate void AnimationProgressEventHandler(IAnimation animation, float progress);

	/// <summary>
	/// Used for reporting when an animation has been finished.
	/// </summary>
	/// <param name="animation">The animation.</param>
	public delegate void AnimationFinished(IAnimation animation);
}
