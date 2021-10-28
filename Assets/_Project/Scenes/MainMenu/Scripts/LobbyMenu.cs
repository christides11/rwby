using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace rwby.menus
{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private Transform playerList;

        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private GameObject playerListContentItem;

        [SerializeField] private GameObject selectablePrefab;
        [SerializeField] private Transform selectableContentHolder;

        public Button characterSelectButton;

        public ContentSelect contentSelect;

        private bool clearInProgress = false;

        public void Awake()
        {
            contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
            MatchManager.onMatchSettingsLoadSuccess += MatchSettingsLoadSuccess;
            MatchManager.onMatchSettingsLoadFailed += MatchSettingsLoadFailed;
        }

        private void MatchSettingsLoadFailed(MatchSettingsLoadFailedReason reason)
        {
            _ = ClearSelectables();
        }

        private void MatchSettingsLoadSuccess()
        {
            _ = ClearSelectables();
        }

        public void Open()
        {
            lobbyName.text = NetworkManager.singleton.FusionLauncher.NetworkRunner.SessionInfo.Name;
            _ = ClearSelectables();
            gameObject.SetActive(true);
        }

        public void ButtonStartMatch()
        {
            _ = MatchManager.instance.StartMatch();
        }

        private async UniTask ClearSelectables()
        {
            if (clearInProgress)
            {
                return;
            }
            clearInProgress = true;

            foreach (Transform child in selectableContentHolder)
            {
                Destroy(child.gameObject);
            }

            GameObject gamemodeSelect = GameObject.Instantiate(selectablePrefab, selectableContentHolder, false);
            EventTrigger trigger = gamemodeSelect.GetComponent<EventTrigger>();
            trigger.AddOnSubmitListeners((data) => { OpenGamemodeSelect(); });

            if (String.IsNullOrEmpty(MatchManager.instance.SelectedGamemode))
            {
                clearInProgress = false;
                return;
            }

            await ContentManager.instance.LoadContentDefinition(ContentType.Gamemode, new ModObjectReference(MatchManager.instance.SelectedGamemode));
            IGameModeDefinition gm = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, new ModObjectReference(MatchManager.instance.SelectedGamemode));
            if (gm == null)
            {
                clearInProgress = false;
                return;
            }

            if(requiredContent == null)
            {
                requiredContent = new List<ModObjectReference>();
                for(int i = 0; i < gm.ContentRequirements.Length; i++)
                {
                    requiredContent.Add(new ModObjectReference());
                }
            }

            TextMeshProUGUI gmSelectText = gamemodeSelect.GetComponentInChildren<TextMeshProUGUI>();
            gmSelectText.text = gm.Name;
            
            for (int i = 0; i < gm.ContentRequirements.Length; i++)
            {
                int currentIndex = i;

                GameObject contentSelectItem = GameObject.Instantiate(selectablePrefab, selectableContentHolder, false);
                contentSelectItem.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { OpenContentSelect(gm.ContentRequirements[currentIndex], currentIndex); });
                contentSelectItem.GetComponentInChildren<TextMeshProUGUI>().text = String.IsNullOrEmpty(requiredContent[currentIndex].modIdentifier) ?
                    $"{gm.ContentRequirements[currentIndex].ToString()} Select"
                    : requiredContent[currentIndex].ToString();
            }

            clearInProgress = false;
        }

        List<ModObjectReference> requiredContent = new List<ModObjectReference>();
        int contentIndex = 0;
        public void OpenContentSelect(ContentType contentType, int contentIndex)
        {
            this.contentIndex = contentIndex;
            contentSelect.OnContentSelected += OnContentSelected;
            _ = contentSelect.OpenMenu(contentType);
            gameObject.SetActive(false);
        }

        private void OnContentSelected(ModObjectReference contentReference)
        {
            requiredContent[contentIndex] = contentReference;
            contentSelect.OnContentSelected -= OnContentSelected;
            contentSelect.CloseMenu();

            string content = "";
            for(int i = 0; i < requiredContent.Count; i++)
            {
                content += requiredContent[i];
                if(i != requiredContent.Count - 1)
                {
                    content += ",";
                }
            }
            MatchManager.instance.GamemodeContent = content;
        }

        public void OpenGamemodeSelect()
        {
            contentSelect.OnContentSelected += OnGamemodeSelected;
            contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
            _ = contentSelect.OpenMenu(ContentType.Gamemode);
            gameObject.SetActive(false);
        }

        private void OnGamemodeSelected(ModObjectReference gamemodeReference)
        {
            contentSelect.CloseMenu();
            contentSelect.OnContentSelected -= OnGamemodeSelected;

            requiredContent = null;
            MatchManager.instance.SelectedGamemode = gamemodeReference.ToString();
        }

        public void OpenCharacterSelect()
        {
            contentSelect.OnContentSelected += OnCharacterSelected;
            _ = contentSelect.OpenMenu(ContentType.Fighter);
            gameObject.SetActive(false);
        }

        private void OnCharacterSelected(ModObjectReference fighterReference)
        {
            ClientManager.local.RPC_SetCharacter(fighterReference.ToString());
            characterSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = fighterReference.ToString();
            contentSelect.OnContentSelected -= OnCharacterSelected;
            contentSelect.CloseMenu();
        }
    }
}