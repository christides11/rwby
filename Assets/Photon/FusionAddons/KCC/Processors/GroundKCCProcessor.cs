namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Base interface to identify and suppress execution of other ground processors which are not of type <c>GroundKCCProcessor</c>.
	/// </summary>
	public partial interface IGroundKCCProcessor
	{
	}

	/// <summary>
	/// Default movement implementation for character while grounded.
	/// </summary>
	public partial class GroundKCCProcessor : KCCProcessor, IGroundKCCProcessor
	{
		// CONSTANTS

		public static readonly int DefaultPriority = 2000;

		// PUBLIC MEMBERS

		public float KinematicSpeed => _kinematicSpeed;
		public float JumpMultiplier => _jumpMultiplier;

		// PRIVATE MEMBERS

		[SerializeField][Tooltip("Maximum allowed speed the KCC can move with player input.")]
		private float _kinematicSpeed = 8.0f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast the KCC slows down if the actual kinematic speed is higher (typically when leaving processor with higher speed).")]
		private float _kinematicSpeedLimitFactor = 1.0f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast the KCC starts accelerating in recently calculated kinematic direction. Existing kinematic velocity is reduced based on angle between the velocity and new direction.")]
		private float _kinematicDirectionResponsivity = 1.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by a costant value.")]
		private float _constantKinematicAcceleration = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by calculated kinematic speed multiplied by this.")]
		private float _relativeKinematicAcceleration = 50.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by (calculated kinematic speed - actual kinematic speed) multiplied by this. The faster KCC moves, the less acceleration is applied.")]
		private float _proportionalKinematicAcceleration = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by a costant value.")]
		private float _constantKinematicFriction = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by calculated kinematic speed multiplied by this.")]
		private float _relativeKinematicFriction = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by actual kinematic speed multiplied by this. The faster KCC moves, the more deceleration is applied.")]
		private float _proportionalKinematicFriction = 35.0f;
		[SerializeField][Tooltip("Dynamic velocity is decelerated by actual dynamic speed multiplied by this. The faster KCC moves, the more deceleration is applied.")]
		private float _proportionalDynamicFriction = 20.0f;
		[SerializeField][Tooltip("Resets dynamic velocity upon grounding.")]
		private bool  _clearDynamicVelocityOnTouch = false;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast input direction propagates to kinematic direction.")]
		private float _inputResponsivity = 1.0f;
		[SerializeField][Range(0.0f, 90.0f)][Tooltip("Maximum ground angle.")]
		private float _maxGroundAngle = 64.0f;
		[SerializeField][Range(0.0f, 90.0f)][Tooltip("Angle at which KCC starts moving slower (resulting kinematic speed is linear interpolation between base kinematic speed and 0).")]
		private float _slowWalkAngle = 48.0f;
		[SerializeField][Tooltip("Custom jump multiplier.")]
		private float _jumpMultiplier = 1.0f;
		[SerializeField][Tooltip("Relative priority. Default ground processor priority is 2000.")]
		private int   _relativePriority = 0;

		// KCCProcessor INTERFACE

		public override float Priority => DefaultPriority + _relativePriority;

		public override EKCCStages GetValidStages(KCC kcc, KCCData data)
		{
			EKCCStages stages = EKCCStages.SetInputProperties | EKCCStages.ProcessPhysicsQuery;

			if (data.IsGrounded == true)
			{
				stages |= EKCCStages.SetDynamicVelocity;
				stages |= EKCCStages.SetKinematicDirection;
				stages |= EKCCStages.SetKinematicTangent;
				stages |= EKCCStages.SetKinematicSpeed;
				stages |= EKCCStages.SetKinematicVelocity;
			}

			return stages;
		}

		public override void SetInputProperties(KCC kcc, KCCData data)
		{
			data.MaxGroundAngle = _maxGroundAngle;

			SuppressOtherProcessors(kcc);
		}

		public override void SetDynamicVelocity(KCC kcc, KCCData data)
		{
			if (data.IsSteppingUp == false && (data.IsSnappingToGround == true || data.GroundDistance > 0.001f))
			{
				data.DynamicVelocity += data.Gravity * data.DeltaTime;
			}

			if (data.JumpImpulse.IsZero() == false && _jumpMultiplier > 0.0f)
			{
				Vector3 jumpDirection = data.JumpImpulse.normalized;

				data.DynamicVelocity -= Vector3.Scale(data.DynamicVelocity, jumpDirection);
				data.DynamicVelocity += (data.JumpImpulse / kcc.Settings.Mass) * _jumpMultiplier;

				data.HasJumped = true;
			}

			data.DynamicVelocity += data.ExternalVelocity;
			data.DynamicVelocity += data.ExternalAcceleration * data.DeltaTime;
			data.DynamicVelocity += (data.ExternalImpulse / kcc.Settings.Mass);
			data.DynamicVelocity += (data.ExternalForce / kcc.Settings.Mass) * data.DeltaTime;

			if (data.DynamicVelocity.IsZero() == false)
			{
				if (data.DynamicVelocity.IsAlmostZero(0.001f) == true)
				{
					data.DynamicVelocity = default;
				}
				else
				{
					Vector3 frictionAxis = Vector3.one;
					if (data.GroundDistance > 0.001f || data.IsSnappingToGround == true)
					{
						frictionAxis.y = default;
					}

					data.DynamicVelocity += KCCPhysicsUtility.GetFriction(data.DynamicVelocity, data.DynamicVelocity, frictionAxis, data.GroundNormal, data.KinematicSpeed, true, 0.0f, 0.0f, _proportionalDynamicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);
				}
			}

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicDirection(KCC kcc, KCCData data)
		{
			Vector3 inputDirectionXZ     = data.InputDirection.OnlyXZ();
			Vector3 kinematicDirectionXZ = data.KinematicDirection.OnlyXZ();

			data.KinematicDirection = KCCUtility.EasyLerpDirection(kinematicDirectionXZ, inputDirectionXZ, data.DeltaTime, _inputResponsivity);

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicTangent(KCC kcc, KCCData data)
		{
			data.KinematicTangent = default;

			if (data.KinematicDirection.IsAlmostZero(0.0001f) == false && KCCPhysicsUtility.ProjectOnGround(data.GroundNormal, data.KinematicDirection, out Vector3 projectedMoveDirection) == true)
			{
				data.KinematicTangent = projectedMoveDirection.normalized;
			}
			else
			{
				data.KinematicTangent = data.GroundTangent;
			}

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicSpeed(KCC kcc, KCCData data)
		{
			if (data.GroundAngle <= _slowWalkAngle || Vector3.Dot(data.KinematicTangent, Vector3.up) <= 0.0f)
			{
				data.KinematicSpeed = _kinematicSpeed;
			}
			else
			{
				float slowdown = KCCMathUtility.Map(_slowWalkAngle, data.MaxGroundAngle, 0.0f, 1.0f, data.GroundAngle);
				data.KinematicSpeed = Mathf.Lerp(0.0f, _kinematicSpeed, 1.0f - slowdown);
			}

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicVelocity(KCC kcc, KCCData data)
		{
			if (data.KinematicVelocity.IsAlmostZero() == false && KCCPhysicsUtility.ProjectOnGround(data.GroundNormal, data.KinematicVelocity, out Vector3 projectedKinematicVelocity) == true)
			{
				data.KinematicVelocity = projectedKinematicVelocity.normalized * data.KinematicVelocity.magnitude;
			}

			if (data.KinematicDirection.IsAlmostZero() == true)
			{
				data.KinematicVelocity += KCCPhysicsUtility.GetFriction(data.KinematicVelocity, data.KinematicVelocity, Vector3.one, data.GroundNormal, data.KinematicSpeed, true, _constantKinematicFriction, _relativeKinematicFriction, _proportionalKinematicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);

				SuppressOtherProcessors(kcc);
				return;
			}

			if (_kinematicDirectionResponsivity > 0.0f)
			{
				data.KinematicVelocity -= data.KinematicVelocity * (1.0f - Mathf.Clamp01(Vector3.Dot(data.KinematicVelocity.OnlyXZ().normalized, data.KinematicDirection.OnlyXZ().normalized))) * Mathf.Min(_kinematicDirectionResponsivity, 1.0f);
			}

			Vector3 dynamicVelocity   = data.DynamicVelocity;
			Vector3 kinematicVelocity = data.KinematicVelocity;

			Vector3 moveDirection = kinematicVelocity;
			if (moveDirection.IsZero() == true)
			{
				moveDirection = data.KinematicTangent;
			}

			Vector3 acceleration = KCCPhysicsUtility.GetAcceleration(kinematicVelocity, data.KinematicTangent, Vector3.one, data.KinematicSpeed, false, data.KinematicDirection.magnitude, _constantKinematicAcceleration, _relativeKinematicAcceleration, _proportionalKinematicAcceleration, data.DeltaTime, kcc.FixedData.DeltaTime);
			Vector3 friction     = KCCPhysicsUtility.GetFriction(kinematicVelocity, moveDirection, Vector3.one, data.GroundNormal, data.KinematicSpeed, false, _constantKinematicFriction, _relativeKinematicFriction, _proportionalKinematicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);

			kinematicVelocity = KCCPhysicsUtility.CombineAccelerationAndFriction(kinematicVelocity, acceleration, friction);

			float kinematicSpeedOld   = data.KinematicVelocity.OnlyXZ().magnitude;
			float kinematicSpeedLimit = Mathf.Max(data.KinematicSpeed, kinematicSpeedOld * Mathf.Clamp01(1.0f - _kinematicSpeedLimitFactor * (data.DeltaTime / kcc.FixedData.DeltaTime)));

			float kinematicSpeed = kinematicVelocity.magnitude;
			if (kinematicSpeed > kinematicSpeedLimit)
			{
				kinematicVelocity *= kinematicSpeedLimit / kinematicSpeed;
			}

			Vector3 dynamicVelocity2D   = dynamicVelocity.OnlyXZ();
			Vector3 kinematicVelocity2D = kinematicVelocity.OnlyXZ();
			Vector3 desiredVelocity2D   = dynamicVelocity2D + kinematicVelocity2D;

			float dynamicSpeed2D = dynamicVelocity2D.magnitude;
			float desiredSpeed2D = desiredVelocity2D.magnitude;

			float kinematicSpeedAboveLimit = desiredSpeed2D - Mathf.Max(dynamicSpeed2D, kinematicSpeedLimit);
			if (kinematicSpeedAboveLimit > 0.0f)
			{
				kinematicVelocity -= kinematicVelocity.normalized * kinematicSpeedAboveLimit;
			}

			if (data.HasJumped == true && kinematicVelocity.y < 0.0f)
			{
				kinematicVelocity.y = 0.0f;
			}

			data.KinematicVelocity = kinematicVelocity;

			SuppressOtherProcessors(kcc);
		}

		public override void ProcessPhysicsQuery(KCC kcc, KCCData data)
		{
			if (data.IsGrounded == false)
				return;

			if (data.WasGrounded == true && data.IsSnappingToGround == false && data.DynamicVelocity.y < 0.0f && data.DynamicVelocity.OnlyXZ().IsAlmostZero() == true)
			{
				data.DynamicVelocity.y = 0.0f;
			}

			if (data.WasGrounded == false)
			{
				if (_clearDynamicVelocityOnTouch == true)
				{
					data.DynamicVelocity = default;
				}

				if (data.KinematicVelocity.OnlyXZ().IsAlmostZero() == true)
				{
					data.KinematicVelocity.y = 0.0f;
				}
				else
				{
					if (KCCPhysicsUtility.ProjectOnGround(data.GroundNormal, data.KinematicVelocity, out Vector3 projectedKinematicVelocity) == true)
					{
						data.KinematicVelocity = projectedKinematicVelocity.normalized * data.KinematicVelocity.magnitude;
					}
				}
			}

			SuppressOtherProcessors(kcc);
		}

		// PRIVATE METHODS

		private static void SuppressOtherProcessors(KCC kcc)
		{
			kcc.SuppressProcessors<IGroundKCCProcessor>();
		}
	}
}
