using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public interface ITargetable
    {
        public bool Targetable { get; }
        public Transform TargetOrigin { get; }

        public Bounds GetBounds();
        public GameObject GetGameObject();
    }
}