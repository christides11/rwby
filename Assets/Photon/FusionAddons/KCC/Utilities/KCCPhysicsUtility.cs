namespace Fusion.KCC
{
	using UnityEngine;

	public static partial class KCCPhysicsUtility
	{
		// PUBLIC METHODS

		public static bool ProjectOnGround(Vector3 groundNormal, Vector3 vector, out Vector3 projectedVector)
		{
			float dot1 = Vector3.Dot(Vector3.up, groundNormal);
			float dot2 = -Vector3.Dot(vector, groundNormal);

			if (dot1.IsAlmostZero(0.001f) == false)
			{
				projectedVector = new Vector3(vector.x, vector.y + dot2 / dot1, vector.z);
				return true;
			}

			projectedVector = default;
			return false;
		}

		public static void ProjectVerticalPenetration(ref Vector3 direction, ref float distance)
		{
			Vector3 desiredCorrection = direction * distance;

			float distanceXZ        = distance * (1.0f - Vector3.Dot(direction, Vector3.up));
			float reflectedDistance = desiredCorrection.y * desiredCorrection.y / distanceXZ;

			desiredCorrection = desiredCorrection.OnlyXZ() * (1.0f + reflectedDistance / distanceXZ);

			float desiredDistance = Vector3.Magnitude(desiredCorrection);
			if (desiredDistance >= 0.000001f)
			{
				direction = desiredCorrection / desiredDistance;
				distance  = desiredDistance;
			}
		}

		public static void ProjectHorizontalPenetration(ref Vector3 direction, ref float distance)
		{
			Vector3 desiredCorrection = direction * distance;

			direction = Vector3.up;
			distance  = 0.0f;

			if (desiredCorrection.y > -0.000001f && desiredCorrection.y < 0.000001f)
				return;

			distance = desiredCorrection.y + (desiredCorrection.x * desiredCorrection.x + desiredCorrection.z * desiredCorrection.z) / desiredCorrection.y;

			if (distance < 0.0f)
			{
				direction = -direction;
				distance  = -distance;
			}
		}

		public static bool CheckGround(Collider collider, Vector3 position, Collider groundCollider, Transform groundTransform, float radius, float height, float extent, float minGroundDot, out Vector3 groundNormal, out float groundDistance, out bool isWithinExtent)
		{
			isWithinExtent = false;

#if KCC_DISABLE_TERRAIN
			if (groundCollider is MeshCollider)
#else
			if (groundCollider is MeshCollider || groundCollider is TerrainCollider)
#endif
			{
				if (Physics.ComputePenetration(collider, position - new Vector3(0.0f, extent, 0.0f), Quaternion.identity, groundCollider, groundTransform.position, groundTransform.rotation, out Vector3 direction, out float distance) == true)
				{
					isWithinExtent = true;

					float directionUpDot = Vector3.Dot(direction, Vector3.up);
					if (directionUpDot >= minGroundDot)
					{
						Vector3 projectedDirection = direction;
						float   projectedDistance  = distance;

						ProjectHorizontalPenetration(ref projectedDirection, ref projectedDistance);

						float verticalDistance = Mathf.Max(0.0f, extent - projectedDistance);

						groundNormal   = direction;
						groundDistance = verticalDistance * directionUpDot;

						return true;
					}
				}
			}
			else
			{
				float   radiusSqr                 = radius * radius;
				float   radiusExtent              = radius + extent;
				float   radiusExtentSqr           = radiusExtent * radiusExtent;
				Vector3 centerPosition            = position + new Vector3(0.0f, radius, 0.0f);
				Vector3 closestPoint              = Physics.ClosestPoint(centerPosition, groundCollider, groundTransform.position, groundTransform.rotation);
				Vector3 closestPointOffset        = closestPoint - centerPosition;
				Vector3 closestPointOffsetXZ      = closestPointOffset.OnlyXZ();
				float   closestPointDistanceXZSqr = closestPointOffsetXZ.sqrMagnitude;

				if (closestPointDistanceXZSqr <= radiusExtentSqr)
				{
					if (closestPointOffset.y < 0.0f)
					{
						float closestPointDistance = Vector3.Magnitude(closestPointOffset);
						if (closestPointDistance <= radiusExtent)
						{
							isWithinExtent = true;

							Vector3 closestPointDirection = closestPointOffset / closestPointDistance;
							Vector3 closestGroundNormal   = -closestPointDirection;

							float closestGroundDot = Vector3.Dot(closestGroundNormal, Vector3.up);
							if (closestGroundDot >= minGroundDot)
							{
								groundNormal   = closestGroundNormal;
								groundDistance = Mathf.Max(0.0f, closestPointDistance - radius);

								return true;
							}
						}
					}
					else if (closestPointOffset.y < height - radius * 2.0f)
					{
						isWithinExtent = true;
					}
				}
			}

			groundNormal   = Vector3.up;
			groundDistance = 0.0f;

			return false;
		}

		public static Vector3 GetAcceleration(Vector3 velocity, Vector3 direction, Vector3 axis, float maxSpeed, bool clampSpeed, float inputAcceleration, float constantAcceleration, float relativeAcceleration, float proportionalAcceleration, float deltaTime, float fixedDeltaTime)
		{
			if (inputAcceleration <= 0.0f)
				return Vector3.zero;
			if (constantAcceleration <= 0.0f && relativeAcceleration <= 0.0f && proportionalAcceleration <= 0.0f)
				return Vector3.zero;
			if (direction.IsZero() == true)
				return Vector3.zero;

			float   baseSpeed     = new Vector3(velocity.x  * axis.x, velocity.y  * axis.y, velocity.z  * axis.z).magnitude;
			Vector3 baseDirection = new Vector3(direction.x * axis.x, direction.y * axis.y, direction.z * axis.z).normalized;

			float missingSpeed = Mathf.Max(0.0f, maxSpeed - baseSpeed);

			if (constantAcceleration     < 0.0f) { constantAcceleration     = 0.0f; }
			if (relativeAcceleration     < 0.0f) { relativeAcceleration     = 0.0f; }
			if (proportionalAcceleration < 0.0f) { proportionalAcceleration = 0.0f; }

			constantAcceleration     *= inputAcceleration;
			relativeAcceleration     *= inputAcceleration;
			proportionalAcceleration *= inputAcceleration;

			float speedGain = (constantAcceleration + maxSpeed * relativeAcceleration + missingSpeed * proportionalAcceleration) * GetCompensatedDeltaTime(deltaTime, fixedDeltaTime);
			if (speedGain <= 0.0f)
				return Vector3.zero;

			if (clampSpeed == true && speedGain > missingSpeed)
			{
				speedGain = missingSpeed;
			}

			return baseDirection.normalized * speedGain;
		}

		public static Vector3 GetAcceleration(Vector3 velocity, Vector3 direction, Vector3 axis, Vector3 normal, float targetSpeed, bool clampSpeed, float inputAcceleration, float constantAcceleration, float relativeAcceleration, float proportionalAcceleration, float deltaTime, float fixedDeltaTime)
		{
			float accelerationMultiplier = 1.0f - Mathf.Clamp01(Vector3.Dot(direction.normalized, normal));

			constantAcceleration     *= accelerationMultiplier;
			relativeAcceleration     *= accelerationMultiplier;
			proportionalAcceleration *= accelerationMultiplier;

			return GetAcceleration(velocity, direction, axis, targetSpeed, clampSpeed, inputAcceleration, constantAcceleration, relativeAcceleration, proportionalAcceleration, deltaTime, fixedDeltaTime);
		}

		public static Vector3 GetFriction(Vector3 velocity, Vector3 direction, Vector3 axis, float maxSpeed, bool clampSpeed, float constantFriction, float relativeFriction, float proportionalFriction, float deltaTime, float fixedDeltaTime)
		{
			if (constantFriction <= 0.0f && relativeFriction <= 0.0f && proportionalFriction <= 0.0f)
				return Vector3.zero;
			if (direction.IsZero() == true)
				return Vector3.zero;

			float   baseSpeed     = new Vector3(velocity.x  * axis.x, velocity.y  * axis.y, velocity.z  * axis.z).magnitude;
			Vector3 baseDirection = new Vector3(direction.x * axis.x, direction.y * axis.y, direction.z * axis.z).normalized;

			if (constantFriction     < 0.0f) { constantFriction     = 0.0f; }
			if (relativeFriction     < 0.0f) { relativeFriction     = 0.0f; }
			if (proportionalFriction < 0.0f) { proportionalFriction = 0.0f; }

			float speedDrop = (constantFriction + maxSpeed * relativeFriction + baseSpeed * proportionalFriction) * GetCompensatedDeltaTime(deltaTime, fixedDeltaTime);
			if (speedDrop <= 0.0f)
				return Vector3.zero;

			if (clampSpeed == true && speedDrop > baseSpeed)
			{
				speedDrop = baseSpeed;
			}

			return -baseDirection * speedDrop;
		}

		public static Vector3 GetFriction(Vector3 velocity, Vector3 direction, Vector3 axis, Vector3 normal, float maxSpeed, bool clampSpeed, float constantFriction, float relativeFriction, float proportionalFriction, float deltaTime, float fixedDeltaTime)
		{
			float frictionMultiplier = 1.0f - Mathf.Clamp01(Vector3.Dot(direction.normalized, normal));

			constantFriction     *= frictionMultiplier;
			relativeFriction     *= frictionMultiplier;
			proportionalFriction *= frictionMultiplier;

			return GetFriction(velocity, direction, axis, maxSpeed, clampSpeed, constantFriction, relativeFriction, proportionalFriction, deltaTime, fixedDeltaTime);
		}

		public static Vector3 CombineAccelerationAndFriction(Vector3 velocity, Vector3 acceleration, Vector3 friction)
		{
			velocity.x = CombineAxis(velocity.x, acceleration.x, friction.x);
			velocity.y = CombineAxis(velocity.y, acceleration.y, friction.y);
			velocity.z = CombineAxis(velocity.z, acceleration.z, friction.z);

			return velocity;

			static float CombineAxis(float axisVelocity, float axisAcceleration, float axisFriction)
			{
				float velocityDelta = axisAcceleration + axisFriction;

				if (Mathf.Abs(axisAcceleration) >= Mathf.Abs(axisFriction))
				{
					axisVelocity += velocityDelta;
				}
				else
				{
					if (axisVelocity > 0.0)
					{
						axisVelocity = Mathf.Max(0.0f, axisVelocity + velocityDelta);
					}
					else if (axisVelocity < 0.0)
					{
						axisVelocity = Mathf.Min(axisVelocity + velocityDelta, 0.0f);
					}
				}

				return axisVelocity;
			}
		}

		public static Vector3 AccumulatePenetrationCorrection(Vector3 accumulatedCorrection, Vector3 contactCorrectionDirection, float contactCorrectionMagnitude)
		{
			float   accumulatedCorrectionMagnitude = Vector3.Magnitude(accumulatedCorrection);
			Vector3 accumulatedCorrectionDirection = accumulatedCorrectionMagnitude > 0.0000000001f ? accumulatedCorrection / accumulatedCorrectionMagnitude : Vector3.zero;

			float   deltaCorrectionMagnitude = default;
			Vector3 deltaCorrectionDirection = Vector3.Cross(Vector3.Cross(accumulatedCorrectionDirection, contactCorrectionDirection), accumulatedCorrectionDirection).normalized;

			float cos    = Vector3.Dot(contactCorrectionDirection, accumulatedCorrectionDirection);
			float sinSqr = 1.0f - cos * cos;

			if (sinSqr > 0.001f)
			{
				deltaCorrectionMagnitude = (contactCorrectionMagnitude - accumulatedCorrectionMagnitude * cos) / Mathf.Sqrt(sinSqr);
			}
			else if (contactCorrectionMagnitude > accumulatedCorrectionMagnitude)
			{
				deltaCorrectionMagnitude = contactCorrectionMagnitude - accumulatedCorrectionMagnitude;
				deltaCorrectionDirection = accumulatedCorrectionDirection;
			}

			accumulatedCorrection += deltaCorrectionDirection * deltaCorrectionMagnitude;

			return accumulatedCorrection;
		}

		// PRIVATE METHODS

		private static float GetCompensatedDeltaTime(float deltaTime, float fixedDeltaTime)
		{
			return deltaTime * Mathf.Clamp(fixedDeltaTime / deltaTime, 1.0f, 8.0f);
		}
	}
}
