using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UMod;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    [CreateAssetMenu(fileName = "UModMapDefinition", menuName = "rwby/Content/UMod/MapDefinition")]
    public class UModMapDefinition : IMapDefinition, IUModModHostRef
    {
        public ModHost modHost { get; set; } = null;
        public override string Name { get { return mapName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string mapName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable;

        [SerializeField] private string[] scenes;

        [NonSerialized] private ModAsyncOperation[] sceneHandles;

        public override async UniTask<bool> Load()
        {
            sceneHandles = new ModAsyncOperation[scenes.Length];
            return true;
        }

        public override async UniTask LoadMap(LoadSceneMode loadMode)
        {
            throw new System.NotImplementedException();
        }

        public override async UniTask<Scene> LoadScene(int sceneIndex, LoadSceneParameters parameters)
        {
            sceneHandles[sceneIndex] = modHost.Scenes.LoadAsync(scenes[sceneIndex], true);
            await sceneHandles[sceneIndex];
            if (!sceneHandles[sceneIndex].IsSuccessful) return default;
            return SceneManager.GetSceneByName(scenes[sceneIndex]);
        }
        
        public override List<string> GetSceneNames()
        {
            List<string> sList = new List<string>();
            for (int i = 0; i < sceneHandles.Length; i++)
            {
                if (!sceneHandles[i].IsSuccessful) continue;
                sList.Add(scenes[i] + ".copy");
            }
            return sList;
        }

        public override async UniTask UnloadScene(int sceneIndex)
        {
            throw new System.NotImplementedException();
        }
        
        public override async UniTask UnloadMap()
        {
            var sNames = GetSceneNames();
            for (int i = 0; i < sNames.Count; i++)
            {
                await SceneManager.UnloadSceneAsync(sNames[i]);
            }
        }
        
        public override bool Unload()
        {
            UnloadMap();
            sceneHandles = null;
            return true;
        }
    }
}