namespace Fusion.KCC
{
	using UnityEngine;

	/// <summary>
	/// Main data structure used for movement calculations. Stores data which require rollback, network synchronization + various metadata.
	/// </summary>
	public sealed partial class KCCData
	{
		// PUBLIC MEMBERS

		/// <summary>
		/// Frame number, equals to <c>Time.frameCount</c>
		/// </summary>
		public int Frame;

		/// <summary>
		/// Tick number, equals to <c>Simulation.Tick</c> or calculated fixed update frame count
		/// </summary>
		public int Tick;

		/// <summary>
		/// Relative position of the time between two fixed times. Valid range is &lt;0.0f, 1.0f&gt;
		/// </summary>
		public float Alpha;

		/// <summary>
		/// Current time, equals to <c>NetworkRunner.SimulationTime</c> or <c>NetworkRunner.SimulationRenderTime</c> or variable if CCD is active
		/// </summary>
		public float Time;

		/// <summary>
		/// Partial delta time, variable if CCD is active. Valid range is &lt;<c>0.0f, UnscaledDeltaTime</c>&gt;
		/// </summary>
		public float DeltaTime;

		/// <summary>
		/// CCD independent delta time
	    /// <list type="number">
	    /// <item><description>FixedUpdate => FixedUpdate</description></item>
	    /// <item><description>FixedUpdate => Render</description></item>
	    /// <item><description>Render => Render</description></item>
	    /// </list>
		/// </summary>
		public float UnscaledDeltaTime;

		/// <summary>
		/// Base position, initialized with TargetPosition at the start of each KCC step
		/// </summary>
		public Vector3 BasePosition;

		/// <summary>
		/// Desired position before depenetration and post-processing
		/// </summary>
		public Vector3 DesiredPosition;

		/// <summary>
		/// Calculated or explicitly set position which is propagated to <c>Transform</c>
		/// </summary>
		public Vector3 TargetPosition;

		/// <summary>
		/// Explicitly set look pitch rotation, this should propagate to camera rotation
		/// </summary>
		public float LookPitch
		{
			get { return _lookPitch; }
			set
			{
				if (_lookPitch != value)
				{
					_lookPitch                    = value;
					_lookRotationCalculated       = false;
					_lookDirectionCalculated      = false;
					_transformRotationCalculated  = false;
					_transformDirectionCalculated = false;
				}
			}
		}

		/// <summary>
		/// Explicitly set look yaw rotation, this should propagate to camera and transform rotation
		/// </summary>
		public float LookYaw
		{
			get { return _lookYaw; }
			set
			{
				if (_lookYaw != value)
				{
					_lookYaw                       = value;
					_lookRotationCalculated        = false;
					_lookDirectionCalculated       = false;
					_transformRotationCalculated   = false;
					_transformDirectionCalculated  = false;
				}
			}
		}

		/// <summary>
		/// Combination of <c>LookPitch</c> and <c>LookYaw</c>
		/// </summary>
		public Quaternion LookRotation
		{
			get
			{
				if (_lookRotationCalculated == false)
				{
					_lookRotation           = Quaternion.Euler(_lookPitch, _lookYaw, 0.0f);
					_lookRotationCalculated = true;
				}

				return _lookRotation;
			}
		}

		/// <summary>
		/// Calculated and cached look direction based on <c>LookRotation</c>
		/// </summary>
		public Vector3 LookDirection
		{
			get
			{
				if (_lookDirectionCalculated == false)
				{
					_lookDirection           = LookRotation * Vector3.forward;
					_lookDirectionCalculated = true;
				}

				return _lookDirection;
			}
		}

		/// <summary>
		/// Calculated and cached transform rotation based on Yaw look rotation
		/// </summary>
		public Quaternion TransformRotation
		{
			get
			{
				if (_transformRotationCalculated == false)
				{
					_transformRotation           = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
					_transformRotationCalculated = true;
				}

				return _transformRotation;
			}
		}

		/// <summary>
		/// Calculated and cached transform direction based on <c>TransformRotation</c>
		/// </summary>
		public Vector3 TransformDirection
		{
			get
			{
				if (_transformDirectionCalculated == false)
				{
					_transformDirection           = TransformRotation * Vector3.forward;
					_transformDirectionCalculated = true;
				}

				return _transformDirection;
			}
		}

		/// <summary>
		/// Non-interpolated world space input direction from - based on Keyboard / Joystick / NavMesh / ...
		/// </summary>
		public Vector3 InputDirection;

		/// <summary>
		/// One-time world space jump impulse based on input
		/// </summary>
		public Vector3 JumpImpulse;

		/// <summary>
		/// Gravitational acceleration
		/// </summary>
		public Vector3 Gravity;

		/// <summary>
		/// Maximum angle between KCC up direction and ground normal (depenetration vector) in degrees. Valid range is &lt;0, 90&gt;. Default is 75.
		/// </summary>
		public float MaxGroundAngle;

