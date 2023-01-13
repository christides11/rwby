using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "SharedModSetContentReference", menuName = "rwby/SharedModSetContentReference")]
    public class SharedModSetContentReference : ScriptableObject
    {
        [FormerlySerializedAs("reference")] public ModObjectSetContentReference reference;
    }
}