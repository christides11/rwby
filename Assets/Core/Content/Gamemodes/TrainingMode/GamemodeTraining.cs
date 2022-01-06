using Cysharp.Threading.Tasks;
using Fusion;
using rwby.menus;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace rwby
{
    
    public class GamemodeTraining : GameModeBase
    {
        public ModObjectReference botReference;
        public FighterInputManager botInputManager;

        public ModObjectReference mapReference = new ModObjectReference();

        public override void AddGamemodeSettings(LobbyMenu lobbyManager)
        {
            GameObject gamemodeOb = GameObject.Instantiate(lobbyManager.gamemodeOptionsContentPrefab, lobbyManager.gamemodeOptionsList, false);
            TextMeshProUGUI[] textMeshes = gamemodeOb.GetComponentsInChildren<TextMeshProUGUI>();
            textMeshes[0].text = mapReference.ToString();
            gamemodeOb.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener((d) => { _ = OpenMapSelection(); });
        }

        private async UniTask OpenMapSelection()
        {
            await ContentSelect.singleton.OpenMenu<IMapDefinition>((a, b) => { 
                ContentSelect.singleton.CloseMenu(); 
                mapReference = b; 
                LobbyManager.singleton.CallGamemodeSettingsChanged(); 
            });
        }

        public override async UniTask<bool> VerifyGameModeSettings()
        {
            List<PlayerRef> failedLoadPlayers = await LobbyManager.singleton.clientContentLoaderService.TellClientsToLoad<IMapDefinition>(mapReference);
            if (failedLoadPlayers == null)
            {
                Debug.LogError("Load Map Local Failure");
                return false;
            }

            foreach (var v in failedLoadPlayers)
            {
                Debug.Log($"{v.PlayerId} failed to load {mapReference.ToString()}.");
            }

            if (failedLoadPlayers.Count != 0) return false;

            return true;
        }

        public override async void StartGamemode()
        {
            await LobbyManager.singleton.clientMapLoaderService.TellClientsToLoad(mapReference);

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapReference);

            if (NetworkManager.singleton.FusionLauncher.TryGetSceneRef(out SceneRef scene))
            {
                Runner.SetActiveScene(scene);
            }
        }

        /*
        public override void StartGamemode()
        {
            base.StartGamemode();

            
            int xOff = 0;
            foreach(var c in NetworkManager.singleton.FusionLauncher.Players)
            {
                ClientManager cm = c.Value.GetComponent<ClientManager>();

                ModObjectReference characterReference = new ModObjectReference(cm.SelectedCharacter);
                IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.singleton.GetContentDefinition<IFighterDefinition>(characterReference);

                FighterInputManager fim = NetworkManager.singleton.FusionLauncher.NetworkRunner.Spawn(fighterDefinition.GetFighter().GetComponent<FighterInputManager>(), new Vector3(xOff, 5, 0), Quaternion.identity, c.Key);
                fim.gameObject.name = $"Player {cm.PlayerName}";
                cm.ClientFighter = fim.GetComponent<NetworkObject>();
                xOff += 5;
            }

            // Spawn BOT
            IFighterDefinition botDefinition = (IFighterDefinition)ContentManager.singleton.GetContentDefinition<IFighterDefinition>(botReference);
            botInputManager = NetworkManager.singleton.FusionLauncher.NetworkRunner.Spawn(botDefinition.GetFighter().GetComponent<FighterInputManager>(), new Vector3(0, 0, 5), Quaternion.identity, null);
            botInputManager.gameObject.name = $"Bot";
        }*/

        public override void FixedUpdateNetwork()
        {
            /*
            if (botInputManager)
            {
                //botInputManager.FeedInput(Runner.Simulation.Tick, CreateBotInput());
            }*/
        }

        public bool buttonBlock;
        public bool buttonJump;

        private NetworkClientInputData CreateBotInput()
        {
            NetworkClientInputData botInput = new NetworkClientInputData();

            /*
            if (buttonBlock) { botInput.Buttons |= NetworkClientInputData.BUTTON_BLOCK; }
            if (buttonJump) { botInput.Buttons |= NetworkClientInputData.BUTTON_JUMP; }*/
            return botInput;
        }
    }
}