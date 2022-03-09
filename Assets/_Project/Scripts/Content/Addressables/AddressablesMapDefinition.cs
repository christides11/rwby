using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

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

        [NonSerialized] private AsyncOperationHandle<SceneInstance>[] sceneHandles;

        public override async UniTask<bool> Load()
        {
            sceneHandles = new AsyncOperationHandle<SceneInstance>[sceneReferences.Length];
            return true;
        }

        public override async UniTask LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            for(int i = 0; i < sceneReferences.Length; i++)
            {
                sceneHandles[i] = Addressables.LoadSceneAsync(sceneReferences[i], loadMode);
                await sceneHandles[i];
            }
        }
        
        public override List<string> GetSceneNames()
        {
            List<string> sList = new List<string>();
            for (int i = 0; i < sceneHandles.Length; i++)
            {
                if (sceneHandles[i].Status != AsyncOperationStatus.Succeeded) continue;
                sList.Add(sceneHandles[i].Result.Scene.name);
                Debug.Log($"Returning {sceneHandles[i].Result.Scene.name}");
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