		/// <summary>
		/// Maximum angle between KCC up direction and wall surface (perpendicular to depenetration vector) in degrees. Valid range is &lt;0, MaxGroundAngle&gt; Default is 5.
		/// </summary>
		public float MaxWallAngle;

		/// <summary>
		/// Maximum angle between KCC up direction and hang surface (perpendicular to depenetration vector) in degrees. Valid range is &lt;MaxWallAngle, 90&gt; Default is 30.
		/// </summary>
		public float MaxHangAngle;

		/// <summary>
		/// Velocity from external sources, one-time effect - reseted on the end of <c>Move()</c> call to prevent subsequent applications in render, ignoring <c>Mass</c>, example usage - jump pad
		/// </summary>
		public Vector3 ExternalVelocity;

		/// <summary>
		/// Acceleration from external sources, continuous effect - value remains same for subsequent applications in render, ignoring <c>Mass</c>, example usage - escalator
		/// </summary>
		public Vector3 ExternalAcceleration;

		/// <summary>
		/// Impulse from external sources, one-time effect - reseted on the end of <c>Move()</c> call to prevent subsequent applications in render, affected by <c>Mass</c>, example usage - explosion
		/// </summary>
		public Vector3 ExternalImpulse;

		/// <summary>
		/// Force from external sources, continuous effect - value remains same for subsequent applications in render, affected by Mass, example usage - attractor
		/// </summary>
		public Vector3 ExternalForce;

		/// <summary>
		/// Absolute position delta which is consumed by single move. It can also be set from ProcessPhysicsQuery and still consumed by currently executed move (useful for depenetration corrections)
		/// </summary>
		public Vector3 ExternalDelta;

		/// <summary>
		/// Speed used to calculate <c>KinematicSpeed</c>
		/// </summary>
		public float KinematicSpeed;

		/// <summary>
		/// Calculated kinematic tangent, based on <c>KinematicDirection</c> and affected by other factors like ground normal, used to calculate <c>KinematicVelocity</c>
		/// </summary>
		public Vector3 KinematicTangent;

		/// <summary>
		/// Desired kinematic direction, based on <c>InputDirection</c> and other factors
		/// </summary>
		public Vector3 KinematicDirection;

		/// <summary>
		/// Velocity calculated from <c>InputDirection</c>, <c>KinematicDirection</c>, <c>KinematicTangent</c>, <c>KinematicSpeed</c>
		/// </summary>
		public Vector3 KinematicVelocity;

		/// <summary>
		/// Velocity calculated from <c>Gravity</c>, <c>ExternalVelocity</c>, <c>ExternalAcceleration</c>, <c>ExternalImpulse</c>, <c>ExternalForce</c>, <c>JumpImpulse</c>
		/// </summary>
		public Vector3 DynamicVelocity;

		/// <summary>
		/// Final calculated velocity used for position change, combined <c>KinematicVelocity</c> and <c>DynamicVelocity</c>
		/// </summary>
		public Vector3 DesiredVelocity => KinematicVelocity + DynamicVelocity;

		/// <summary>
		/// Speed calculated from real position change
		/// </summary>
		public float RealSpeed;

		/// <summary>
		/// Velocity calculated from real position change
		/// </summary>
		public Vector3 RealVelocity;

		/// <summary>
		/// Flag that indicates KCC has jumped in current tick
		/// </summary>
		public bool HasJumped;

		/// <summary>
		/// Flag that indicates KCC has teleported in current tick
		/// </summary>
		public bool HasTeleported;

		/// <summary>
		/// Flag that indicates KCC is touching a collider with normal angle lower than <c>MaxGroundAngle</c>
		/// </summary>
		public bool IsGrounded;

		/// <summary>
		/// Same as IsGrounded previous tick or physics query
		/// </summary>
		public bool WasGrounded;

		/// <summary>
		/// Indicates the KCC temporarily or permanently lost grounded state
		/// </summary>
		public bool IsOnEdge => IsGrounded == false && WasGrounded == true;

		/// <summary>
		/// Indicates the KCC is stepping up
		/// </summary>
		public bool IsSteppingUp;

		/// <summary>
		/// Same as IsSteppingUp previous tick or physics query
		/// </summary>
		public bool WasSteppingUp;

		/// <summary>
		/// Indicates the KCC temporarily lost grounded state and is snapping to ground
		/// </summary>
		public bool IsSnappingToGround;

		/// <summary>
		/// Same as IsSnappingToGround previous tick or physics query
		/// </summary>
		public bool WasSnappingToGround;

		/// <summary>
		/// Combined normal of all touching colliders. Normals less distant from up direction have bigger impacton final normal
		/// </summary>
		public Vector3 GroundNormal;

		/// <summary>
		/// Tangent to <c>GroundNormal</c>, can be calculated from <c>DesiredVelocity</c> or <c>TargetRotation</c> if <c>GroundNormal</c> and up direction is same
		/// </summary>
		public Vector3 GroundTangent;

