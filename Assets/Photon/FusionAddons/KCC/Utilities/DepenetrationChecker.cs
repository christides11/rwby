namespace Fusion.KCC
{
	using UnityEngine;
	using UnityEngine.Profiling;

	/// <summary>
	/// Drag & drop utility to check depenetration algorithm behavior on specific position.
	/// </summary>
	public sealed class DepenetrationChecker : MonoBehaviour
	{
		[SerializeField]
		private Transform _origin;
		[SerializeField]
		private Transform _depenetrated;
		[SerializeField]
		private Collider  _collider;
		[SerializeField]
		private float     _radius = 0.35f;
		[SerializeField]
		private float     _height = 1.8f;
		[SerializeField]
		private float     _extent = 0.035f;
		[SerializeField]
		private float     _maxGroundAngle = 75.0f;
		[SerializeField]
		private float     _maxWallAngle = 5.0f;
		[SerializeField]
		private LayerMask _collisionLayerMask = 1;
		[SerializeField]
		private int       _subSteps = 3;
		[SerializeField]
		private Vector3   _dynamicVelocity;

		private KCCData        _data         = new KCCData();
		private Collider[]     _hitColliders = new Collider[64];
		private KCCOverlapInfo _overlapInfo  = new KCCOverlapInfo(64);
		private KCCResolver    _resolver     = new KCCResolver(64);

		private void Update()
		{
			Profiler.BeginSample("DepenetrationChecker");

			Vector3 basePosition    = _origin.position;
			Vector3 desiredPosition = transform.position;

			KCCData data = _data;

			data.BasePosition    = basePosition;
			data.DesiredPosition = desiredPosition;
			data.TargetPosition  = desiredPosition;
			data.DynamicVelocity = _dynamicVelocity;
			data.MaxGroundAngle  = _maxGroundAngle;
			data.MaxWallAngle    = _maxWallAngle;

			data.WasGrounded         = data.IsGrounded;
			data.WasSteppingUp       = data.IsSteppingUp;
			data.WasSnappingToGround = data.IsSnappingToGround;

			data.IsGrounded          = default;
			data.IsSteppingUp        = default;
			data.IsSnappingToGround  = default;
			data.GroundNormal        = default;
			data.GroundTangent       = default;
			data.GroundPosition      = default;
			data.GroundDistance      = default;
			data.GroundAngle         = default;

			OverlapCapsule(_overlapInfo, _data, data.TargetPosition, _radius, _height, _radius, _collisionLayerMask, QueryTriggerInteraction.Collide);

			_overlapInfo.ToggleConvexMeshColliders(false);

			data.TargetPosition = DepenetrateColliders(_overlapInfo, data, data.BasePosition, data.TargetPosition, true, true, _subSteps);

			_overlapInfo.ToggleConvexMeshColliders(true);

			if (data.TargetPosition.IsEqual(desiredPosition) == false)
			{
				Debug.DrawLine(desiredPosition, desiredPosition + (data.TargetPosition - desiredPosition).normalized, Color.magenta);
				Debug.DrawLine(desiredPosition, data.TargetPosition, Color.green);
			}

			_depenetrated.position = data.TargetPosition;

			Profiler.EndSample();
		}

		private Vector3 DepenetrateColliders(KCCOverlapInfo overlapInfo, KCCData data, Vector3 basePosition, Vector3 targetPosition, bool probeGrounding, bool probeSteppingUp, int maxSubSteps)
		{
			if (overlapInfo.ColliderHitCount == 0)
				return targetPosition;

			if (overlapInfo.ColliderHitCount == 1)
				return DepenetrateSingle(overlapInfo, data, basePosition, targetPosition, probeGrounding);

			return DepenetrateMultiple(overlapInfo, data, basePosition, targetPosition, probeGrounding, maxSubSteps);
		}

		private Vector3 DepenetrateSingle(KCCOverlapInfo overlapInfo, KCCData data, Vector3 basePosition, Vector3 targetPosition, bool probeGrounding)
		{
			bool    hasGroundDot   = default;
			float   minGroundDot   = default;
			Vector3 groundNormal   = Vector3.up;
			float   groundDistance = default;

			KCCOverlapHit hit = overlapInfo.ColliderHits[0];

			hit.HasPenetration = Physics.ComputePenetration(_collider, targetPosition, Quaternion.identity, hit.Collider, hit.Transform.position, hit.Transform.rotation, out Vector3 direction, out float distance);
			if (hit.HasPenetration == true)
			{
				Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction, Color.yellow);
				Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction * distance, Color.red);

				hit.IsWithinExtent = true;

				hasGroundDot = true;
				minGroundDot = Mathf.Cos(Mathf.Clamp(data.MaxGroundAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);

				float directionUpDot = Vector3.Dot(direction, Vector3.up);
				if (directionUpDot >= minGroundDot)
				{
					hit.CollisionType = ECollisionType.Ground;

					data.IsGrounded = true;

					groundNormal = direction;
				}
				else
				{
					probeGrounding = false;

					float minWallDot = -Mathf.Cos(Mathf.Clamp(90.0f - data.MaxWallAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);

					if (directionUpDot > -minWallDot)
					{
						hit.CollisionType = ECollisionType.Slope;
					}
					else if (directionUpDot >= minWallDot)
					{
						hit.CollisionType = ECollisionType.Wall;
					}
					else
					{
						float minHangDot = -Mathf.Cos(Mathf.Clamp(90.0f - data.MaxHangAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);

						if (directionUpDot >= minHangDot)
						{
							hit.CollisionType = ECollisionType.Hang;
						}
						else
						{
							hit.CollisionType = ECollisionType.Top;
						}
					}

					if (directionUpDot > 0.0f && distance >= 0.000001f && data.DynamicVelocity.y <= 0.0f)
					{
						Vector3 positionDelta = targetPosition - basePosition;

						float movementDot = Vector3.Dot(positionDelta.OnlyXZ(), direction.OnlyXZ());
						if (movementDot < 0.0f)
						{
							KCCPhysicsUtility.ProjectVerticalPenetration(ref direction, ref distance);
						}
					}
				}

				targetPosition += direction * distance;
			}

			if (probeGrounding == true && data.IsGrounded == false)
			{
				if (hasGroundDot == false)
				{
					minGroundDot = Mathf.Cos(Mathf.Clamp(data.MaxGroundAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);
				}

				bool isGrounded = KCCPhysicsUtility.CheckGround(_collider, targetPosition, hit.Collider, hit.Transform, _radius, _height, _extent, minGroundDot, out Vector3 checkGroundNormal, out float checkGroundDistance, out bool isWithinExtent);
				if (isGrounded == true)
				{
					groundNormal   = checkGroundNormal;
					groundDistance = checkGroundDistance;

					data.IsGrounded = true;

					hit.CollisionType = ECollisionType.Ground;
				}

				hit.IsWithinExtent |= isWithinExtent;
			}

			if (data.IsGrounded == true)
			{
				data.GroundNormal   = groundNormal;
				data.GroundAngle    = Vector3.Angle(groundNormal, Vector3.up);
				data.GroundPosition = targetPosition + new Vector3(0.0f, _radius, 0.0f) - groundNormal * (_radius + groundDistance);
				data.GroundDistance = groundDistance;
			}

			return targetPosition;
		}

		private Vector3 DepenetrateMultiple(KCCOverlapInfo overlapInfo, KCCData data, Vector3 basePosition, Vector3 targetPosition, bool probeGrounding, int maxSubSteps)
		{
			float   minGroundDot        = Mathf.Cos(Mathf.Clamp(data.MaxGroundAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);
			float   minWallDot          = -Mathf.Cos(Mathf.Clamp(90.0f - data.MaxWallAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);
			float   minHangDot          = -Mathf.Cos(Mathf.Clamp(90.0f - data.MaxHangAngle, 0.0f, 90.0f) * Mathf.Deg2Rad);
			int     groundColliders     = default;
			float   groundDistance      = default;
			float   maxGroundDot        = default;
			Vector3 maxGroundNormal     = default;
			Vector3 averageGroundNormal = default;
			Vector3 positionDelta       = targetPosition - basePosition;
			Vector3 positionDeltaXZ     = positionDelta.OnlyXZ();

			_resolver.Reset();

			for (int i = 0; i < overlapInfo.ColliderHitCount; ++i)
			{
				KCCOverlapHit hit = overlapInfo.ColliderHits[i];

				hit.HasPenetration = Physics.ComputePenetration(_collider, targetPosition, Quaternion.identity, hit.Collider, hit.Transform.position, hit.Transform.rotation, out Vector3 direction, out float distance);
				if (hit.HasPenetration == false)
					continue;

				Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction, Color.yellow);
				Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction * distance, Color.red);

				hit.IsWithinExtent = true;

				float directionUpDot = Vector3.Dot(direction, Vector3.up);
				if (directionUpDot >= minGroundDot)
				{
					hit.CollisionType = ECollisionType.Ground;

					data.IsGrounded = true;

					++groundColliders;

					if (directionUpDot >= maxGroundDot)
					{
						maxGroundDot    = directionUpDot;
						maxGroundNormal = direction;
					}

					averageGroundNormal += direction * directionUpDot;
				}
				else
				{
					if (directionUpDot > -minWallDot)
					{
						hit.CollisionType = ECollisionType.Slope;
					}
					else if (directionUpDot >= minWallDot)
					{
						hit.CollisionType = ECollisionType.Wall;
					}
					else if (directionUpDot >= minHangDot)
					{
						hit.CollisionType = ECollisionType.Hang;
					}
					else
					{
						hit.CollisionType = ECollisionType.Top;
					}

					if (directionUpDot > 0.0f && distance >= 0.000001f && data.DynamicVelocity.y <= 0.0f)
					{
						float movementDot = Vector3.Dot(positionDeltaXZ, direction.OnlyXZ());
						if (movementDot < 0.0f)
						{
							KCCPhysicsUtility.ProjectVerticalPenetration(ref direction, ref distance);
						}
					}
				}

				_resolver.AddCorrection(direction, distance);
			}

			int remainingSubSteps = Mathf.Max(0, maxSubSteps);

			float multiplier = 1.0f - Mathf.Min(remainingSubSteps, 2) * 0.25f;

			if (_resolver.Size == 2)
			{
				_resolver.GetCorrection(0, out Vector3 direction0);
				_resolver.GetCorrection(1, out Vector3 direction1);

				if (Vector3.Dot(direction0, direction1) >= 0.0f)
				{
					targetPosition += _resolver.CalculateMinMax() * multiplier;
				}
				else
				{
					targetPosition += _resolver.CalculateBinary() * multiplier;
				}
			}
			else
			{
				targetPosition += _resolver.CalculateGradientDescent(12, 0.0001f) * multiplier;
			}

			while (remainingSubSteps > 0)
			{
				--remainingSubSteps;

				_resolver.Reset();

				for (int i = 0; i < overlapInfo.ColliderHitCount; ++i)
				{
					KCCOverlapHit hit = overlapInfo.ColliderHits[i];

					bool hasPenetration = Physics.ComputePenetration(_collider, targetPosition, Quaternion.identity, hit.Collider, hit.Transform.position, hit.Transform.rotation, out Vector3 direction, out float distance);
					if (hasPenetration == false)
						continue;

					float directionUpDot = Vector3.Dot(direction, Vector3.up);

					if (hit.HasPenetration == false)
					{
						Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction, Color.yellow);
						Debug.DrawLine(hit.Transform.position, hit.Transform.position + direction * distance, Color.red);

						if (directionUpDot >= minGroundDot)
						{
							hit.CollisionType = ECollisionType.Ground;

							data.IsGrounded = true;

							++groundColliders;

							if (directionUpDot >= maxGroundDot)
							{
								maxGroundDot    = directionUpDot;
								maxGroundNormal = direction;
							}

							averageGroundNormal += direction * directionUpDot;
						}
						else if (directionUpDot > -minWallDot)
						{
							hit.CollisionType = ECollisionType.Slope;
						}
						else if (directionUpDot >= minWallDot)
						{
							hit.CollisionType = ECollisionType.Wall;
						}
						else if (directionUpDot >= minHangDot)
						{
							hit.CollisionType = ECollisionType.Hang;
						}
						else
						{
							hit.CollisionType = ECollisionType.Top;
						}
					}

					hit.HasPenetration = true;
					hit.IsWithinExtent = true;

					if (directionUpDot < minGroundDot)
					{
						if (directionUpDot > 0.0f && distance >= 0.000001f && data.DynamicVelocity.y <= 0.0f)
						{
							float movementDot = Vector3.Dot(positionDeltaXZ, direction.OnlyXZ());
							if (movementDot < 0.0f)
							{
								KCCPhysicsUtility.ProjectVerticalPenetration(ref direction, ref distance);
							}
						}
					}

					_resolver.AddCorrection(direction, distance);
				}

				if (_resolver.Size == 0)
					break;

				if (remainingSubSteps == 0)
				{
					if (_resolver.Size == 2)
					{
						_resolver.GetCorrection(0, out Vector3 direction0);
						_resolver.GetCorrection(1, out Vector3 direction1);

						if (Vector3.Dot(direction0, direction1) >= 0.0f)
						{
							targetPosition += _resolver.CalculateGradientDescent(12, 0.0001f);
						}
						else
						{
							targetPosition += _resolver.CalculateBinary();
						}
					}
					else
					{
						targetPosition += _resolver.CalculateGradientDescent(12, 0.0001f);
					}
				}
				else if (remainingSubSteps == 1)
				{
					targetPosition += _resolver.CalculateMinMax() * 0.75f;
				}
				else
				{
					targetPosition += _resolver.CalculateMinMax() * 0.5f;
				}
			}

			if (probeGrounding == true && data.IsGrounded == false)
			{
				Vector3 closestGroundNormal   = Vector3.up;
				float   closestGroundDistance = 1000.0f;

				for (int i = 0; i < overlapInfo.ColliderHitCount; ++i)
				{
					KCCOverlapHit hit = overlapInfo.ColliderHits[i];

					bool isGrounded = KCCPhysicsUtility.CheckGround(_collider, targetPosition, hit.Collider, hit.Transform, _radius, _height, _extent, minGroundDot, out Vector3 checkGroundNormal, out float checkGroundDistance, out bool isWithinExtent);
					if (isGrounded == true)
					{
						data.IsGrounded = true;

						if (checkGroundDistance < closestGroundDistance)
						{
							closestGroundNormal   = checkGroundNormal;
							closestGroundDistance = checkGroundDistance;
						}

						hit.CollisionType = ECollisionType.Ground;
					}

					hit.IsWithinExtent |= isWithinExtent;
				}

				if (data.IsGrounded == true)
				{
					maxGroundNormal     = closestGroundNormal;
					averageGroundNormal = closestGroundNormal;
					groundDistance      = closestGroundDistance;
					groundColliders     = 1;
				}
			}

			if (data.IsGrounded == true)
			{
				if (groundColliders <= 1)
				{
					averageGroundNormal = maxGroundNormal;
				}
				else
				{
					averageGroundNormal.Normalize();
				}

				data.GroundNormal   = averageGroundNormal;
				data.GroundAngle    = Vector3.Angle(data.GroundNormal, Vector3.up);
				data.GroundPosition = targetPosition + new Vector3(0.0f, _radius, 0.0f) - data.GroundNormal * (_radius + groundDistance);
				data.GroundDistance = groundDistance;
			}

			return targetPosition;
		}

		private bool OverlapCapsule(KCCOverlapInfo overlapInfo, KCCData data, Vector3 position, float radius, float height, float extent, LayerMask layerMask, QueryTriggerInteraction triggerInteraction)
		{
			overlapInfo.Reset(false);

			overlapInfo.Position           = position;
			overlapInfo.Radius             = radius;
			overlapInfo.Height             = height;
			overlapInfo.Extent             = extent;
			overlapInfo.LayerMask          = layerMask;
			overlapInfo.TriggerInteraction = triggerInteraction;

			Vector3 positionUp   = position + new Vector3(0.0f, height - radius, 0.0f);
			Vector3 positionDown = position + new Vector3(0.0f, radius, 0.0f);

			Collider   hitCollider;
			Collider[] hitColliders     = _hitColliders;
			int        hitColliderCount = Physics.defaultPhysicsScene.OverlapCapsule(positionDown, positionUp, radius + extent, hitColliders, layerMask, triggerInteraction);

			for (int i = 0; i < hitColliderCount; ++i)
			{
				hitCollider = hitColliders[i];

				if (hitCollider != _collider)
				{
					overlapInfo.AddHit(hitCollider);
				}
			}

			return overlapInfo.AllHitCount > 0;
		}
	}
}
