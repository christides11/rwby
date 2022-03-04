using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class ClientMapLoaderService : NetworkBehaviour
    {
        public bool loadInProgress = false;

        public async UniTask TellClientsToLoad(ModObjectReference mapReference)
        {
            if (loadInProgress) return;
            loadInProgress = true;

            foreach(ClientManager c in ClientManager.clientManagers)
            {
                c.mapLoadPercent = 0;
            }

            RPC_ClientTryLoad(mapReference);

            float loadResult = 0.0f;
            while(loadResult < 1.0f)
            {
                loadResult = ClientManager.clientManagers[0].mapLoadPercent;
                foreach(ClientManager client in ClientManager.clientManagers)
                {
                    loadResult = Mathf.Min(loadResult, client.mapLoadPercent);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), ignoreTimeScale: true);
            }

            loadInProgress = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ClientTryLoad(ModObjectReference mapReference)
        {
            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapReference);
            _ = ClientTryLoad(mapDefinition);
        }

        private async UniTask ClientTryLoad(IMapDefinition mapDefinition)
        {
            await mapDefinition.LoadMap(UnityEngine.SceneManagement.LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(mapDefinition.GetSceneNames()[0]));
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), ignoreTimeScale: true);
            RPC_ReportLoadPercentage(1.0f);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_ReportLoadPercentage(float percentage, RpcInfo info = default)
        {
            Runner.GetPlayerObject(info.Source).GetBehaviour<ClientManager>().mapLoadPercent = percentage;
        }
    }
}