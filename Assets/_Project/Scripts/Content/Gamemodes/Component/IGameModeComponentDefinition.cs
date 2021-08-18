using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public abstract class IGameModeComponentDefinition : IContentDefinition
    {
        public abstract UniTask<bool> LoadGamemodeComponent();
        public abstract GameObject GetGamemodeComponent();
        public abstract void UnloadGamemodeComponent();
    }
}