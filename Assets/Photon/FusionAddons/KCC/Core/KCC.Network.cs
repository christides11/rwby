namespace Fusion.KCC
{
	using System.Collections.Generic;
	using UnityEngine;

	public class KCCNetworkContext
	{
		public KCC         KCC;
		public KCCData     Data;
		public KCCSettings Settings;
	}

	public partial class KCC
	{
		// PRIVATE MEMBERS

		private KCCNetworkContext     _networkContext;
		private IKCCNetworkProperty[] _networkProperties;
		private KCCNetworkProperties  _defaultProperties;

		private static float _defaultPositionReadAccuracy = float.NaN;

		// PUBLIC METHODS

		public unsafe Vector3 ReadNetworkPosition(int* ptr)
		{
			return _defaultProperties.ReadPosition(ptr);
		}

		// NetworkAreaOfInterestBehaviour INTERFACE

		public override int PositionWordOffset => 0;

		// PRIVATE METHODS

		private int GetNetworkDataWordCount()
		{
			InitializeNetworkProperties();

			int wordCount = 0;

			for (int i = 0, count = _networkProperties.Length; i < count; ++i)
			{
				IKCCNetworkProperty property = _networkProperties[i];
				wordCount += property.WordCount;
			}

			return wordCount;
		}

		private unsafe void ReadNetworkData()
		{
			_networkContext.Data = _fixedData;

			int* ptr = Ptr;

			for (int i = 0, count = _networkProperties.Length; i < count; ++i)
			{
				IKCCNetworkProperty property = _networkProperties[i];
				property.Read(ptr);
				ptr += property.WordCount;
			}
		}

		private unsafe void WriteNetworkData()
		{
			_networkContext.Data = _fixedData;

			int* ptr = Ptr;

			for (int i = 0, count = _networkProperties.Length; i < count; ++i)
			{
				IKCCNetworkProperty property = _networkProperties[i];
				property.Write(ptr);
				ptr += property.WordCount;
			}
		}

		private unsafe void InterpolateNetworkData(float alpha = -1.0f)
		{
			if (_driver != EKCCDriver.Fusion)
				return;
			if (GetInterpolationData(out InterpolationData interpolationData) == false)
				return;

			if (alpha >= 0.0f && alpha <= 1.0f)
			{
				interpolationData.Alpha = alpha;
			}

			int   ticks = interpolationData.ToTick - interpolationData.FromTick;
			float tick  = interpolationData.FromTick + interpolationData.Alpha * ticks;

			// Store base Ptr for later use.

			int* basePtrFrom = interpolationData.From;
			int* basePtrTo   = interpolationData.To;

			// We start with fixed data which has a state from server or interpolated state from last frame.

			_networkContext.Data = _fixedData;

			// Set general properties.

			_fixedData.Frame             = Time.frameCount;
			_fixedData.Tick              = Mathf.RoundToInt(tick);
			_fixedData.Alpha             = interpolationData.Alpha;
			_fixedData.DeltaTime         = Runner.DeltaTime;
			_fixedData.UnscaledDeltaTime = _fixedData.DeltaTime;
			_fixedData.Time              = tick * _fixedData.DeltaTime;

			// Interpolate all networked properties.

			for (int i = 0, count = _networkProperties.Length; i < count; ++i)
			{
				IKCCNetworkProperty property = _networkProperties[i];
				property.Interpolate(interpolationData);
				interpolationData.From += property.WordCount;
				interpolationData.To   += property.WordCount;
			}

			// Teleport detection.

			if (ticks > 0)
			{
				Vector3 fromPosition = KCCNetworkUtility.ReadVector3(basePtrFrom, _defaultPositionReadAccuracy);
				Vector3 toPosition   = KCCNetworkUtility.ReadVector3(basePtrTo,   _defaultPositionReadAccuracy);

				Vector3 positionDifference = toPosition - fromPosition;
				if (positionDifference.sqrMagnitude > _settings.TeleportThreshold * _settings.TeleportThreshold * ticks * ticks)
				{
					_fixedData.TargetPosition = toPosition;
					_fixedData.RealVelocity   = Vector3.zero;
					_fixedData.RealSpeed      = 0.0f;
				}
				else
				{
					_fixedData.RealVelocity = positionDifference / (_fixedData.DeltaTime * ticks);
					_fixedData.RealSpeed    = _fixedData.RealVelocity.magnitude;
				}
			}

			// User interpolation and post-processing.

			InterpolateUserNetworkData(interpolationData);

			// Flipping fixed data to render data.

			_renderData.CopyFromOther(_fixedData);
		}

		private void RestoreHistoryData(KCCData historyData)
		{
			// Some values can be synchronized from user code.
			// We have to ensure these properties are in correct state with other properties.

			if (_fixedData.IsGrounded == true)
			{
				// Reset IsGrounded to history state, otherwise using GroundNormal and other ground related properties leads to undefined behavior and NaN propagation.
				// This has effect only if IsGrounded is synchronized over network.
				_fixedData.IsGrounded = historyData.IsGrounded;
			}

			// User history data restoration.

			RestoreUserHistoryData(historyData);
		}

		private void InitializeNetworkProperties()
		{
			if (_defaultPositionReadAccuracy.IsNaN() == true)
			{
				_defaultPositionReadAccuracy = new Accuracy(AccuracyDefaults.POSITION).Value;
			}

			if (_networkContext != null)
				return;

			_networkContext = new KCCNetworkContext();
			_networkContext.KCC      = this;
			_networkContext.Settings = _settings;

			_defaultProperties = new KCCNetworkProperties(_networkContext, _settings.PositionAccuracy, _settings.RotationAccuracy);

			List<IKCCNetworkProperty> properties = new List<IKCCNetworkProperty>(32);
			properties.Add(_defaultProperties);

			if (_settings.MaxNetworkedCollisions > 0) { properties.Add(new KCCNetworkCollisions(_networkContext, _settings.MaxNetworkedCollisions)); }
			if (_settings.MaxNetworkedModifiers  > 0) { properties.Add(new KCCNetworkModifiers (_networkContext, _settings.MaxNetworkedModifiers));  }
			if (_settings.MaxNetworkedIgnores    > 0) { properties.Add(new KCCNetworkIgnores   (_networkContext, _settings.MaxNetworkedIgnores));    }

			InitializeUserNetworkProperties(_networkContext, properties);

			_networkProperties = properties.ToArray();
		}

		// PARTIAL METHODS

		partial void InitializeUserNetworkProperties(KCCNetworkContext networkContext, List<IKCCNetworkProperty> networkProperties);
		partial void InterpolateUserNetworkData(InterpolationData interpolationData);
		partial void RestoreUserHistoryData(KCCData historyData);
	}
}
