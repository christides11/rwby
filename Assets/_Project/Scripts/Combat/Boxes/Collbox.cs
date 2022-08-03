using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class Collbox : CustomHitbox
    {
        public BoxCollider boxCollider;

        public override void SetBoxActiveState(bool state)
        {
            base.SetBoxActiveState(state);
            //boxCollider.enabled = state;
        }

        public override void SetBoxSize(Vector3 offset, Vector3 boxExtents)
        {
            base.SetBoxSize(offset, boxExtents);
            boxCollider.size = boxExtents * 2.0f;
        }
    }
}