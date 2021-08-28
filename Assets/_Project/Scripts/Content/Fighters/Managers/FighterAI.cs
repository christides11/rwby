using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    [OrderBefore(typeof(FighterInputManager))]
    public class FighterAI : NetworkBehaviour
    {
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }
    }
}