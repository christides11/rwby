using HnSF;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "StateTimeline", menuName = "rwby/StateTimeline")]
    public class StateTimeline : HnSF.StateTimeline, IBoxDefinitionCollection
    {
        public HitInfo[] HitboxInfo
        {
            get { return hitboxInfo; }
        }
        public ThrowInfo[] ThrowboxInfo
        {
            get { return throwboxInfo; }
        }
        public HurtboxInfo[] HurtboxInfo
        {
            get { return hurtboxInfo; }
        }
        
        [SelectImplementation(typeof(StateConditionBase))] [SerializeField, SerializeReference]
        public StateConditionBase conditon = new StateConditionBoolean();
        
        [SerializeField] [EnumFlags] public StateGroupType stateGroup;
        public bool autoIncrement;
        public bool autoLoop;
        public int loopFrame = 1;
        //[SelectImplementation((typeof(HitInfo)))] [SerializeField, SerializeReference] 
        [SerializeField] private HitInfo[] hitboxInfo;
        [SerializeField] private ThrowInfo[] throwboxInfo;
        [SerializeField] private HurtboxInfo[] hurtboxInfo;
        
        public bool useParent = false;
        [AllowNesting]
        [EnableIf("useParent")]
        public StateTimeline parentTimeline;
    }
}