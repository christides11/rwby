using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby
{
    public abstract class IHUDElementbankDefinition : IContentDefinition
    {
        public virtual List<HUDElementbankEntry> HUDElements { get; }
        public virtual Dictionary<string, int> HUDElementMap { get; }
        
        public abstract GameObject GetHUDElement(string name);
    }
}