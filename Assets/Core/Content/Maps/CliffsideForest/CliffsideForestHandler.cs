using System;
using Cysharp.Threading.Tasks;
using Fusion;
using Rewired;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    public class CliffsideForestHandler : MapHandler
    {
        public PlayableDirector playableDirector;
        public PlayableAsset playable;

        public GameObject[] objsToDisable;

        public bool cutsceneSkip;

        private void Update()
        {
            if (ReInput.controllers.GetAnyButtonDown()) cutsceneSkip = true;
        }

        public override async UniTask DoPreMatch(GameModeBase gamemode)
        {
            RPC_PlayIntroCutscene();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayIntroCutscene()
        {
            _ = PlayIntroCutscene();
        }

        public async UniTask PlayIntroCutscene()
        {
            playableDirector.Play(playable);
            
            await UniTask.WaitUntil(() => playableDirector.time >= playable.duration == true || cutsceneSkip);
            foreach (var go in objsToDisable)
            {
                go.SetActive(false);
            }
            GameModeBase.singleton.GamemodeState = GameModeState.MATCH_IN_PROGRESS;
        }
    }
}