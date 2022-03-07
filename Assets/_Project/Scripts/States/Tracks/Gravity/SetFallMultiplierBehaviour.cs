using HnSF.Sample.TDAction;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class SetFallMultiplierBehaviour : FighterStateBehaviour
    {
        [SelectImplementation((typeof(FighterStatReferenceBase<float>)))] [SerializeReference]
        public FighterStatReferenceFloatBase value = new FighterBaseStatReferenceFloat();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager cm = playerData as FighterManager;
            if (cm == null) return;
            if (conditon.IsTrue(cm) == false) return;
            cm.CurrentFallMultiplier = value.GetValue(cm);
        }
    }
}