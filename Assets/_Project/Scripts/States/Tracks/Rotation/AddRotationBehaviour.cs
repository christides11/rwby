using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [System.Serializable]
    public class AddRotationBehaviour : RotationBehaviour
    {
        public float rotateYValue;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //FighterManager manager = (FighterManager)playerData;
            euler = new Vector3(0, rotateYValue, 0);
        }
    }
}