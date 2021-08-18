using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class IMapDefinition : IContentDefinition
    {
        public override string Name { get; }
        public virtual string SceneName { get; }
        public override string Description { get; }
        public virtual bool Selectable { get; }

        public abstract UniTask LoadMap(UnityEngine.SceneManagement.LoadSceneMode loadMode);
        public abstract UniTask UnloadMap();
    }
}