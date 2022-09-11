using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterManager), typeof(FighterPhysicsManager), typeof(FighterStateManager), typeof(FighterBoxManager), typeof(FighterHitManager), typeof(FighterCombatManager))]
    [OrderAfter(typeof(ClientManager))]
    public class FighterInputManager : NetworkBehaviour
    {
        public static int inputCapacity = 40;

        [Networked] public uint BufferLimit { get; set; }

        [Networked] public NetworkBool inputEnabled { get; set; } = false;
        [Networked, Accuracy("Default"), Capacity(40)] protected NetworkArray<Vector2> movement => default;
        [Networked, Accuracy("Default"), Capacity(40)] protected NetworkArray<Vector3> cameraForward => default;
        [Networked, Accuracy("Default"), Capacity(40)] protected NetworkArray<Vector3> cameraRight => default;
        [Networked, Accuracy("Default"), Capacity(40)] protected NetworkArray<Vector3> cameraPos => default;
        [Networked, Capacity(40)] protected NetworkArray<NetworkButtons> buttons => default;
        [Networked] protected ushort heldATime { get; set; }
        [Networked] protected ushort heldBTime { get; set; }
        [Networked] protected ushort heldCTime { get; set; }
        [Networked] protected ushort heldJumpTime { get; set; }
        [Networked] protected ushort heldBlockTime { get; set; }
        [Networked] protected ushort heldDashTime { get; set; }
        [Networked] protected ushort heldLockOnTime { get; set; }
        [Networked] protected ushort heldAbility1Time { get; set; }
        [Networked] protected ushort heldAbility2Time { get; set; }
        [Networked] protected ushort heldAbility3Time { get; set; }
        [Networked] protected ushort heldAbility4Time { get; set; }
        [Networked] protected ushort heldExtra1Time { get; set; }
        [Networked] protected ushort heldExtra2Time { get; set; }
        [Networked] protected ushort heldExtra3Time { get; set; }
        [Networked] protected ushort heldExtra4Time { get; set; }

        public FighterManager manager;

        [Networked] public NetworkObject inputProvider { get; set; }
        [Networked] public byte inputSourceIndex { get; set; }

        public override void Spawned()
        {
            base.Spawned();
        }

        public void FeedInput()
        {
            int frame = Runner.Simulation.Tick;
            if (!inputProvider) return;
            if (!inputProvider.TryGetComponent<IInputProvider>(out IInputProvider pip)) return;
            NetworkPlayerInputData input = pip.GetInput(inputSourceIndex);
            movement.Set(frame % inputCapacity, input.movement.sqrMagnitude > 1 ? input.movement.normalized : input.movement);
            cameraForward.Set(frame % inputCapacity, input.forward);
            cameraRight.Set(frame % inputCapacity, input.right);
            cameraPos.Set(frame % inputCapacity, input.camPos);
            buttons.Set(frame % inputCapacity, input.buttons);
        }

        public virtual Vector2 GetMovement(int startOffset = 0)
        {
            return inputEnabled ? movement[(Runner.Simulation.Tick - startOffset) % inputCapacity] : Vector2.zero;
        }

        public virtual Vector3 GetCameraForward(int startOffset = 0)
        {
            return cameraForward[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual Vector3 GetCameraRight(int startOffset = 0)
        {
            return cameraRight[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual Vector3 GetCameraPosition(int startOffset = 0)
        {
            return cameraPos[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual InputButtonData GetA(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.A, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetB(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.B, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetC(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.C, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetJump(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.JUMP, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetDash(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.DASH, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetBlock(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.BLOCK, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetLockOn(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.LOCK_ON, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility1(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.ABILITY_1, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility2(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.ABILITY_2, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility3(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.ABILITY_3, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility4(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.ABILITY_4, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra1(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.EXTRA_1, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra2(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.EXTRA_2, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra3(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.EXTRA_3, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra4(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton((int)PlayerInputType.EXTRA_4, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetButton(int button, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(button, out int bOff, startOffset, bufferFrames);
        }
        
        public virtual InputButtonData GetButton(int button, out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            buttonOffset = startOffset;
            if (!inputEnabled) return default;
            int inputTick = Runner.Simulation.Tick - startOffset;
            for(int i = 0; i < bufferFrames; i++)
            {
                if (BufferLimit >= inputTick - i)
                {
                    break;
                }
                
                if (!IsFirstPress(button, inputTick - i)) continue;
                buttonOffset = startOffset + i;
                return new InputButtonData(){ firstPress = true, isDown = true, released = false};
            }

            return CreateButtonData(button, inputTick);
        }
        
        protected virtual InputButtonData CreateButtonData(int button, int frame)
        {
            bool isFirstPress = IsFirstPress(button, frame);
            return new InputButtonData()
            {
                firstPress = isFirstPress, 
                isDown = isFirstPress || IsDown(button, frame),
                released = !isFirstPress && IsRelease(button, frame)
            };
        }

        protected virtual bool IsFirstPress(int button, int frame)
        {
            if (buttons[frame % inputCapacity].IsSet(button) && !buttons[(frame - 1) % inputCapacity].IsSet(button)) return true;
            return false;
        }
        
        protected virtual bool IsRelease(int button, int frame)
        {
            if (!buttons[frame % inputCapacity].IsSet(button) && buttons[(frame - 1) % inputCapacity].IsSet(button)) return true;
            return false;
        }
        
        protected virtual bool IsDown(int button, int frame)
        {
            if (buttons[frame % inputCapacity].IsSet(button)) return true;
            return false;
        }
    }
}