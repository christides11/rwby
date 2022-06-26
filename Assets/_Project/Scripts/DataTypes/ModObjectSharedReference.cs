using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "SharedReference", menuName = "Mahou/ModObjectSharedReference")]
    public class ModObjectSharedReference : ScriptableObject
    {
        [FormerlySerializedAs("reference")] public ModGUIDContentReference contentReference;
    }
}