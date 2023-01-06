using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UMod;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesGameModeDefinition", menuName = "rwby/Content/UMod/GameModeDefinition")]
    public class UModGameModeDefinition : IGameModeDefinition, IUModModHostRef
    {
        public ModHost modHost { get; set; } = null;
        public override string Name { get { return gamemodeName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string gamemodeName;
        [SerializeField] [TextArea] private string description;

        [SerializeField] private string gamemodeReference = "";
        [NonSerialized] private ModAsyncOperation<GameObject> gamemodeHandle;

        public override async UniTask<bool> Load()
        {
            if (gamemodeHandle != null && gamemodeHandle.IsSuccessful) return true;
            if (gamemodeHandle == null) gamemodeHandle = modHost.Assets.LoadAsync<GameObject>(gamemodeReference);
            if (gamemodeHandle.IsDone == false || gamemodeHandle.IsSuccessful == false) await gamemodeHandle;
            if (gamemodeHandle.IsSuccessful)
            {
                return true;
            }
            Debug.LogError($"Could not load gamemode {gamemodeName}.");
            return false;
        }

        public override GameObject GetGamemode()
        {
            return gamemodeHandle.Result;
        }

        public override bool Unload()
        {
            if(gamemodeHandle.IsSuccessful) gamemodeHandle.Reset();
            gamemodeHandle = null;
            return true;
        }
    }
}