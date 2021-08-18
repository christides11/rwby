using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "SharedReference", menuName = "Mahou/ModObjectSharedReference")]
    public class ModObjectSharedReference : ScriptableObject
    {
        public ModObjectReference reference;
    }
}