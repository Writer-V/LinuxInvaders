using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Graphics
{
    public class AnimationSet
    {
        public Texture2D Texture {get;}
        private readonly Dictionary<AnimSequenceType,AnimationClip> clips = new();
        public bool HasAnimType(AnimSequenceType type) => clips.ContainsKey(type);
        public readonly int Rows;
        public AnimSequenceType DefaultType {get;}
        public readonly int FrameWidth;
        public int FrameHeight => Texture.Height / Rows;
        public AnimationSet(Texture2D texture, int frameWidth, AnimSequenceType defaultType, 
            params (AnimSequenceType type, int frameCount, int fps, bool loop)[] rows)
        {
            Texture = texture;
            Rows = rows.Length;
            FrameWidth = frameWidth;
            DefaultType = defaultType;
            for(int i = 0; i < rows.Length; i++)
            {
                clips.Add(rows[i].type, new AnimationClip(texture, rows[i].frameCount, frameWidth, i, frameHeight: FrameHeight, 
                    framesPerSec: rows[i].fps, loop: rows[i].loop));
            }
            //Using with to reflect existing ones to create bonus sequences.
            if (clips.ContainsKey(AnimSequenceType.MoveLeft) && !clips.ContainsKey(AnimSequenceType.MoveRight))
                clips.Add(AnimSequenceType.MoveRight, clips[AnimSequenceType.MoveLeft] with {ApplyEffect = SpriteEffects.FlipHorizontally});
            else if (!clips.ContainsKey(AnimSequenceType.MoveLeft) && clips.ContainsKey(AnimSequenceType.MoveRight))
                clips.Add(AnimSequenceType.MoveLeft, clips[AnimSequenceType.MoveRight] with {ApplyEffect = SpriteEffects.FlipHorizontally});
            if (clips.ContainsKey(AnimSequenceType.FlyingUp) && !clips.ContainsKey(AnimSequenceType.FlyingDown))
                clips.Add(AnimSequenceType.FlyingDown, clips[AnimSequenceType.FlyingUp] with { ApplyEffect = SpriteEffects.FlipVertically});
            else if (!clips.ContainsKey(AnimSequenceType.FlyingUp) && clips.ContainsKey(AnimSequenceType.FlyingDown))
                clips.Add(AnimSequenceType.FlyingUp, clips[AnimSequenceType.FlyingDown] with { ApplyEffect = SpriteEffects.FlipVertically});
            if(!clips.ContainsKey(defaultType)) throw new ArgumentException("AnimationSet doesn't include default sequence");
        }
        public AnimationClip BestClipFor(AnimSequenceType type)
        {
            AnimationClip clipToReturn;
            if(clips.TryGetValue(type,out clipToReturn)) return clipToReturn; //The easy way...
            switch (type)
            {
                //Sometimes a specific fallback makes sense
                case AnimSequenceType.MoveLeft:
                    if(clips.TryGetValue(AnimSequenceType.FlyingUp, out clipToReturn)) return clipToReturn;
                    else if(clips.TryGetValue(AnimSequenceType.Idle, out clipToReturn)) return clipToReturn;
                    else return clips[DefaultType];
                case AnimSequenceType.Death:
                    if(clips.TryGetValue(AnimSequenceType.Hit, out clipToReturn)) return clipToReturn;
                    else return clips[DefaultType];
                default:
                    return clips[DefaultType];
            }
        }
    }
}