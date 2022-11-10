using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public interface IGamemodeInitialization
    {
        UniTask<bool> VerifyGameModeSettings();
        UniTaskVoid StartGamemode();
    }
}