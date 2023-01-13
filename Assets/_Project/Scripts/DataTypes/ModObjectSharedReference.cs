using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "SharedReference", menuName = "rwby/ModObjectSharedReference")]
    public class ModObjectSharedReference : ScriptableObject
    {
        [FormerlySerializedAs("reference")] public ModContentStringReference reference;
    }
}