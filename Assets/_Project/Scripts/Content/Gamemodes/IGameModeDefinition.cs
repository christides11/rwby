using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public abstract class IGameModeDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }
        public virtual ModObjectReference[] GameModeComponentReferences { get; }
        public virtual ContentType[] ContentRequirements { get; }

        public abstract GameObject GetGamemode();
    }
}