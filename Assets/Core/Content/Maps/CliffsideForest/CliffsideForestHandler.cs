using System;
using System.Collections.Generic;
using System.Linq;
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

        public Camera animatedCamera;
        public GameObject[] objsToDisable;

        public bool cutsceneSkip;

        public HashSet<int> playersFinished = new HashSet<int>();

        private void Update()
        {
            if (ReInput.controllers.GetAnyButtonDown()) cutsceneSkip = true;
        }

        public override async UniTask DoPreMatch(GameModeBase gamemode)
        {
            RPC_PlayIntroCutscene();
            
            while (playersFinished.Count < Runner.ActivePlayers.Count())
            {
                await UniTask.WaitForFixedUpdate();
            }
            
            RPC_EndIntroCutscene();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_PlayIntroCutscene()
        {
            _ = PlayIntroCutscene();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_EndIntroCutscene()
        {
            playableDirector.Stop();
            foreach (var go in objsToDisable)
            {
                go.SetActive(false);
            }
        }

        public async UniTask PlayIntroCutscene()
        {
            playableDirector.Play(playable);
            
            await UniTask.WaitUntil(() => playableDirector.time >= playable.duration == true || cutsceneSkip);
            RPC_ReportCutsceneFinished();
        }
        
        [Rpc(RpcSources.All, RpcTargets.InputAuthority | RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_ReportCutsceneFinished(RpcInfo info = default)
        {
            playersFinished.Add(info.Source.PlayerId);
        }
    }
}