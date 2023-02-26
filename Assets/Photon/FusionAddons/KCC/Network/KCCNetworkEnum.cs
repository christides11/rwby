namespace Fusion.KCC
{
	using System;

	public sealed unsafe class KCCNetworkEnum<TContext, TEnum> : KCCNetworkProperty<TContext> where TContext : class where TEnum : unmanaged, Enum
	{
		// PRIVATE MEMBERS

		private readonly Action<TContext, TEnum>                    _set;
		private readonly Func<TContext, TEnum>                      _get;
		private readonly Func<TContext, float, TEnum, TEnum, TEnum> _interpolate;

		// CONSTRUCTORS

		public KCCNetworkEnum(TContext context, Action<TContext, TEnum> set, Func<TContext, TEnum> get, Func<TContext, float, TEnum, TEnum, TEnum> interpolate) : base(context, 1)
		{
			_set         = set;
			_get         = get;
			_interpolate = interpolate;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			_set(Context, EnumConvertor.ToEnum<TEnum>(*ptr));
		}

		public override void Write(int* ptr)
		{
			*ptr = EnumConvertor.ToInt(_get(Context));
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
			int fromValue = *interpolationData.From;
			int toValue   = *interpolationData.To;
			int value;

			if (_interpolate != null)
			{
				value = EnumConvertor.ToInt(_interpolate(Context, interpolationData.Alpha, EnumConvertor.ToEnum<TEnum>(fromValue), EnumConvertor.ToEnum<TEnum>(toValue)));
			}
			else
			{
				value = interpolationData.Alpha < 0.5f ? fromValue: toValue;
			}

			_set(Context, EnumConvertor.ToEnum<TEnum>(value));
		}

		private static class EnumConvertor
		{
			public static int ToInt<T>(T value) where T : unmanaged, Enum
			{
				return *(int*)(&value);
			}

			public static T ToEnum<T>(int value) where T : unmanaged, Enum
			{
				return *(T*)(&value);
			}
		}
	}
}
