using Fusion;
using KinematicCharacterController;

namespace rwby
{
    [OrderBefore(typeof(NetworkPhysicsSimulation3D)), OrderAfter(typeof(NetworkPhysicsSimulation2D))]
    public class NetworkKCCSimulation : SimulationBehaviour
    {
        private void Awake()
        {
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.Settings.AutoSimulation = false;
            KinematicCharacterSystem.Settings.Interpolate = false;
        }

        public override void FixedUpdateNetwork()
        {
            KinematicCharacterSystem.PreSimulationInterpolationUpdate(Runner.DeltaTime);
            KinematicCharacterSystem.Simulate(Runner.DeltaTime, KinematicCharacterSystem.CharacterMotors, KinematicCharacterSystem.PhysicsMovers);
        }
    }
}