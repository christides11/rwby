using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "SongDefinition", menuName = "rwby/Content/Addressables/SongDefinition")]
    public class AddressablesSongDefinition : ISongDefinition
    {
        public override string Name { get { return songName; } }
        public override string Description { get { return description; } }
        public override SongAudio Song => songHandle.Result;

        [SerializeField] private string songName;
        [SerializeField] [TextArea] private string description;

        [SerializeField] private AssetReference songReference;
        [NonSerialized] private AsyncOperationHandle<SongAudio> songHandle;
        
        public override async UniTask<bool> Load()
        {
            try
            {
                if (songHandle.IsValid() == false)
                {
                    songHandle = Addressables.LoadAssetAsync<SongAudio>(songReference);
                }
                if (songHandle.IsDone == false)
                {
                    await songHandle;
                }
                if (songHandle.Status != AsyncOperationStatus.Succeeded) return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Song Load Error: {e.Message}");
                return false;
            }
            return true;
        }

        public override bool Unload()
        {
            if(songHandle.Status == AsyncOperationStatus.Succeeded) Addressables.Release(songHandle);
            return true;
        }
    }
}