using System.Collections;
using System.Collections.Generic;
using Fusion;
using rwby;
using UnityEngine;

namespace rwby
{
    public class CustomNetworkSceneManager : CustomNetworkSceneManagerBase
    {
        protected override IEnumerator SwitchScene(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes, FinishedLoadingDelegate finished)
        {
            return UpdateScenesSinglePeer(oldScenes, newScenes, finished);
            /*
            if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                return SwitchScenesSinglePeer(oldScenes, newScenes, finished);
            }
            else
            {
                throw new System.NotImplementedException();
                //return SwitchSceneMultiplePeer(prevScene, newScene, finished);
            }
            */
        }

        protected virtual IEnumerator UpdateScenesSinglePeer(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes,
            FinishedLoadingDelegate finished)
        {
            List<NetworkObject> sceneObjects = new List<NetworkObject>();

            for (int i = 0; i < newScenes.Count; i++)
            {
                // Already loaded before.
                if (oldScenes.Contains(newScenes[i])) continue;

                //ContentManager.singleton.GetContentDefinition<IMapDefinition>();
            }
            
            for (int i = 1; i > 0; --i)
            {
                yield return null;
            }

            finished(sceneObjects);
            /*
            var sceneObjects = FindNetworkObjects(loadedScene, disable: true);
            finished(sceneObjects);*/
        }
    }
}