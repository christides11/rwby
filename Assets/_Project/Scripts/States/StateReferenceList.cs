using System.Collections;
using System.Collections.Generic;
using HnSF;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "State Reference List", menuName = "rwby/statereferencelist")]
    public class StateReferenceList : ScriptableObject
    {
        [SelectImplementation((typeof(FighterStateReferenceBase)))] [SerializeField, SerializeReference]
        public FighterStateReferenceBase[] stateList;
    }
}
