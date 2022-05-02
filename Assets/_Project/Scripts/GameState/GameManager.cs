using System;
using Cysharp.Threading.Tasks;
using Rewired.UI.ControlMapper;
using Rewired;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager singleton;

        [SerializeField] private ModLoader modLoader;
        public ContentManager contentManager;
        public ControlMapper cMapper;
        public LocalPlayerManager localPlayerManager;
        public ControllerAssignmentMenu controllerAssignmentMenu;
        public LoadingMenu loadingMenu;
        public ProfilesManager profilesManager;
        public NetworkManager networkManager;

        public Settings settings;

        public async UniTask Initialize()
        {
            singleton = this;
            await modLoader.Initialize();
            contentManager.Initialize();
            localPlayerManager.Initialize();
            profilesManager.Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                QualitySettings.vSyncCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                QualitySettings.vSyncCount = 1;
            }

            /*
            if (Input.GetKeyDown(KeyCode.F4))
            {
                var player = ReInput.players.GetPlayer(0);
                
                // Disable all Rule Sets
                foreach(var ruleSet in player.controllers.maps.layoutManager.ruleSets) {
                    ruleSet.enabled = false;
                }
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "k&m_default").enabled = true;
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "js_default").enabled = true;
                player.controllers.maps.layoutManager.Apply();
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var player = ReInput.players.GetPlayer(0);
                
                // Disable all Rule Sets
                foreach(var ruleSet in player.controllers.maps.layoutManager.ruleSets) {
                    ruleSet.enabled = false;
                }

                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "k&m_keyboard").enabled = true;
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "js_default").enabled = true;
                player.controllers.maps.layoutManager.Apply();
            }*/
        }

        public virtual async UniTask<Scene> LoadScene(CustomSceneRef sceneReference, LoadSceneParameters parameters)
        {
            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(new ModObjectReference((sceneReference.source, sceneReference.modIdentifier), sceneReference.mapIdentifier));

            var result = await mapDefinition.LoadScene(sceneReference.sceneIdentifier, parameters);
            return result;
        }

        public virtual string[] GetSceneNames(CustomSceneRef sceneReference){
            if (sceneReference.source == 0)
            {
                return new string[] { SceneManager.GetSceneByBuildIndex(sceneReference.mapIdentifier).name };
            }

            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(new ModObjectReference((sceneReference.source, sceneReference.modIdentifier), sceneReference.mapIdentifier));

            return mapDefinition.GetSceneNames().ToArray();
        }

        public virtual string GetSceneName(CustomSceneRef sceneReference)
        {
            if (sceneReference.source == 0)
            {
                return SceneManager.GetSceneByBuildIndex(sceneReference.sceneIdentifier).name;
            }
            
            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(new ModObjectReference((sceneReference.source, sceneReference.modIdentifier), sceneReference.mapIdentifier));

            return mapDefinition.GetSceneNames()[sceneReference.sceneIdentifier];
        }
    }
}