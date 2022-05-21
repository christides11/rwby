using System.Collections;
using System.Collections.Generic;
using HnSF;
using HnSF.Input;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ConditionButtonSequence : IConditionVariables
    {
        public int FunctionMap => (int)BaseStateConditionEnum.BUTTON_SEQUENCE;
        
        public InputSequence sequence;
        public int offset;
        public bool processSequenceButtons;
        public bool holdInput;
    }
}