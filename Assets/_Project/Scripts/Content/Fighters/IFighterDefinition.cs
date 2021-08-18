using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public abstract class IFighterDefinition : IContentDefinition
    {
        public virtual bool Selectable { get; }
        public virtual int Health { get; }

        /// <summary>
        /// Loads everything related to the fighter. Call this before any other method.
        /// </summary>
        /// <returns>True if the load was successful.</returns>
        public abstract UniTask<bool> LoadFighter();

        public abstract GameObject GetFighter();
        public abstract string GetFighterGUID();

        public abstract Moveset[] GetMovesets();
        /// <summary>
        /// Unloads everything related to the fighter. Call this when the fighter is no longer needed.
        /// </summary>
        public abstract void UnloadFighter();
    }
}