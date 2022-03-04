using Cysharp.Threading.Tasks;
using Rewired.UI.ControlMapper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            if (Input.GetKeyDown(KeyCode.F5))
            {
                QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
            }
        }

        public virtual string[] GetSceneNames(CustomSceneRef sceneReference){
            if (sceneReference.source == 0)
            {
                return new string[] { SceneManager.GetSceneByBuildIndex(sceneReference.sceneIndex).name };
            }

            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(new ModObjectReference((sceneReference.source, sceneReference.modIdentifier), sceneReference.sceneIndex));

            return mapDefinition.GetSceneNames().ToArray();
        }
    }
}