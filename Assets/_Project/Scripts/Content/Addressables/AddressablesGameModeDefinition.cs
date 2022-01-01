using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesGameModeDefinition", menuName = "rwby/Content/Addressables/GameModeDefinition")]
    public class AddressablesGameModeDefinition : IGameModeDefinition
    {

        public override string Name { get { return gamemodeName; } }
        public override string Description { get { return description; } }
        public override ModObjectReference[] GameModeComponentReferences { get { return componentReferences; } }
        public override ContentType[] ContentRequirements { get { return contentRequirements; } }

        [SerializeField] private string gamemodeName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReference gamemodeReference;
        [SerializeField] private ModObjectReference[] componentReferences;
        [SerializeField] private ContentType[] contentRequirements;

        [NonSerialized] private GameObject gamemode;

        public override async UniTask<bool> Load()
        {
            if (gamemode != null)
            {
                return true;
            }

            try
            {
                var hh = await Addressables.LoadAssetAsync<GameObject>(gamemodeReference).Task;
                gamemode = hh;
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public override GameModeBase GetGamemode()
        {
            if(gamemode == null)
            {
                Debug.Log("Null gamemode");
                return null;
            }
            return gamemode.GetComponent<GameModeBase>();
        }

        public override bool Unload()
        {
            Addressables.Release<GameObject>(gamemode);
            return true;
        }
    }
}