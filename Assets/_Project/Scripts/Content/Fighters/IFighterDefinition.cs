using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public abstract class IFighterDefinition : IContentDefinition
    {
        public virtual bool Selectable { get; }
        public virtual int Health { get; }

        public abstract GameObject GetFighter();
        public abstract string GetFighterGUID();
        public abstract Moveset[] GetMovesets();
    }
}