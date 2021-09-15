using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;
using Fusion;

namespace rwby.Combat.AttackEvents
{
    public class PlaySFX : AttackEvent
    {
        public NetworkedAudioClip aClipPrefab;
        public AudioClip audioClip;

        public override string GetName()
        {
            return "Play SFX";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, IFighterBase controller)
        {
            //var key = new NetworkObjectPredictionKey { Byte0 = (byte)(controller as FighterManager).Runner.Simulation.Tick};
            //NetworkedAudioClip no = (controller as FighterManager).Runner.Spawn(aClipPrefab, (controller as FighterManager).transform.position, Quaternion.identity);
            //no.audioSource.PlayOneShot(audioClip);
            return AttackEventReturnType.NONE;
        }
    }
}