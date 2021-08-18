using Fusion;
using Fusion.Sockets;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterManager), typeof(FighterPhysicsManager), typeof(FighterStateManager), typeof(FighterHurtboxManager), typeof(FighterHitboxManager), typeof(FighterCombatManager))]
    public class FighterInputManager : NetworkBehaviour
    {
        protected NetworkManager networkManager;

        public static int inputCapacity = 1024;

        [Networked] public int BufferLimit { get; set; }

        protected Vector2[] Movement = new Vector2[inputCapacity];
        protected InputButtonData[] LightAttack = new InputButtonData[inputCapacity];
        protected InputButtonData[] HeavyAttack = new InputButtonData[inputCapacity];
        protected InputButtonData[] Jump = new InputButtonData[inputCapacity];
        protected InputButtonData[] Block = new InputButtonData[inputCapacity];
        protected InputButtonData[] Shoot = new InputButtonData[inputCapacity];
        protected InputButtonData[] Dash = new InputButtonData[inputCapacity];
        protected InputButtonData[] LockOn = new InputButtonData[inputCapacity];

        private void Awake()
        {
            networkManager = NetworkManager.singleton;
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        public void FeedInput(int frame, NetworkInputData inputData)
        {
            Movement[frame % inputCapacity] = inputData.movement;
            LightAttack[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_LIGHT_ATTACK), LightAttack[(frame-1) % inputCapacity]);
            HeavyAttack[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_HEAVY_ATTACK), HeavyAttack[(frame-1) % inputCapacity]);
            Jump[frame % inputCapacity] = new InputButtonData(inputData.IsDown(NetworkInputData.BUTTON_JUMP), Jump[(frame-1) % inputCapacity]);
        }

        public virtual Vector2 GetMovement(int startOffset = 0)
        {
            return Movement[networkManager.FusionLauncher.NetworkRunner.Simulation.Tick % inputCapacity];
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

        protected virtual InputButtonData GetButton(ref InputButtonData[] buttonArray, out int buttonOffset, int startOffset = 0, int bufferFrames = 0)
        {
            buttonOffset = startOffset;
            int currentTick = networkManager.FusionLauncher.NetworkRunner.Simulation.Tick;
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