using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [CreateAssetMenu(fileName = "SharedModIDReference", menuName = "rwby/SharedModIDReference")]
    public class SharedModIDContentReference : ScriptableObject
    {
        [FormerlySerializedAs("reference")] public ModIDContentReference reference;
    }
}