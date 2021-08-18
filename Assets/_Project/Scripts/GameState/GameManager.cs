using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager singleton;

        [SerializeField] private ModLoader modLoader;
        [SerializeField] private ContentManager contentManager;

        public Settings settings;
        public string localUsername;

        public string currentMapSceneName;

        public void Initialize()
        {
            singleton = this;
            modLoader.Initialize();
            contentManager.Initialize();
            modLoader.loadedMods.Add("core", new LoadedModDefinition(null, settings.baseMod));
        }

        public virtual async UniTask<bool> LoadMap(ModObjectReference map)
        {
            await contentManager.LoadContentDefinitions(ContentType.Map, map.modIdentifier);
            IMapDefinition mapDefinition = (IMapDefinition)contentManager.GetContentDefinition(ContentType.Map, map);

            if (mapDefinition == null)
            {
                Debug.Log($"Can not find map {map.ToString()}.");
                return false;
            }

            bool result = await contentManager.LoadMap(map);
            if (!result)
            {
                Debug.Log($"Error loading map {map.ToString()}.");
                return false;
            }

            currentMapSceneName = mapDefinition.SceneName;
            return true;
        }
    }
}