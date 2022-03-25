using Cysharp.Threading.Tasks;
using Rewired.UI.ControlMapper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HnSF.Input;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager singleton;

        [SerializeField] private ModLoader modLoader;
        public ContentManager contentManager;
        [SerializeField] private ControlMapper cMapper;
        public LocalPlayerManager localPlayerManager;
        public ControllerAssignmentMenu controllerAssignmentMenu;

        public Settings settings;
        public string localUsername;

        public string currentMapSceneName;

        public async UniTask Initialize()
        {
            singleton = this;
            await modLoader.Initialize();
            contentManager.Initialize();
            localPlayerManager.Initialize();
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

            if (Input.GetKeyDown(KeyCode.F2))
            {
                QualitySettings.vSyncCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                QualitySettings.vSyncCount = 1;
            }

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