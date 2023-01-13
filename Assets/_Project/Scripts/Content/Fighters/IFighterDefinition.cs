using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public abstract class IFighterDefinition : IContentDefinition
    {
        [System.Serializable]
        public struct CameraDef
        {
            public int id;
            public BaseCameraManager cam;
        }
        
        public virtual bool Selectable { get; }
        public virtual int Health { get; }
        public virtual int Aura { get; }
        public virtual int AuraGainPerFrame { get; }
        public virtual ModObjectItemReference[] huds { get; }
        public virtual CameraDef[] cameras { get; }
        public virtual SerializableGuid FighterGUID { get; }

        public abstract GameObject GetFighter();
        public abstract Moveset[] GetMovesets();
    }
}