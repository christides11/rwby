using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static Fusion.Allocator;
using static Fusion.NetworkCharacterController;
using UnityEngine.UIElements;

namespace rwby
{
    public class FighterCharacterController : NetworkBehaviour
    {
        [Networked] public Vector3 moveVector { get; set; }

        public NetworkCharacterController ncc;

        protected Transform _transform;

        public bool Grounded;

        public void Awake()
        {
            _transform = transform;
            ncc = GetComponent<NetworkCharacterController>();
        }

        public void ForceUnground()
        {
            ncc.Jumped = true;
        }

        public void Move(Vector3 movementForce, float gravityForce, ICallbacks callback = null, LayerMask? layerMask = null)
        {
            if(movementForce.y > 0)
            {
                ForceUnground();
            }

            var dt = Runner.DeltaTime;
            var movementPack = ncc.ComputeRawMovement(movementForce, callback, layerMask);

            ComputeRawSteer(ref movementPack, dt, gravityForce);

            var movement = ncc.Velocity * dt;
            if (movementPack.Penetration > float.Epsilon)
            {
                if (movementPack.Penetration > ncc.Config.AllowedPenetration)
                {
                    movement += movementPack.Correction;
                }
                else
                {
                    movement += movementPack.Correction * ncc.Config.PenetrationCorrection;
                }
            }

            _transform.position += movement;

#if DEBUG
            //LastMovement = movementPack;
#endif
        }

        void ComputeRawSteer(ref Movement movementPack, float dt, float gravityForce)
        {
            Grounded = movementPack.Grounded;

            var current = ncc.Velocity;
            switch (movementPack.Type)
            {
                case MovementType.FreeFall:
                    ncc.Grounded = false;
                    current = movementPack.Tangent * dt;
                    current.y = gravityForce;
                    break;
                case MovementType.Horizontal:
                    ncc.Grounded = true;
                    current = movementPack.Tangent * dt;
                    current.y = gravityForce;
                    break;
                case MovementType.SlopeFall:
                    ncc.Grounded = true;
                    current = movementPack.SlopeTangent * dt;
                    current.y = gravityForce;
                    break;
                case MovementType.None:
                    ncc.Grounded = true;
                    current.y = gravityForce;
                    break;
            }

            ncc.Velocity = current;
            ncc.Jumped = false;
        }
    }
}