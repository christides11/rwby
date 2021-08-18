using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class GamemodeTraining : GameModeBase
    {
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
                cm.RPC_SetPlayer(fim);
                xOff += 5;
            }
            Debug.Log("Started gamemode.");
        }
    }
}