using Fusion;
using Fusion.Sockets;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterManager), typeof(FighterPhysicsManager), typeof(FighterStateManager), typeof(FighterBoxManager), typeof(FighterHitboxManager), typeof(FighterCombatManager))]
    public class FighterInputManager : NetworkBehaviour
    {
        public static int inputCapacity = 1024;

        [Networked] public int BufferLimit { get; set; }

        protected Vector2[] Movement = new Vector2[inputCapacity];
        protected Vector3[] CameraForward = new Vector3[inputCapacity];
        protected Vector3[] CameraRight = new Vector3[inputCapacity];
        protected InputButtonData[] LightAttack = new InputButtonData[inputCapacity];
        protected InputButtonData[] HeavyAttack = new InputButtonData[inputCapacity];
        protected InputButtonData[] Jump = new InputButtonData[inputCapacity];
        protected InputButtonData[] Block = new InputButtonData[inputCapacity];
        protected InputButtonData[] Grab = new InputButtonData[inputCapacity];
        protected InputButtonData[] Dash = new InputButtonData[inputCapacity];
        protected InputButtonData[] LockOn = new InputButtonData[inputCapacity];
        protected InputButtonData[] Ability1 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Ability2 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Ability3 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Ability4 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Extra1 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Extra2 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Extra3 = new InputButtonData[inputCapacity];
        protected InputButtonData[] Extra4 = new InputButtonData[inputCapacity];

        public FighterManager manager;

        public override void Spawned()
        {
            base.Spawned();
        }

        public void FeedInput(int frame, NetworkInputData inputData)
        {
            Movement[frame % inputCapacity] = inputData.movement;
            CameraForward[frame % inputCapacity] = inputData.forward;
            CameraRight[frame % inputCapacity] = inputData.right;
            LightAttack[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_LIGHT_ATTACK), LightAttack[(frame-1) % inputCapacity]);
            HeavyAttack[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_HEAVY_ATTACK), HeavyAttack[(frame-1) % inputCapacity]);
            Jump[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_JUMP), Jump[(frame-1) % inputCapacity]);
            Block[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_BLOCK), Block[(frame-1) % inputCapacity]);
            Grab[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_GRAB), Grab[(frame-1) % inputCapacity]);
            Dash[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_DASH), Dash[(frame-1) % inputCapacity]);
            LockOn[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_LOCK_ON), LockOn[(frame-1) % inputCapacity]);
            Ability1[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_ABILITY_ONE), Ability1[(frame - 1) % inputCapacity]);
            Ability2[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_ABILITY_TWO), Ability2[(frame - 1) % inputCapacity]);
            Ability3[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_ABILITY_THREE), Ability3[(frame - 1) % inputCapacity]);
            Ability4[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_ABILITY_FOUR), Ability4[(frame - 1) % inputCapacity]);
            Extra1[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_Extra_1), Extra1[(frame - 1) % inputCapacity]);
            Extra2[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_Extra_2), Extra2[(frame - 1) % inputCapacity]);
            Extra3[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_Extra_3), Extra3[(frame - 1) % inputCapacity]);
            Extra4[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_Extra_4), Extra4[(frame - 1) % inputCapacity]);
        }

        public virtual Vector2 GetMovement(int startOffset = 0)
        {
            return Movement[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual Vector3 GetCameraForward(int startOffset = 0)
        {
            return CameraForward[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual Vector3 GetCameraRight(int startOffset = 0)
        {
            return CameraRight[(Runner.Simulation.Tick - startOffset) % inputCapacity];
        }

        public virtual InputButtonData GetLightAttack(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref LightAttack, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetHeavyAttack(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref HeavyAttack, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetJump(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Jump, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetDash(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Dash, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetBlock(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Block, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetGrab(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Grab, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetLockOn(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref LockOn, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility1(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Ability1, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility2(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Ability2, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility3(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Ability3, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetAbility4(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Ability4, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra1(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Extra1, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra2(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Extra2, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra3(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Extra3, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetExtra4(out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            return GetButton(ref Extra4, out buttonOffset, startOffset, bufferFrames);
        }

        public virtual InputButtonData GetButton(PlayerInputType button, out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            buttonOffset = startOffset;
            switch (button)
            {
                case PlayerInputType.JUMP:
                    return GetJump(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.LIGHT_ATTACK:
                    return GetLightAttack(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.HEAVY_ATTACK:
                    return GetHeavyAttack(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.DASH:
                    return GetDash(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.ABILITY_1:
                    return GetAbility1(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.ABILITY_2:
                    return GetAbility2(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.ABILITY_3:
                    return GetAbility3(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.ABILITY_4:
                    return GetAbility4(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.LOCK_ON:
                    return GetLockOn(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.EXTRA_1:
                    return GetExtra1(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.EXTRA_2:
                    return GetExtra2(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.EXTRA_3:
                    return GetExtra3(out buttonOffset, startOffset, bufferFrames);
                case PlayerInputType.EXTRA_4:
                    return GetExtra4(out buttonOffset, startOffset, bufferFrames);
                default:
                    return new InputButtonData();
            }
        }

        protected virtual InputButtonData GetButton(ref InputButtonData[] buttonArray, out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            buttonOffset = startOffset;
            int currentTick = Runner.Simulation.Tick;
            for(int i = 0; i < bufferFrames; i++)
            {
                if (BufferLimit >= currentTick - (bufferFrames + i))
                {
                    break;
                }

                if (buttonArray[(currentTick - (bufferFrames + i)) % inputCapacity].firstPress)
                {
                    buttonOffset = startOffset + i;
                    return buttonArray[(currentTick - (bufferFrames + i)) % inputCapacity];
                }
            }
            return buttonArray[(currentTick - startOffset) % inputCapacity];
        }
    }
}