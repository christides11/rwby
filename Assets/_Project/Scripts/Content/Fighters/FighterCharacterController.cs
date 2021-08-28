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
        public NetworkCharacterController ncc;
        protected Transform _transform;

        public Movement LastMovement;

        public LayerMask groundMask;

        public void Awake()
        {
            _transform = transform;
            ncc = GetComponent<NetworkCharacterController>();
        }

        public void ForceUnground()
        {
            ncc.Jumped = true;
        }

        public void Move(Vector3 movementForce, float gravityForce, ICallbacks callback = null)
        {
            if(gravityForce > 0)
            {
                ForceUnground();
            }

            var dt = Runner.DeltaTime;
            var movementPack = ncc.ComputeRawMovement(movementForce.normalized, callback, groundMask);

            ComputeRawSteer(ref movementPack, dt, movementForce, gravityForce);

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
            LastMovement = movementPack;
#endif
        }

        void ComputeRawSteer(ref Movement movementPack, float dt, Vector3 movementForce, float gravityForce)
        {
            ncc.Grounded = movementPack.Grounded;
            var current = ncc.Velocity;

            if(movementPack.Contacts == 1 && Vector3.Angle(Vector3.up, movementPack.GroundNormal) > ncc.Config.MaxSlope)
            {
                ncc.Grounded = false;
            }

            switch (movementPack.Type)
            {
                case MovementType.FreeFall:
                    current = movementForce;
                    current.y = gravityForce;
                    break;
                case MovementType.Horizontal:
                    current = movementForce;
                    current.y = gravityForce;
                    break;
                case MovementType.SlopeFall:
                    current = movementPack.SlopeTangent;
                    current.y = gravityForce;
                    break;
                case MovementType.None:
                    current = movementForce;//movementPack.Tangent;
                    current.y = gravityForce;
                    break;
            }

            ncc.Velocity = current;
            ncc.Jumped = false;
        }
    }
}