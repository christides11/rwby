using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class AnimationEffect : BaseEffect
    {
        public Animation animation;
        public AnimationClip clip;

        public override void SetFrame(float time)
        {
            base.SetFrame(time);
        }
    }
}
