using Animancer;
using Animancer.Units;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class IKFootPlacement : MonoBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private Animator anim;
        [SerializeField] private AnimancerComponent animancer;

        public LayerMask layerMask; // Select all layers that foot placement applies to.

        private Transform _LeftFoot;
        private Transform _RightFoot;
        
        private AnimatedFloat _FootWeights;
        
        private void Awake()
        {
            //_FootWeights = new AnimatedFloat(animancer, "LeftFootIK", "RightFootIK");
            //_LeftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            //_RightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            //UpdateFootIK(_LeftFoot, AvatarIKGoal.LeftFoot, _FootWeights[0], anim.leftFeetBottomHeight);
            //UpdateFootIK(_RightFoot, AvatarIKGoal.RightFoot, _FootWeights[1], anim.rightFeetBottomHeight);
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
        
            if (Physics.Raycast(position, -localUp, out var hit, distance, layerMask))
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