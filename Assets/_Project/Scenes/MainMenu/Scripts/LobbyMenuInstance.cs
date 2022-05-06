using Fusion;
using rwby.ui.mainmenu;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer ? "Start Match" : "Ready";
            if (lobbyMenuHandler.sessionManagerGamemode.Runner.IsServer)
            {
                readyButton.GetComponent<Selectable>().onSubmit.AddListener(async () => await lobbyMenuHandler.StartMatch());
            }
            exitButton.onSubmit.AddListener(menuHandler.ExitLobby);

            for (int i = 0; i < cssConnections.Length; i++)
            {
                int temp = i;
                cssConnections[i].cssSelectable.onSubmit.AddListener(() => { SetCharacter(cssConnections[temp].characterReference); });
            }
            
            ResetCharacterList();
        }

        public void ResetCharacterList()
        {
            foreach(Transform child in characterContentTransform)
            {
                Destroy(child.gameObject);
            }

            PlayerRef localPlayerRef = lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            if (clientInfo.players.Count <= playerID) return;
            ClientManager cm = lobbyMenuHandler.sessionManagerGamemode.Runner.GetPlayerObject(localPlayerRef).GetComponent<ClientManager>();

            for (int i = 0; i < clientInfo.players[playerID].characterReferences.Count; i++)
            {
                GameObject chara = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
                chara.GetComponentInChildren<TextMeshProUGUI>().text = "?";
                if (clientInfo.players[playerID].characterReferences[i].IsValid())
                {
                    IFighterDefinition fighterDefinition = ContentManager.singleton.GetContentDefinition<IFighterDefinition>(clientInfo.players[playerID].characterReferences[i]);
                    chara.GetComponentInChildren<TextMeshProUGUI>().text = fighterDefinition.Name;
                }
                int selectIndex = i;
                chara.GetComponent<Selectable>().onSubmit.AddListener(() => {OpenCharacterSelect(selectIndex);});
            }
            
            if (clientInfo.players[playerID].characterReferences.Count == lobbyMenuHandler.sessionManagerGamemode.GetTeamDefinition(clientInfo.players[playerID].team).maxCharactersPerPlayer) return;
            GameObject cAdd = GameObject.Instantiate(characterContentPrefab, characterContentTransform, false);
            cAdd.GetComponentInChildren<TextMeshProUGUI>().text = "+";
            cAdd.GetComponent<Selectable>().onSubmit.AddListener(TryAddCharacter);
        }

        void TryAddCharacter()
        {
            PlayerRef localPlayerRef = lobbyMenuHandler.sessionManagerGamemode.Runner.LocalPlayer;
            var clientInfo = lobbyMenuHandler.sessionManagerGamemode.GetClientInformation(localPlayerRef);
            if (clientInfo.clientRef.IsValid == false) return;
            lobbyMenuHandler.sessionManagerGamemode.CLIENT_SetPlayerCharacterCount(playerID, clientInfo.players[playerID].characterReferences.Count+1);
        }
        
        private int currentSelectingCharacterIndex = 0;
        public void OpenCharacterSelect(int playerCharacterIndex)
        {
            currentSelectingCharacterIndex = playerCharacterIndex;
            characterSelectMenu.SetActive(true);
        }

        public void OpenCustomCharacterSelect()
        {
            
        }
        
        public void SetCharacter(ModObjectReference characterReference)
        {
            characterSelectMenu.SetActive(false);
            lobbyMenuHandler.sessionManagerGamemode.CLIENT_SetPlayerCharacter(playerID, currentSelectingCharacterIndex, characterReference);
        }
        
        public void Cleanup()
        {
            
        }
    }
}