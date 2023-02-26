namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Base interface to identify and suppress execution of other air processors which are not of type <c>AirKCCProcessor</c>.
	/// </summary>
	public partial interface IAirKCCProcessor
	{
	}

	/// <summary>
	/// Default movement implementation for character while not grounded.
	/// </summary>
	public partial class AirKCCProcessor : KCCProcessor, IAirKCCProcessor
	{
		// CONSTANTS

		public static readonly int DefaultPriority = 1000;

		// PUBLIC MEMBERS

		public float KinematicSpeed    => _kinematicSpeed;
		public float GravityMultiplier => _gravityMultiplier;

		// PRIVATE MEMBERS

		[SerializeField][Tooltip("Maximum allowed speed the KCC can move with player input.")]
		private float _kinematicSpeed = 8.0f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast the KCC slows down if the actual kinematic speed is higher (typically when leaving processor with higher speed).")]
		private float _kinematicSpeedLimitFactor = 0.025f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast the KCC starts accelerating in recently calculated kinematic direction. Existing kinematic velocity is reduced based on angle between the velocity and new direction.")]
		private float _kinematicDirectionResponsivity = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by a costant value.")]
		private float _constantKinematicAcceleration = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by calculated kinematic speed multiplied by this.")]
		private float _relativeKinematicAcceleration = 5.0f;
		[SerializeField][Tooltip("Kinematic velocity is accelerated by (calculated kinematic speed - actual kinematic speed) multiplied by this. The faster KCC moves, the less acceleration is applied.")]
		private float _proportionalKinematicAcceleration = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by a costant value.")]
		private float _constantKinematicFriction = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by calculated kinematic speed multiplied by this.")]
		private float _relativeKinematicFriction = 0.0f;
		[SerializeField][Tooltip("Kinematic velocity is decelerated by actual kinematic speed multiplied by this. The faster KCC moves, the more deceleration is applied.")]
		private float _proportionalKinematicFriction = 2.0f;
		[SerializeField][Tooltip("Dynamic velocity is decelerated by actual dynamic speed multiplied by this. The faster KCC moves, the more deceleration is applied.")]
		private float _proportionalDynamicFriction = 2.0f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How fast input direction propagates to kinematic direction.")]
		private float _inputResponsivity = 0.75f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How much impact has overall vertical velocity on limiting kinematic velocity.")]
		private float _verticalVelocityImpact = 0.25f;
		[SerializeField][Tooltip("Custom gravity multiplier.")]
		private float _gravityMultiplier = 1.0f;
		[SerializeField][Tooltip("Relative priority. Default air processor priority is 1000.")]
		private int   _relativePriority = 0;

		// KCCProcessor INTERFACE

		public override float Priority => DefaultPriority + _relativePriority;

		public override EKCCStages GetValidStages(KCC kcc, KCCData data)
		{
			EKCCStages stages = EKCCStages.ProcessPhysicsQuery;

			if (data.IsGrounded == false)
			{
				stages |= EKCCStages.SetInputProperties;
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
			data.Gravity *= _gravityMultiplier;

			SuppressOtherProcessors(kcc);
		}

		public override void SetDynamicVelocity(KCC kcc, KCCData data)
		{
			data.DynamicVelocity += data.Gravity * data.DeltaTime;
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
					data.DynamicVelocity += KCCPhysicsUtility.GetFriction(data.DynamicVelocity, data.DynamicVelocity, new Vector3(1.0f, 0.0f, 1.0f), data.KinematicSpeed, true, 0.0f, 0.0f, _proportionalDynamicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);
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

			if (data.KinematicDirection.IsAlmostZero(0.0001f) == false)
			{
				data.KinematicTangent = data.KinematicDirection.normalized;
			}
			else
			{
				data.KinematicTangent = data.TransformDirection;
			}

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicSpeed(KCC kcc, KCCData data)
		{
			data.KinematicSpeed = _kinematicSpeed;

			SuppressOtherProcessors(kcc);
		}

		public override void SetKinematicVelocity(KCC kcc, KCCData data)
		{
			if (data.KinematicDirection.IsZero() == true)
			{
				data.KinematicVelocity += KCCPhysicsUtility.GetFriction(data.KinematicVelocity, data.KinematicVelocity, new Vector3(1.0f, 0.0f, 1.0f), data.KinematicSpeed, true, _constantKinematicFriction, _relativeKinematicFriction, _proportionalKinematicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);

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
			Vector3 friction     = KCCPhysicsUtility.GetFriction(kinematicVelocity, moveDirection, new Vector3(1.0f, 0.0f, 1.0f), data.KinematicSpeed, false, _constantKinematicFriction, _relativeKinematicFriction, _proportionalKinematicFriction, data.DeltaTime, kcc.FixedData.DeltaTime);

			kinematicVelocity = KCCPhysicsUtility.CombineAccelerationAndFriction(kinematicVelocity, acceleration, friction);

			float kinematicSpeedOld   = new Vector3(data.KinematicVelocity.x, data.KinematicVelocity.y * _verticalVelocityImpact, data.KinematicVelocity.z).magnitude;
			float kinematicSpeedLimit = Mathf.Max(data.KinematicSpeed, kinematicSpeedOld * Mathf.Clamp01(1.0f - _kinematicSpeedLimitFactor * (data.DeltaTime / kcc.FixedData.DeltaTime)));

			float kinematicSpeed = kinematicVelocity.magnitude;
			if (kinematicSpeed > kinematicSpeedLimit)
			{
				kinematicVelocity *= kinematicSpeedLimit / kinematicSpeed;
			}

			Vector3 scaledDynamicVelocity = dynamicVelocity;
			scaledDynamicVelocity.y *= _verticalVelocityImpact;

			Vector3 scaledKinematicVelocity = kinematicVelocity;
			scaledKinematicVelocity.y *= _verticalVelocityImpact;

			Vector3 scaledDesiredVelocity = scaledDynamicVelocity + scaledKinematicVelocity;

			float scaledDynamicSpeed = scaledDynamicVelocity.magnitude;
			float scaledDesiredSpeed = scaledDesiredVelocity.magnitude;

			float kinematicSpeedAboveLimit = scaledDesiredSpeed - Mathf.Max(scaledDynamicSpeed, kinematicSpeedLimit);
			if (kinematicSpeedAboveLimit > 0.0f)
			{
				kinematicVelocity -= kinematicVelocity.normalized * kinematicSpeedAboveLimit;
			}

			data.KinematicVelocity = kinematicVelocity;

			SuppressOtherProcessors(kcc);
		}

		public override void ProcessPhysicsQuery(KCC kcc, KCCData data)
		{
			if (data.IsGrounded == true || data.WasGrounded == true)
				return;

			if (data.DynamicVelocity.y > 0.0f && data.DeltaTime > 0.0f)
			{
				Vector3 currentVelocity = (data.TargetPosition - data.BasePosition) / data.DeltaTime;
				if (currentVelocity.y.IsAlmostZero() == true)
				{
					data.DynamicVelocity.y = 0.0f;
				}
			}

			SuppressOtherProcessors(kcc);
		}

		// PRIVATE METHODS

		private static void SuppressOtherProcessors(KCC kcc)
		{
			kcc.SuppressProcessors<IAirKCCProcessor>();
		}
	}
}
