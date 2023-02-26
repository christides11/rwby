namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	public sealed unsafe class KCCNetworkFloatArray<TContext> : KCCNetworkProperty<TContext> where TContext : class
	{
		// PRIVATE MEMBERS

		private readonly int   _count;
		private readonly float _readAccuracy;
		private readonly float _writeAccuracy;

		private readonly Action<TContext, int, float>                    _set;
		private readonly Func<TContext, int, float>                      _get;
		private readonly Func<TContext, int, float, float, float, float> _interpolate;

		// CONSTRUCTORS

		public KCCNetworkFloatArray(TContext context, int count, float accuracy, Action<TContext, int, float> set, Func<TContext, int, float> get, Func<TContext, int, float, float, float, float> interpolate) : base(context, count)
		{
			_count         = count;
			_readAccuracy  = accuracy > 0.0f ? accuracy        : 0.0f;
			_writeAccuracy = accuracy > 0.0f ? 1.0f / accuracy : 0.0f;

			_set         = set;
			_get         = get;
			_interpolate = interpolate;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			for (int i = 0; i < _count; ++i)
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

				_set(Context, i, value);

				++ptr;
			}
		}

		public override void Write(int* ptr)
		{
			for (int i = 0; i < _count; ++i)
			{
				float value = _get(Context, i);

				if (_writeAccuracy <= 0.0f)
				{
					*(float*)ptr = value;
				}
				else
				{
					*ptr = value < 0.0f ? (int)((value * _writeAccuracy) - 0.5f) : (int)((value * _writeAccuracy) + 0.5f);
				}

				++ptr;
			}
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
			for (int i = 0; i < _count; ++i)
			{
				float fromValue = _readAccuracy <= 0.0f ? *(float*)interpolationData.From : (*interpolationData.From) * _readAccuracy;
				float toValue   = _readAccuracy <= 0.0f ? *(float*)interpolationData.To   : (*interpolationData.To)   * _readAccuracy;
				float value;

				if (_interpolate != null)
				{
					value = _interpolate(Context, i, interpolationData.Alpha, fromValue, toValue);
				}
				else
				{
					value = Mathf.Lerp(fromValue, toValue, interpolationData.Alpha);
				}

				_set(Context, i, value);

				++interpolationData.From;
				++interpolationData.To;
			}
		}
	}
}
