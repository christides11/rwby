using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UMod;
using UnityEngine;

namespace rwby
{
    public class UModSongDefinition : ISongDefinition, IUModModHostRef
    {
        public ModHost modHost { get; set; } = null;
        public override string Name { get { return songName; } }
        public override string Description { get { return description; } }
        public override SongAudio Song => songHandle.Result;

        [SerializeField] private string songName;
        [SerializeField] [TextArea] private string description;

        [SerializeField] private string songReference = "";
        [NonSerialized] private ModAsyncOperation<SongAudio> songHandle = null;
        
        public override async UniTask<bool> Load()
        {
            if (songHandle != null && songHandle.IsSuccessful) return true;

            if (songHandle == null) songHandle = modHost.Assets.LoadAsync<SongAudio>(songReference);

            if (songHandle.IsDone == false || !songHandle.IsSuccessful) await songHandle;

            if (songHandle.IsSuccessful) return true;
                
            Debug.LogError($"Error loading song {songName}.");
            return false;
        }

        public override bool Unload()
        {
            if(songHandle.IsSuccessful) songHandle.Reset();
            songHandle = null;
            return true;
        }
    }
}