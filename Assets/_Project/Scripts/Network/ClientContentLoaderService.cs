using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;
using System.Threading;
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

        // TODO: Tuple with load failure reason.
        public async UniTask<List<PlayerRef>> TellClientsToLoad<T>(ModGUIDContentReference contentReference, bool loadContent = true) where T : IContentDefinition
        {
            bool localLoadResult = await ContentManager.singleton.LoadContentDefinition(contentReference);
            if (localLoadResult == false)
            {
                Debug.LogError($"Load Error: {contentReference.ToString()}");
                return null;
            }
            bool localContentLoadResult = await ContentManager.singleton.GetContentDefinition<T>(contentReference).Load();
            if (localContentLoadResult == false)
            {
                Debug.LogError($"Get Error: {contentReference.ToString()}");
                return null;
            }

            if (Runner.ActivePlayers.Count() == 0)
            {
                Debug.LogError("No active players.");
                return new List<PlayerRef>();
            }

            int loadRequestNumber = loadRequestCounter;
            loadRequestCounter++;
            loadRequests.Add(loadRequestNumber, new List<ClientLoadRequestTracker>());
            // Tell clients to load the content.
            RPC_ClientTryLoad(loadRequestNumber, new NetworkModObjectGUIDReference(contentReference.modGUID, contentReference.contentType, contentReference.contentIdx), loadContent);

            // Wait until all other clients report their results, or until the timeout period.
            float startTime = Time.realtimeSinceStartup;
            
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeoutTime));

            try
            {
                await UniTask.WaitUntil(() => loadRequests[loadRequestNumber].Count >= Runner.ActivePlayers.Count(),
                    cancellationToken: cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.LogError("Timeout waiting for client(s) to report load result(s).");
                }
            }
            Debug.Log($"CHECK: {loadRequests[loadRequestNumber].Count} : {Runner.ActivePlayers.Count()} : {loadRequests[loadRequestNumber].Count >= Runner.ActivePlayers.Count()} : {Time.realtimeSinceStartup-startTime}");
            /*while (loadRequests[loadRequestNumber].Count < Runner.ActivePlayers.Count())
            {
                await UniTask.WaitForFixedUpdate();
                if ((Time.realtimeSinceStartup - startTime) >= timeoutTime) break;
            }*/

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
        public void RPC_ClientTryLoad(int requestNumber, NetworkModObjectGUIDReference objectReference, NetworkBool loadContent)
        {
            if (Runner.IsServer && !Runner.LocalPlayer) return;
            _ = ClientTryLoad(requestNumber, objectReference, loadContent);
        }

        
        public async UniTask ClientTryLoad(int requestNumber, NetworkModObjectGUIDReference objectReference, NetworkBool loadContent)
        {
            bool loadResult = await ContentManager.singleton.LoadContentDefinition(objectReference);
            if(loadResult == true)
            {
                if (loadContent)
                {
                    loadResult = await ContentManager.singleton.GetContentDefinition(objectReference).Load();
                    if (loadResult == false) Debug.LogError($"Error loading {objectReference.ToString()} content.");
                }
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