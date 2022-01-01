using Cysharp.Threading.Tasks;
using Rewired.UI.ControlMapper;
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
        [SerializeField] private ControlMapper cMapper;

        public Settings settings;
        public string localUsername;

        public string currentMapSceneName;

        public async UniTask Initialize()
        {
            singleton = this;
            await modLoader.Initialize();
            contentManager.Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (cMapper.isOpen)
                {
                    cMapper.Close(false);
                }
                else
                {
                    cMapper.Open();
                }
            }
        }

        public virtual async UniTask<bool> LoadMap(ModObjectReference map)
        {
            return false;
            /*
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
            return true;*/
        }
    }
}