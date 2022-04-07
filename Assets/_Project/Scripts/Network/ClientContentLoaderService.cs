using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace rwby
{
    public class ClientContentLoaderService : NetworkBehaviour
    {
        public enum ClientLoadResultType
        {
            NONE = 0,
            FAILED = 1,
            SUCCESS = 2
        }

        public struct ClientLoadRequestTracker
        {
            public PlayerRef client;
            public ClientLoadResultType result;
        }

        public float timeoutTime = 5.0f;
        public int loadRequestCounter = 0;

        public Dictionary<int, List<ClientLoadRequestTracker>> loadRequests = new Dictionary<int, List<ClientLoadRequestTracker>>();

        public async UniTask<List<PlayerRef>> TellClientsToLoad<T>(ModObjectReference objectReference) where T : IContentDefinition
        {
            bool localLoadResult = await ContentManager.singleton.LoadContentDefinition<T>(objectReference);
            if (localLoadResult == false) return null;
            bool localContentLoadResult = await ContentManager.singleton.GetContentDefinition<T>(objectReference).Load();
            if(localContentLoadResult == false) return null;

            int loadRequestNumber = loadRequestCounter;
            loadRequestCounter++;
            loadRequests.Add(loadRequestNumber, new List<ClientLoadRequestTracker>());
            // Tell clients to load the content.
            RPC_ClientTryLoad(loadRequestNumber, typeof(T).ToString(), objectReference);

            // Wait until all other clients report their results, or until the timeout period.
            float startTime = Time.realtimeSinceStartup;
            while (loadRequests[loadRequestNumber].Count < Runner.ActivePlayers.Count())
            {
                await UniTask.WaitForFixedUpdate();
                if ((Time.realtimeSinceStartup - startTime) >= timeoutTime) break;
            }

            // Record the clients that didn't report back.
            List<PlayerRef> failedToLoadClients = new List<PlayerRef>();
            foreach(PlayerRef r in Runner.ActivePlayers){
                ClientLoadRequestTracker clr = loadRequests[loadRequestNumber].FirstOrDefault(x => x.client == r);
                if(clr.result != ClientLoadResultType.SUCCESS)
                {
                    Debug.Log($"CLIENT FAILURE: result = {clr.result}, number = {loadRequestNumber}, client = {r.PlayerId}");
                    failedToLoadClients.Add(r);
                }
            }
            loadRequests.Remove(loadRequestNumber);
            return failedToLoadClients;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_ClientTryLoad(int requestNumber, string typeName, ModObjectReference objectReference)
        {
            Type typeAsType = Type.GetType(typeName);
            if (typeAsType == null)
            {
                Debug.LogError("Received invalid type.");
                RPC_ClientReportLoadResult(requestNumber, false);
                return;
            }
            _ = ClientTryLoad(requestNumber, typeAsType, objectReference);
        }

        
        public async UniTask ClientTryLoad(int requestNumber, Type contentType, ModObjectReference objectReference)
        {
            bool loadResult = await ContentManager.singleton.LoadContentDefinition(contentType, objectReference);
            if(loadResult == true)
            {
                loadResult = await ContentManager.singleton.GetContentDefinition(contentType, objectReference).Load();
                if(loadResult == false) Debug.LogError($"Error loading {objectReference.ToString()} content.");
            }
            else
            {
                Debug.LogError($"Error loading {objectReference.ToString()}.");
            }
            // Need to wait a bit before sending an RPC directly back. Gets ignored otherwise.
            await UniTask.WaitForFixedUpdate();
            RPC_ClientReportLoadResult(requestNumber, loadResult);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_ClientReportLoadResult(int requestNumber, NetworkBool loadResult, RpcInfo info = default)
        {
            if (loadRequests.ContainsKey(requestNumber) == false)
            {
                Debug.Log($"Request number {requestNumber} doesn't exist.");
                return;
            }
            ClientLoadRequestTracker request = new ClientLoadRequestTracker
            {
                client = info.Source, result = loadResult ? ClientLoadResultType.SUCCESS : ClientLoadResultType.FAILED
            };
            
            loadRequests[requestNumber].Add(request);
        }
    }
}