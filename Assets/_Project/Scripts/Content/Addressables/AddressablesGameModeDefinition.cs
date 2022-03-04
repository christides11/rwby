using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesGameModeDefinition", menuName = "rwby/Content/Addressables/GameModeDefinition")]
    public class AddressablesGameModeDefinition : IGameModeDefinition
    {
        public override string Name { get { return gamemodeName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string gamemodeName;
        [SerializeField] [TextArea] private string description;

        [SerializeField] private AssetReference gamemodeReference;
        [NonSerialized] private AsyncOperationHandle<GameObject> gamemodeHandle;

        public override async UniTask<bool> Load()
        {
            try
            {
                if (gamemodeHandle.IsValid() == false)
                {
                    gamemodeHandle = Addressables.LoadAssetAsync<GameObject>(gamemodeReference);
                }
                if (gamemodeHandle.IsDone == false)
                {
                    await gamemodeHandle;
                }
                if (gamemodeHandle.Status != AsyncOperationStatus.Succeeded) return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"GameMode Load Error: {e.Message}");
                return false;
            }
            return true;
        }

        public override GameObject GetGamemode()
        {
            return gamemodeHandle.Result;
        }

        public override bool Unload()
        {
            Addressables.Release(gamemodeHandle);
            return true;
        }
    }
}