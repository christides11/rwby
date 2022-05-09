using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using rwby;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class CustomNetworkSceneManager : CustomNetworkSceneManagerBase
    {
        [Header("Single Peer Options")] public int PostLoadDelayFrames = 1;
        
        public Dictionary<CustomSceneRef, Scene> scenesLoaded = new Dictionary<CustomSceneRef, Scene>();

        protected override IEnumerator SwitchScene(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes, FinishedLoadingDelegate finished)
        {
            return UniTask.ToCoroutine( async () => await UpdateScenesMultiPeer(oldScenes, newScenes, finished));
            /*
            if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                return UpdateScenesSinglePeer(oldScenes, newScenes, finished);
            }
            else
            {
                return UniTask.ToCoroutine( async () => await UpdateScenesMultiPeer(oldScenes, newScenes, finished));
            }*/
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
            
            for (int i = PostLoadDelayFrames; i > 0; --i)
            {
                yield return null;
            }

            finished(sceneObjects);
        }
        
        protected virtual async UniTask UpdateScenesMultiPeer(List<CustomSceneRef> oldScenes,
            List<CustomSceneRef> newScenes,
            FinishedLoadingDelegate finished)
        {
            //Debug.Log($"Updating Scenes Multi Peer: {oldScenes.Count} to {newScenes.Count}");
            List<NetworkObject> sceneObjects = new List<NetworkObject>();

            var loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, NetworkProjectConfig.ConvertPhysicsMode(Runner.Config.PhysicsEngine));
            
            var sceneToUnload = Runner.MultiplePeerUnityScene;
            var tempSceneSpawnedPrefabs = Runner.IsMultiplePeerSceneTemp ? sceneToUnload.GetRootGameObjects() : Array.Empty<GameObject>();
            
            // Old temp scene, unload it & create a new one.
            if (sceneToUnload.IsValid())
            {
                if (Runner.TryMultiplePeerAssignTempScene())
                {
                    LogTrace($"Unloading previous temp scene: {sceneToUnload}, temp scene created");
                    await SceneManager.UnloadSceneAsync(sceneToUnload);
                }
            }
            
            // UNLOAD //
            var keys = scenesLoaded.Keys.ToList();
            for (int i = keys.Count-1; i >= 0; i--)
            {
                if (newScenes.Contains(keys[i]))
                {
                    newScenes.RemoveAt(newScenes.IndexOf(keys[i]));
                    continue;
                }
                await SceneManager.UnloadSceneAsync(scenesLoaded[keys[i]]);
                scenesLoaded.Remove(keys[i]);
            }


            LogTrace($"Loading scenes with parameters: {JsonUtility.ToJson(loadSceneParameters)}");
            
            var tempScene = Runner.MultiplePeerUnityScene;

            // LOAD //
            //Scene firstLoadedScene = default;
            for (int i = 0; i < newScenes.Count; i++)
            {
                Scene loadedScene = await LoadSceneAsync(newScenes[i], loadSceneParameters);
                if (i == 0) Runner.MultiplePeerUnityScene = loadedScene;

                if (!loadedScene.IsValid())
                {
                    throw new InvalidOperationException($"Failed to load scene {newScenes[i].ToString()}: async op failed");
                }
                
                sceneObjects.AddRange(FindNetworkObjects(loadedScene, disable: true, addVisibilityNodes: true));
                
                // Temp scene spawned prefabs.
                if (tempScene.IsValid() && tempSceneSpawnedPrefabs.Length > 0)
                {
                    LogTrace(
                        $"Temp scene has {tempSceneSpawnedPrefabs.Length} spawned prefabs, need to move them to the loaded scene.");
                    foreach (var go in tempSceneSpawnedPrefabs)
                    {
                        Assert.Check(go.GetComponent<NetworkObject>(),
                            $"Expected {nameof(NetworkObject)} on a GameObject spawned on the temp scene {tempScene.name}");
                        SceneManager.MoveGameObjectToScene(go, loadedScene);
                    }
                }
            }
            
            Debug.Log($"Multiple Peer US: {Runner.MultiplePeerUnityScene.name}");
            //Runner.MultiplePeerUnityScene = firstLoadedScene;
            
            LogTrace($"Loaded scenes with parameters: {JsonUtility.ToJson(loadSceneParameters)}");
            
            // unload temp scene
            if (tempScene.IsValid())
            {
                LogTrace($"Unloading temp scene {tempScene}");
                await SceneManager.UnloadSceneAsync(tempScene);
            }

            finished(sceneObjects);
        }
        
        protected virtual async UniTask<Scene> LoadSceneAsync(CustomSceneRef sceneRef, LoadSceneParameters parameters)
        {
            var scene = await GameManager.singleton.LoadScene(sceneRef, parameters);
            return scene;
        }

        protected virtual YieldInstruction UnloadSceneAsync(Scene scene)
        {
            return SceneManager.UnloadSceneAsync(scene);
        }

    }
}