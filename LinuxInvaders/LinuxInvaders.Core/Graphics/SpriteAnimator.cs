using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Graphics
{
	public class SpriteAnimator
	{
		private enum PlaybackMode { Play, PlayOnceResume, PlayOnceHold, Stopped }
		private readonly AnimationSet animationSet;
		private AnimationClip clip; // Current clip from the set.
		// Total amount of time the animation has been running.
		private float totalElapsed;
		private int currentFrame = 0;
		public int FrameWidth => animationSet.FrameWidth;
		public int FrameHeight => animationSet.FrameHeight;
		public bool HasAnimType(AnimSequenceType t) => animationSet.HasAnimType(t);
		// Intended AnimSequenceType, even if actual clip needs a fallback
		private AnimSequenceType baseSequence;
		private PlaybackMode state;
		private bool paused = false;
        // For getting the sprite with complicated parameters.
		public float Rotation { get; set; }
		public float Scale { get; set; }
		public float Depth { get; set; }
		public bool IsLoopingAnim => clip.Loop;
		private Vector2 origin; //For rotation
		public event EventHandler  FinishedAnim;
		public SpriteAnimator(AnimationSet animationSet, float rotation = 0f, float depth = 0f, float scale = 1f,
			bool centerOrigin = false, AnimSequenceType initialAnimType = AnimSequenceType.Default)
		{
			this.animationSet = animationSet;
			Rotation = rotation;
            Depth = depth;
			Scale = scale;
			baseSequence = initialAnimType;
			clip = animationSet.BestClipFor(baseSequence);
            if (centerOrigin) //For sprites that need to turn
				origin = new Vector2(animationSet.FrameWidth / 2f, animationSet.FrameHeight / 2f);
			state = PlaybackMode.Play;
		}
		public void Update(float elapsed)
		{
			if (paused)
				return;
			totalElapsed += elapsed;
			while (totalElapsed > clip.TimePerFrame)
			{
				totalElapsed -= clip.TimePerFrame;
				if(currentFrame < (clip.Frames-1))
					currentFrame++;
				else
				{
					switch (state)
					{
						case PlaybackMode.Play:
							if(clip.Loop) currentFrame = 0;
							break;
						case PlaybackMode.PlayOnceResume:
							clip = animationSet.BestClipFor(baseSequence);
							Reset();
							state = PlaybackMode.Play;
							FinishedAnim?.Invoke(this, EventArgs.Empty);
							break;
						case PlaybackMode.PlayOnceHold:
							state = PlaybackMode.Stopped;
							FinishedAnim?.Invoke(this, EventArgs.Empty);
							break;
					}
				}
			}
		}
		public void Draw(SpriteBatch batch, Vector2 pos)
		{
			batch.Draw(animationSet.Texture, pos, clip.FrameRectangle(currentFrame), Color.White, Rotation, origin, Scale, clip.ApplyEffect, Depth);
		}
		public void Play(AnimSequenceType type)
		{
			if(type == baseSequence && state == PlaybackMode.Play) return; //Already has the right animation. Otherwise set what to return to when not in PlayOnce.
			if(state == PlaybackMode.Stopped) state = PlaybackMode.Play;
			if(state == PlaybackMode.Play || state == PlaybackMode.Stopped) Reset();
			baseSequence = type;
			if(state == PlaybackMode.Play) clip = animationSet.BestClipFor(baseSequence);
		}
		public void PlayOnce(AnimSequenceType type, bool resume = false)
		{
			clip = animationSet.BestClipFor(type);
			Reset();
			if(resume) state = PlaybackMode.PlayOnceResume;
			else state = PlaybackMode.PlayOnceHold;
		}
		public void Reset()
		{
			currentFrame = 0;
			totalElapsed = 0f;
		}

		public void RewindAndStop()
		{
			Pause();
			Reset();
		}

		public void Pause()
		{
			paused = true;
		}
		public void Resume()
		{
			paused = false;
		}
	}
}
