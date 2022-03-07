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

            if (Input.GetKeyDown(KeyCode.F3))
            {
                QualitySettings.vSyncCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                QualitySettings.vSyncCount = 1;
            }
            
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Application.targetFrameRate = 0;
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Application.targetFrameRate = 60;
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