using HnSF;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class BoxBehaviour : FighterStateBehaviour
    {
        protected bool IsRectangle => shape == BoxShape.Rectangle;

        public FighterBoxType boxType;
        public int attachedTo;
        public BoxShape shape;
        public Vector3 offset;
        [ShowIf("IsRectangle")]
        public Vector3 boxExtents;
        [HideIf("IsRectangle")]
        public float radius;
        public int definitionIndex;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            FighterManager manager = (FighterManager)playerData;
            manager.BoxManager.AddBox(boxType, attachedTo, shape, offset, boxExtents, radius, definitionIndex, timelineClip.parentTrack.timelineAsset as StateTimeline);
        }
    }
}