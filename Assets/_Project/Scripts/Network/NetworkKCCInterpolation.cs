using Fusion;
using KinematicCharacterController;

namespace rwby
{
    [OrderBefore(typeof(ClientManager))]
    public class NetworkKCCInterpolation : SimulationBehaviour
    {
        public override void Render()
        {
            if (KinematicCharacterSystem.Settings.Interpolate)
            {
                KinematicCharacterSystem.CustomInterpolationUpdate();
            }
        }
    }
}