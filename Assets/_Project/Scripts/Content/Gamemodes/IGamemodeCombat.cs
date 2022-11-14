using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public interface IGamemodeCombat
    {
        public bool IsHitHurtboxValid(CustomHitbox attackerHitbox, Hurtbox attackeeHurtbox);
        public bool IsHitHitboxValid(CustomHitbox attackerHitbox, CustomHitbox attackeeHitbox);
    }
}