namespace Fusion.KCC
{
	using System;

	/// <summary>
	/// Controls mode in which KCC operates. Use <c>Fusion</c> for fully networked characters, <c>Unity</c> can be used for non-networked local characters (NPCs, cinematics, ...).
	/// </summary>
	public enum EKCCDriver
	{
		None   = 0,
		Unity  = 1,
		Fusion = 2,
	}

	/// <summary>
	/// Defines KCC render behavior for input/state authority.
    /// <list type="bullet">
    /// <item><description>None - Skips render completely. Useful when render update is perfectly synchronized with fixed update or debugging.</description></item>
    /// <item><description>Predict - Full processing and physics query.</description></item>
    /// <item><description>Interpolate - Interpolation between last two fixed updates.</description></item>
    /// </list>
	/// </summary>
	public enum EKCCRenderBehavior
	{
		None        = 0,
		Predict     = 1,
		Interpolate = 2,
	}

	/// <summary>
	/// Defines KCC physics behavior.
    /// <list type="bullet">
    /// <item><description>None - Skips almost all execution including processors, collider is despawned.</description></item>
    /// <item><description>Capsule - Full processing with capsule collider spawned.</description></item>
    /// <item><description>Void - Skips internal physics query, collider is despawned, processors are executed.</description></item>
    /// </list>
	/// </summary>
	public enum EKCCShape
	{
		None    = 0,
		Capsule = 1,
		Void    = 2,
	}

	public enum EKCCStage
	{
		None                  = 0,
		SetInputProperties    = 1,
		SetDynamicVelocity    = 2,
		SetKinematicDirection = 3,
		SetKinematicTangent   = 4,
		SetKinematicSpeed     = 5,
		SetKinematicVelocity  = 6,
		ProcessPhysicsQuery   = 7,
		OnStay                = 8,
		OnInterpolate         = 9,
		ProcessUserLogic      = 10,
	}

	[Flags]
	public enum EKCCStages
	{
		None                  = 0,
		SetInputProperties    = 1 << EKCCStage.SetInputProperties,
		SetDynamicVelocity    = 1 << EKCCStage.SetDynamicVelocity,
		SetKinematicDirection = 1 << EKCCStage.SetKinematicDirection,
		SetKinematicTangent   = 1 << EKCCStage.SetKinematicTangent,
		SetKinematicSpeed     = 1 << EKCCStage.SetKinematicSpeed,
		SetKinematicVelocity  = 1 << EKCCStage.SetKinematicVelocity,
		ProcessPhysicsQuery   = 1 << EKCCStage.ProcessPhysicsQuery,
		OnStay                = 1 << EKCCStage.OnStay,
		OnInterpolate         = 1 << EKCCStage.OnInterpolate,
		ProcessUserLogic      = 1 << EKCCStage.ProcessUserLogic,
		All                   = -1
	}

	public enum EKCCFeature
	{
		None                 = 0,
		StepUp               = 1,
		SnapToGround         = 2,
		PredictionCorrection = 3,
		AntiJitter           = 4,
		CCD                  = 5,
	}

	[Flags]
	public enum EKCCFeatures
	{
		None                 = 0,
		StepUp               = 1 << EKCCFeature.StepUp,
		SnapToGround         = 1 << EKCCFeature.SnapToGround,
		PredictionCorrection = 1 << EKCCFeature.PredictionCorrection,
		AntiJitter           = 1 << EKCCFeature.AntiJitter,
		CCD                  = 1 << EKCCFeature.CCD,
		All                  = -1
	}

	public enum EColliderType
	{
		None    = 0,
		Sphere  = 1,
		Capsule = 2,
		Box     = 3,
		Mesh    = 4,
		Terrain = 5,
	}

	/// <summary>
	/// Defines collision type between KCC and overlapping collider surface calculated from depenetration or trigger.
    /// <list type="bullet">
    /// <item><description>None - Default.</description></item>
    /// <item><description>Ground - Angle between Up and normalized depenetration vector is between 0 and KCCData.MaxGroundAngle.</description></item>
    /// <item><description>Slope - Angle between Up and normalized depenetration vector is between KCCData.MaxGroundAngle and (90 - KCCData.MaxWallAngle).</description></item>
    /// <item><description>Wall - Angle between Back and normalized depenetration vector is between -KCCData.MaxWallAngle and KCCData.MaxWallAngle.</description></item>
    /// <item><description>Hang - Angle between Back and normalized depenetration vector is between -30 and -KCCData.MaxWallAngle.</description></item>
    /// <item><description>Top - Angle between Back and normalized depenetration vector is lower than -30.</description></item>
    /// <item><description>Trigger - Overlapping collider - trigger. Penetration is unknown.</description></item>
    /// </list>
	/// </summary>
	public enum ECollisionType
	{
		None    = 0,
		Ground  = 1,
		Slope   = 1 << 1,
		Wall    = 1 << 2,
		Hang    = 1 << 3,
		Top     = 1 << 4,
		Trigger = 1 << 5,
	}
}
