using rwby.menus;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Selectable = rwby.ui.Selectable;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour
    {
        [System.Serializable]
        public struct CSSConnection
        {
            public Selectable cssSelectable;
            public ModObjectReference characterReference;
        }
        
        public int playerID;
        
        public Canvas canvas;

        public GameObject defaultSelectedObject;
        
        private LobbyMenuHandler lobbyMenuHandler;

        [Header("Content")] 
        public Selectable readyButton;
        public Selectable profileButton;
        public Selectable spectateButton;
        public Selectable exitButton;
        public Selectable topBar;
        public Transform characterContentTransform;
        public GameObject characterContentPrefab;
        public GameObject characterSelectMenu;
        public GameObject characterSelectBigCharacter;
        public CSSConnection[] cssConnections;

        public void Initialize(LobbyMenuHandler menuHandler)
        {
            this.lobbyMenuHandler = menuHandler;
            if (NetworkManager.singleton.FusionLauncher.NetworkRunner.IsServer)
            {
                readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Match";
            }
            else
            {
                readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
            }
            exitButton.onSubmit.AddListener(() => { menuHandler.ExitLobby(); });

            for (int i = 0; i < cssConnections.Length; i++)
            {
                cssConnections[i].cssSelectable.onSubmit.AddListener(() => { SetCharacter(cssConnections[i].characterReference); });
            }
            
            ResetCharacterList();
            //GameObject cAdd = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
            //cAdd.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            //cAdd.GetComponent<Selectable>().onSubmit.AddListener(() => { OpenCharacterSelect(0); });
        }

        public void ResetCharacterList()
        {
            foreach(Transform child in characterContentTransform)
            {
                Destroy(child.gameObject);
            }
            
            ClientManager cm = ClientManager.local;
            for (int i = 0; i < cm.ClientPlayers[playerID].characterReferences.Count; i++)
            {
                GameObject chara = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
                chara.GetComponentInChildren<TextMeshProUGUI>().text = "?";
            }
                         
            GameObject cAdd = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
            cAdd.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            cAdd.GetComponent<Selectable>().onSubmit.AddListener(() => { TryAddCharacter(); });
        }

        void TryAddCharacter()
        {
            ClientManager cm = ClientManager.local;
            cm.CLIENT_SetPlayerCharacterCount(playerID, cm.ClientPlayers[playerID].characterReferences.Count+1);
        }
        
        public int currentSelectingCharacter = 0;
        public void OpenCharacterSelect(int playerCharacterIndex)
        {
            currentSelectingCharacter = playerCharacterIndex;
            characterSelectMenu.SetActive(true);
        }

        public void OpenCustomCharacterSelect()
        {
            
        }
        
        public void SetCharacter(ModObjectReference characterReference)
        {
            characterSelectMenu.SetActive(false);
        }
        
        public void Cleanup()
        {
            
        }
    }
}