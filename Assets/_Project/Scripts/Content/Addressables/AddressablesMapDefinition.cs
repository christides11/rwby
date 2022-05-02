using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesMapDefinition", menuName = "rwby/Content/Addressables/MapDefinition")]
    public class AddressablesMapDefinition : IMapDefinition
    {
        public override string Name { get { return mapName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string mapName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable;

        [SerializeField] private AssetReference[] sceneReferences;

        [NonSerialized] private AsyncOperationHandle<SceneLoadData>[] sceneHandles;

        public override async UniTask<bool> Load()
        {
            sceneHandles = new AsyncOperationHandle<SceneLoadData>[sceneReferences.Length];
            return true;
        }

        public override async UniTask LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            for(int i = 0; i < sceneReferences.Length; i++)
            {
                sceneHandles[i] = AddressablesSceneLoader.LoadSceneAsync(sceneReferences[i], new LoadSceneParameters(loadMode)); //Addressables.LoadSceneAsync(sceneReferences[i], loadMode);
                await sceneHandles[i];
            }
        }

        public override async UniTask<Scene> LoadScene(int sceneIndex, LoadSceneParameters parameters)
        {
            sceneHandles[sceneIndex] = AddressablesSceneLoader.LoadSceneAsync(sceneReferences[sceneIndex], parameters);
            await sceneHandles[sceneIndex];
            return sceneHandles[sceneIndex].Result.scene;
        }

        public override UniTask UnloadScene(int sceneIndex)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetSceneNames()
        {
            List<string> sList = new List<string>();
            for (int i = 0; i < sceneHandles.Length; i++)
            {
                if (sceneHandles[i].Status != AsyncOperationStatus.Succeeded) continue;
                sList.Add(sceneHandles[i].Result.scene.name);
                Debug.Log($"Returning {sceneHandles[i].Result.scene.name}");
            }
            return sList;
        }

        public override async UniTask UnloadMap()
        {
            for (int i = 0; i < sceneHandles.Length; i++)
            {
                if (sceneHandles[i].Status == AsyncOperationStatus.Succeeded) Addressables.UnloadSceneAsync(sceneHandles[i]);
            }
        }

        public override bool Unload()
        {
            foreach (var handle in sceneHandles)
            {
                if(handle.Status == AsyncOperationStatus.Succeeded) Addressables.Release(handle);
            }
            return true;
        }
    }
}