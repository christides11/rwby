using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace rwby
{
    public abstract class IMapDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }
        public virtual bool Selectable { get; }

        public abstract List<string> GetSceneNames();
        public abstract UniTask LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode);
        public abstract UniTask UnloadMap();
    }
}