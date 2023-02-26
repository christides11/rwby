namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	public sealed unsafe class KCCNetworkFloat<TContext> : KCCNetworkProperty<TContext> where TContext : class
	{
		// PRIVATE MEMBERS

		private readonly float _readAccuracy;
		private readonly float _writeAccuracy;

		private readonly Action<TContext, float>                    _set;
		private readonly Func<TContext, float>                      _get;
		private readonly Func<TContext, float, float, float, float> _interpolate;

		// CONSTRUCTORS

		public KCCNetworkFloat(TContext context, float accuracy, Action<TContext, float> set, Func<TContext, float> get, Func<TContext, float, float, float, float> interpolate) : base(context, 1)
		{
			_readAccuracy  = accuracy > 0.0f ? accuracy        : 0.0f;
			_writeAccuracy = accuracy > 0.0f ? 1.0f / accuracy : 0.0f;

			_set         = set;
			_get         = get;
			_interpolate = interpolate;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			float value;

			if (_readAccuracy <= 0.0f)
			{
				value = *(float*)ptr;
			}
			else
			{
				value = (*ptr) * _readAccuracy;
			}

			_set(Context, value);
		}

		public override void Write(int* ptr)
		{
			float value = _get(Context);

			if (_writeAccuracy <= 0.0f)
			{
				*(float*)ptr = value;
			}
			else
			{
				*ptr = value < 0.0f ? (int)((value * _writeAccuracy) - 0.5f) : (int)((value * _writeAccuracy) + 0.5f);
			}
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
			float fromValue = _readAccuracy <= 0.0f ? *(float*)interpolationData.From : (*interpolationData.From) * _readAccuracy;
			float toValue   = _readAccuracy <= 0.0f ? *(float*)interpolationData.To   : (*interpolationData.To)   * _readAccuracy;
			float value;

			if (_interpolate != null)
			{
				value = _interpolate(Context, interpolationData.Alpha, fromValue, toValue);
			}
			else
			{
				value = Mathf.Lerp(fromValue, toValue, interpolationData.Alpha);
			}

			_set(Context, value);
		}
	}
}
