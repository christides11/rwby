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

        // Teleport
        public float teleportHeight = 5;
        public float teleportDist = 3;
        public float teleportGravity = 20;
        public Vector3 teleportLockoffOffset;

        // Gundash
        [Networked] public int GundashCount { get; set; }

        public override async UniTask<bool> OnFighterLoaded()
        {
            bool globalSnbkLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Soundbank, new ModObjectReference("core", "global"));
            if(globalSnbkLoadResult == false)
            {
                return false;
            }

            bool rubyRoseSnbkLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Soundbank, new ModObjectReference("core", "rubyrose"));
            if (rubyRoseSnbkLoadResult == false)
            {
                return false;
            }

            bool globalEffectbankLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Effectbank, new ModObjectReference("core", "global"));
            if (globalEffectbankLoadResult == false)
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

            ISoundbankDefinition rubyRoseSnbk = (ISoundbankDefinition)ContentManager.instance.GetContentDefinition(ContentType.Soundbank, new ModObjectReference("core", "rubyrose"));
            if (rubyRoseSnbk == null)
            {
                Debug.LogError("Error loading global soundbank.");
            }
            soundbankContainer.AddSoundbank(rubyRoseSnbk.Name, rubyRoseSnbk);

            IEffectbankDefinition globalEffectbank = (IEffectbankDefinition)ContentManager.instance.GetContentDefinition(ContentType.Effectbank, new ModObjectReference("core", "global"));
            if (globalEffectbank == null)
            {
                Debug.LogError("Error loading global effectbank.");
            }
            effectbankContainer.AddEffectbank(globalEffectbank.Name, globalEffectbank);
        }

        protected override void SetupStates()
        {
            stateManager.AddState(new RRTeleport(), (ushort)RubyRoseStates.TELEPORT);
            stateManager.AddState(new RRGunDashGround(), (ushort)RubyRoseStates.GUN_DASH_GROUND);
            base.SetupStates();
        }
    }
}