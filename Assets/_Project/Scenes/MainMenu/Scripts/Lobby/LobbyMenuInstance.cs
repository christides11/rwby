using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour, IMenuHandler
    {
        public int playerID;
        
        public Canvas canvas;
        
        public LobbyMenuHandler lobbyMenuHandler;

        [Header("Menus")] 
        [SerializeField] private LobbyMenu lobbyMenu;
        [SerializeField] private CharacterSelectMenu characterSelectMenu;

        [SerializeField] private List<int> history = new List<int>();
        public Dictionary<int, MenuBase> menus = new Dictionary<int, MenuBase>();

        private LocalPlayerManager localPlayerManager;
        private EventSystem eventSystem;
        
        public void Initialize(LobbyMenuHandler menuHandler)
        {
            menus.Add((int)LobbyMenuType.LOBBY, lobbyMenu);
            menus.Add((int)LobbyMenuType.CHARACTER_SELECT, characterSelectMenu);
            history.Add((int)LobbyMenuType.LOBBY);
            
            foreach (var menu in menus.Values)
            {
                menu.TryClose(MenuDirection.BACKWARDS, true);
            }
            menus[(int)LobbyMenuType.LOBBY].Open(MenuDirection.FORWARDS, this);
            
            this.lobbyMenuHandler = menuHandler;
            
            localPlayerManager = GameManager.singleton.localPlayerManager;
            eventSystem = EventSystem.current;
        }

        public void Refresh()
        {
            lobbyMenu.Refresh();
            characterSelectMenu.Refresh();
        }

        /*
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

            profileButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Profile: {cm.profiles[playerID]}";
            
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
        
        public void SetCharacter(ModGUIDContentReference characterContentReference)
        {
            characterSelectMenu.SetActive(false);
            lobbyMenuHandler.sessionManagerGamemode.CLIENT_SetPlayerCharacter(playerID, currentSelectingCharacterIndex, characterContentReference);
        }*/
        
        public void Cleanup()
        {
            
        }
        
        public bool Forward(int menu, bool autoClose = true)
        {
            if (!menus.ContainsKey(menu)) return false;
            EventSystem.current.SetSelectedGameObject(null);
            if (autoClose) GetCurrentMenu().TryClose(MenuDirection.FORWARDS, true);
            menus[menu].Open(MenuDirection.FORWARDS, this);
            history.Add(menu);
            return true;
        }

        public bool Back()
        {
            if (history.Count <= 1) return false;
            bool result = GetCurrentMenu().TryClose(MenuDirection.BACKWARDS);
            if(result == true) history.RemoveAt(history.Count-1);
            GetCurrentMenu().Open(MenuDirection.BACKWARDS, this);
            return result;
        }

        public IList GetHistory()
        {
            return history;
        }

        public IMenu GetCurrentMenu()
        {
            if (history.Count == 0) return null;
            return menus[history[^1]];
        }
    }
}