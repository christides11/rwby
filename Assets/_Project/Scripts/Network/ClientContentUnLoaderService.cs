using UnityEngine;
using Fusion;
using System.Linq;

namespace rwby
{
    public class ClientContentUnLoaderService : NetworkBehaviour
    {
        public void TellClientsToUnload(ModGUIDContentReference contentReference)
        {
            bool unloadResult = ContentManager.singleton.UnloadContentDefinition(contentReference);

            if(!unloadResult) Debug.LogError($"Error unloading {contentReference.ToString()}");
            
            if (Runner.ActivePlayers.Count() == 0)
            {
                Debug.LogError("No active players.");
                return;
            }
            
            // Tell clients to load the content.
            RPC_ClientTryUnload(new NetworkModObjectGUIDReference(contentReference.modGUID, contentReference.contentType, contentReference.contentIdx));
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_ClientTryUnload(NetworkModObjectGUIDReference objectReference)
        {
            if (Runner.IsServer && !Runner.LocalPlayer) return;
            ContentManager.singleton.UnloadContentDefinition(objectReference);
        }
    }
}