		/// <summary>
		/// Position of the KCC collider surface touching the ground collider
		/// </summary>
		public Vector3 GroundPosition;

		/// <summary>
		/// Distance from ground
		/// </summary>
		public float GroundDistance;

		/// <summary>
		/// Difference between ground normal and up direction in degrees
		/// </summary>
		public float GroundAngle;

		/// <summary>
		/// Collection of networked collisions. Represents colliders the KCC interacts with.
		/// Only objects with NetworkObject component are stored for compatibility with local prediction.
		/// </summary>
		public readonly KCCCollisions Collisions = new KCCCollisions();

		/// <summary>
		/// Collection of manually registered modifiers (for example processors) the KCC interacts with.
		/// Only objects with NetworkObject component are stored for compatibility with local prediction.
		/// </summary>
		public readonly KCCModifiers Modifiers = new KCCModifiers();

		/// <summary>
		/// Collection of ignored colliders.
		/// Only objects with NetworkObject component are stored for compatibility with local prediction.
		/// </summary>
		public readonly KCCIgnores Ignores = new KCCIgnores();

		/// <summary>
		/// Collection of colliders/triggers the KCC overlaps (radius + extent).
		/// This collection is not synchronized! All objects are stored, NetworkObject component is not needed, only local history is supported.
		/// </summary>
		public readonly KCCHits Hits = new KCCHits();

		// PRIVATE MEMBERS

		private float      _lookPitch;
		private float      _lookYaw;
		private Quaternion _lookRotation;
		private bool       _lookRotationCalculated;
		private Vector3    _lookDirection;
		private bool       _lookDirectionCalculated;
		private Quaternion _transformRotation;
		private bool       _transformRotationCalculated;
		private Vector3    _transformDirection;
		private bool       _transformDirectionCalculated;

		// PUBLIC METHODS

		public Vector2 GetLookRotation(bool pitch, bool yaw)
		{
			Vector2 lookRotation = default;

			if (pitch == true) { lookRotation.x = _lookPitch; }
			if (yaw   == true) { lookRotation.y = _lookYaw;   }

			return lookRotation;
		}

		public void Clear()
		{
			ClearUserData();

			Collisions.Clear(true);
			Modifiers.Clear(true);
			Ignores.Clear(true);
			Hits.Clear(true);
		}

		public void CopyFromOther(KCCData other)
		{
			Frame                        = other.Frame;
			Tick                         = other.Tick;
			Alpha                        = other.Alpha;
			Time                         = other.Time;
			DeltaTime                    = other.DeltaTime;
			UnscaledDeltaTime            = other.UnscaledDeltaTime;
			BasePosition                 = other.BasePosition;
			DesiredPosition              = other.DesiredPosition;
			TargetPosition               = other.TargetPosition;
			LookPitch                    = other.LookPitch;
			LookYaw                      = other.LookYaw;

			InputDirection               = other.InputDirection;
			JumpImpulse                  = other.JumpImpulse;
			Gravity                      = other.Gravity;
			MaxGroundAngle               = other.MaxGroundAngle;
			MaxWallAngle                 = other.MaxWallAngle;
			MaxHangAngle                 = other.MaxHangAngle;
			ExternalVelocity             = other.ExternalVelocity;
			ExternalAcceleration         = other.ExternalAcceleration;
			ExternalImpulse              = other.ExternalImpulse;
			ExternalForce                = other.ExternalForce;
			ExternalDelta                = other.ExternalDelta;

			KinematicSpeed               = other.KinematicSpeed;
			KinematicTangent             = other.KinematicTangent;
			KinematicDirection           = other.KinematicDirection;
			KinematicVelocity            = other.KinematicVelocity;
			DynamicVelocity              = other.DynamicVelocity;

			RealSpeed                    = other.RealSpeed;
			RealVelocity                 = other.RealVelocity;
			HasJumped                    = other.HasJumped;
			HasTeleported                = other.HasTeleported;
			IsGrounded                   = other.IsGrounded;
			WasGrounded                  = other.WasGrounded;
			IsSteppingUp                 = other.IsSteppingUp;
			WasSteppingUp                = other.WasSteppingUp;
			IsSnappingToGround           = other.IsSnappingToGround;
			WasSnappingToGround          = other.WasSnappingToGround;
			GroundNormal                 = other.GroundNormal;
			GroundTangent                = other.GroundTangent;
			GroundPosition               = other.GroundPosition;
			GroundDistance               = other.GroundDistance;
			GroundAngle                  = other.GroundAngle;

			Collisions.CopyFromOther(other.Collisions);
			Modifiers.CopyFromOther(other.Modifiers);
			Ignores.CopyFromOther(other.Ignores);
			Hits.CopyFromOther(other.Hits);

			CopyUserDataFromOther(other);
		}

		// PARTIAL METHODS

		partial void ClearUserData();
		partial void CopyUserDataFromOther(KCCData other);
	}
}
