using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class MapHandler : NetworkBehaviour
    {
        public virtual async UniTask Initialize(GameModeBase gamemode)
        {
            
        }

        public virtual async UniTask DoPreMatch(GameModeBase gamemode)
        {
            
        }

        public virtual void Teardown(GameModeBase gamemode)
        {
            
        }
    }
}