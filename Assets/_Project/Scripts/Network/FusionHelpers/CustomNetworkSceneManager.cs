using System.Collections;
using System.Collections.Generic;
using Fusion;
using rwby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class CustomNetworkSceneManager : CustomNetworkSceneManagerBase
    {
        protected override IEnumerator SwitchScene(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes, FinishedLoadingDelegate finished)
        {
            return UpdateScenesSinglePeer(oldScenes, newScenes, finished);
        }

        protected virtual IEnumerator UpdateScenesSinglePeer(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes,
            FinishedLoadingDelegate finished)
        {
            Debug.Log($"Updating Scenes Single Peer: {oldScenes.Count} to {newScenes.Count}");
            List<NetworkObject> sceneObjects = new List<NetworkObject>();

            for (int p = 0; p < oldScenes.Count; p++)
            {
                // Still loaded.
                if (newScenes.Contains(oldScenes[p])) continue;
                
                // Unload scene(s).
                string[] sceneNames = GameManager.singleton.GetSceneNames(oldScenes[p]);
                for (int j = 0; j < sceneNames.Length; j++)
                {
                    var unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sceneNames[j]), UnloadSceneOptions.None);
                    while (unloadOp.isDone == false)
                    {
                        yield return null;
                    }
                }
            }
            
            for (int i = 0; i < newScenes.Count; i++)
            {
                // Already loaded before.
                if (oldScenes.Contains(newScenes[i])) continue;
                
                // Get scene objects.
                string[] sceneNames = GameManager.singleton.GetSceneNames(newScenes[i]);
                for (int j = 0; j < sceneNames.Length; j++)
                {
                    sceneObjects.AddRange(FindNetworkObjects(SceneManager.GetSceneByName(sceneNames[j]), disable: true));
                }
            }
            
            for (int i = 1; i > 0; --i)
            {
                yield return null;
            }

            finished(sceneObjects);
        }
    }
}