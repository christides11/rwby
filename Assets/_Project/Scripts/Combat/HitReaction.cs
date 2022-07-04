using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class HitReaction : HnSF.Combat.HitReactionBase
    {
        public HitReactionType reaction;
        public HitInfo.HitInfoGroup hitInfoGroup;
    }
}