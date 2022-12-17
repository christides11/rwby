using Animancer.Units;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class IKFootPlacement : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Animator anim;

        public LayerMask layerMask; // Select all layers that foot placement applies to.
        [Range (0f, 2f)]
        public float DistanceToGround; // Distance from where the foot transform is to the lowest possible position of the foot.

        public Vector3 offsetFoot;
        
        private Transform _LeftFoot;
        private Transform _RightFoot;
        private void Awake()
        {
            _LeftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            _RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            //UpdateFootIK(_LeftFoot, AvatarIKGoal.LeftFoot, 1.0f, anim.leftFeetBottomHeight);
            //UpdateFootIK(_RightFoot, AvatarIKGoal.RightFoot, 1.0f, anim.rightFeetBottomHeight);
            /*
            // Left Foot
            RaycastHit hit;
            if (networkObject.Runner.GetPhysicsScene().Raycast(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up,
                    Vector3.down, out hit, DistanceToGround, layerMask)) {
                Vector3 footPosition = hit.point;
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition + offsetFoot);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }

            // Right Foot
            if (networkObject.Runner.GetPhysicsScene().Raycast(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, 
                    Vector3.down, out hit, DistanceToGround, layerMask)) {
                Vector3 footPosition = hit.point;
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
                anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition + offsetFoot);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }*/
        }

        [SerializeField, Meters] private float _RaycastOriginY = 0.5f;
        [SerializeField, Meters] private float _RaycastEndY = -0.2f;
        private void UpdateFootIK(Transform footTransform, AvatarIKGoal goal, float weight, float footBottomHeight)
        {
            var animator = anim;
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKRotationWeight(goal, weight);

            if (weight == 0)
                return;

            var rotation = anim.GetIKRotation(goal);
            var localUp = rotation * Vector3.up;

            var position = footTransform.position;
            position += localUp * _RaycastOriginY;

            var distance = _RaycastOriginY - _RaycastEndY;
        
            if (Physics.Raycast(position, Vector3.down, out var hit, distance))
            {
                position = hit.point;
                position += localUp * footBottomHeight;
                anim.SetIKPosition(goal, position);

                var rotAxis = Vector3.Cross(localUp, hit.normal);
                var angle = Vector3.Angle(localUp, hit.normal);
                rotation = Quaternion.AngleAxis(angle, rotAxis) * rotation;

                anim.SetIKRotation(goal, rotation);
            }else
            {
                position += localUp * (footBottomHeight - distance);
                anim.SetIKPosition(goal, position);
            }
        }
    }
}