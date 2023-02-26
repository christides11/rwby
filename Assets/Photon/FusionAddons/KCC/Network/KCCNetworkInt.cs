namespace Fusion.KCC
{
	using System;
	using UnityEngine;

	public sealed unsafe class KCCNetworkInt<TContext> : KCCNetworkProperty<TContext> where TContext : class
	{
		// PRIVATE MEMBERS

		private readonly Action<TContext, int>                _set;
		private readonly Func<TContext, int>                  _get;
		private readonly Func<TContext, float, int, int, int> _interpolate;

		// CONSTRUCTORS

		public KCCNetworkInt(TContext context, Action<TContext, int> set, Func<TContext, int> get, Func<TContext, float, int, int, int> interpolate) : base(context, 1)
		{
			_set         = set;
			_get         = get;
			_interpolate = interpolate;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			_set(Context, *ptr);
		}

		public override void Write(int* ptr)
		{
			*ptr = _get(Context);
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
			int fromValue = *interpolationData.From;
			int toValue   = *interpolationData.To;
			int value;

			if (_interpolate != null)
			{
				value = _interpolate(Context, interpolationData.Alpha, fromValue, toValue);
			}
			else
			{
				value = (int)Mathf.Lerp(fromValue, toValue, interpolationData.Alpha);
			}

			_set(Context, value);
		}
	}
}
