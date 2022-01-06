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

            RPC_ClientTryLoad(mapReference.ToString());

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
        private void RPC_ClientTryLoad(string mapReferenceString)
        {
            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(new ModObjectReference(mapReferenceString));
            _ = ClientTryLoad(mapDefinition);
        }

        private async UniTask ClientTryLoad(IMapDefinition mapDefinition)
        {
            await mapDefinition.LoadMap(UnityEngine.SceneManagement.LoadSceneMode.Additive);
            ClientManager.local.mapLoadPercent = 1.0f;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(mapDefinition.SceneName));
        }
    }
}