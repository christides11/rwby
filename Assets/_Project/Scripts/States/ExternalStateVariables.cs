using System;
using HnSF;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "ExternalStateVars", menuName = "rwby/ExternalStateVars")]
    public class ExternalStateVariables : ScriptableObject
    {
        [SelectImplementation(typeof(IStateVariables))] [SerializeField, SerializeReference]
        public IStateVariables[] data = Array.Empty<IStateVariables>();
    }
}
