using Cysharp.Threading.Tasks;
using Fusion;
using rwby.fighters.states;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core.content
{
    public class RubyRoseManager : FighterManager
    {
        [Networked] public Vector3 TeleportPosition { get; set; }

        public float teleportHeight = 5;
        public float teleportDist = 3;
        public float teleportGravity = 20;
        public Vector3 teleportLockoffOffset;

        public override async UniTask<bool> OnFighterLoaded()
        {
            bool globalSnbkLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Soundbank, new ModObjectReference("core", "global"));
            if(globalSnbkLoadResult == false)
            {
                return false;
            }
            return true;
        }

        public override void Awake()
        {
            base.Awake();
            ISoundbankDefinition globalSnbk = (ISoundbankDefinition)ContentManager.instance.GetContentDefinition(ContentType.Soundbank, new ModObjectReference("core", "global"));
            if (globalSnbk == null)
            {
                Debug.LogError("Error loading global soundbank.");
            }

            soundbankContainer.AddSoundbank(globalSnbk.Name, globalSnbk);
        }

        protected override void SetupStates()
        {
            stateManager.AddState(new RRTeleport(), (ushort)RubyRoseStates.TELEPORT);
            base.SetupStates();
        }
    }
}