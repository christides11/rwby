using Cysharp.Threading.Tasks;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    
    public class GamemodeTraining : GameModeBase
    {
        public ModObjectReference botReference;
        public FighterInputManager botInputManager;

        public override async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            bool baseResult = await base.SetupGamemode(componentReferences, content);
            if (baseResult == false)
            {
                SetupFailed();
                return false;
            }

            if (content.Count != 1)
            {
                SetupFailed();
                return false;
            }

            bool mapLoadResult = await GameManager.singleton.LoadMap(content[0]);
            if (mapLoadResult == false)
            {
                SetupFailed();
                return false;
            }

            SetupSuccess();
            return true;
        }

        public override void StartGamemode()
        {
            base.StartGamemode();

            int xOff = 0;
            foreach(var c in NetworkManager.singleton.FusionLauncher.Players)
            {
                ClientManager cm = c.Value.GetComponent<ClientManager>();

                ModObjectReference characterReference = new ModObjectReference(cm.SelectedCharacter);
                IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, characterReference);

                FighterInputManager fim = NetworkManager.singleton.FusionLauncher.NetworkRunner.Spawn(fighterDefinition.GetFighter().GetComponent<FighterInputManager>(), new Vector3(xOff, 5, 0), Quaternion.identity, c.Key);
                fim.gameObject.name = $"Player {cm.PlayerName}";
                cm.ClientFighter = fim.GetComponent<NetworkObject>();
                xOff += 5;
            }

            // Spawn BOT
            IFighterDefinition botDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, botReference);
            botInputManager = NetworkManager.singleton.FusionLauncher.NetworkRunner.Spawn(botDefinition.GetFighter().GetComponent<FighterInputManager>(), new Vector3(0, 0, 5), Quaternion.identity, null);
            botInputManager.gameObject.name = $"Bot";
        }

        public override void FixedUpdateNetwork()
        {
            if (botInputManager)
            {
                botInputManager.FeedInput(Runner.Simulation.Tick, CreateBotInput());
            }
        }

        public bool buttonBlock;
        public bool buttonJump;

        private NetworkInputData CreateBotInput()
        {
            NetworkInputData botInput = new NetworkInputData();

            
            if (buttonBlock) { botInput.Buttons |= NetworkInputData.BUTTON_BLOCK; }
            if (buttonJump) { botInput.Buttons |= NetworkInputData.BUTTON_JUMP; }
            return botInput;
        }
    }
}