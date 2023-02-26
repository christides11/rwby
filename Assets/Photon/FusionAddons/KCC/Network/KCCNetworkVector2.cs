namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	public sealed unsafe class KCCNetworkVector2<TContext> : KCCNetworkProperty<TContext> where TContext : class
	{
		// PRIVATE MEMBERS

		private readonly float _readAccuracy;
		private readonly float _writeAccuracy;

		private readonly Action<TContext, Vector2>                        _set;
		private readonly Func<TContext, Vector2>                          _get;
		private readonly Func<TContext, float, Vector2, Vector2, Vector2> _interpolate;

		// CONSTRUCTORS

		public KCCNetworkVector2(TContext context, float accuracy, Action<TContext, Vector2> set, Func<TContext, Vector2> get, Func<TContext, float, Vector2, Vector2, Vector2> interpolate) : base(context, 2)
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
			Vector2 value = default;

			if (_readAccuracy <= 0.0f)
			{
				value.x = *(float*)(ptr + 0);
				value.y = *(float*)(ptr + 1);
			}
			else
			{
				value.x = (*(ptr + 0)) * _readAccuracy;
				value.y = (*(ptr + 1)) * _readAccuracy;
			}

			_set(Context, value);
		}

		public override void Write(int* ptr)
		{
			Vector2 value = _get(Context);

			if (_writeAccuracy <= 0.0f)
			{
				*(float*)(ptr + 0) = value.x;
				*(float*)(ptr + 1) = value.y;
			}
			else
			{
				*(ptr + 0) = value.x < 0.0f ? (int)((value.x * _writeAccuracy) - 0.5f) : (int)((value.x * _writeAccuracy) + 0.5f);
				*(ptr + 1) = value.y < 0.0f ? (int)((value.y * _writeAccuracy) - 0.5f) : (int)((value.y * _writeAccuracy) + 0.5f);
			}
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
			int* fromPtr = interpolationData.From;
			int* toPtr   = interpolationData.To;

			Vector2 fromValue;
			Vector2 toValue;
			Vector2 value;

			if (_readAccuracy <= 0.0f)
			{
				fromValue.x = *(float*)(fromPtr + 0);
				fromValue.y = *(float*)(fromPtr + 1);

				toValue.x = *(float*)(toPtr + 0);
				toValue.y = *(float*)(toPtr + 1);
			}
			else
			{
				fromValue.x = (*(fromPtr + 0)) * _readAccuracy;
				fromValue.y = (*(fromPtr + 1)) * _readAccuracy;

				toValue.x = (*(toPtr + 0)) * _readAccuracy;
				toValue.y = (*(toPtr + 1)) * _readAccuracy;
			}

			if (_interpolate != null)
			{
				value = _interpolate(Context, interpolationData.Alpha, fromValue, toValue);
			}
			else
			{
				value = Vector2.Lerp(fromValue, toValue, interpolationData.Alpha);
			}

			_set(Context, value);
		}
	}
}
