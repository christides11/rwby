namespace Fusion.KCC
{
	using System;
	using System.Runtime.CompilerServices;
	using UnityEngine;

	public static partial class KCCMathUtility
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Map(float inMin, float inMax, float outMin, float outMax, float value)
		{
			if (value <= inMin)
				return outMin;

			if (value >= inMax)
				return outMax;

			return (outMax - outMin) * ((value - inMin) / (inMax - inMin)) + outMin;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyIn2(float t)
		{
			return t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyIn3(float t)
		{
			return t * t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyIn4(float t)
		{
			return t * t * t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyOut2(float t)
		{
			t = 1.0f - t;
			return 1.0f - t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyOut3(float t)
		{
			t = 1.0f - t;
			return 1.0f - t * t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyOut4(float t)
		{
			t = 1.0f - t;
			return 1.0f - t * t * t * t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyInOut2(float t)
		{
			t = t * 2.0f;
			if (t <= 1.0f)
			{
				t = 0.5f * t * t;
			}
			else
			{
				t = t - 1.0f;
				t = -0.5f * (t * (t - 2.0f) - 1.0f);
			}
			return t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyInOut3(float t)
		{
			t = t * 2.0f;
			if (t <= 1.0f)
			{
				t = 0.5f * t * t * t;
			}
			else
			{
				t = t - 2.0f;
				t = 0.5f * (t * t * t + 2.0f);
			}
			return t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EasyInOut4(float t)
		{
			t = t * 2.0f;
			if (t <= 1.0f)
			{
				t = 0.5f * t * t * t * t;
			}
			else
			{
				t = t - 2.0f;
				t = -0.5f * (t * t * t * t - 2.0f);
			}
			return t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FastAtan(float x)
		{
			return (0.97239411f - 0.19194795f * x * x) * x;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FastAtan2(float y, float x)
		{
			if (x != 0.0f)
			{
				float absX = x >= 0.0f ? x : -x;
				float absY = y >= 0.0f ? y : -y;

				if (absX > absY)
				{
					if (x > 0.0f)
						return FastAtan(y / x);
					if (y >= 0.0f)
						return FastAtan(y / x) + Mathf.PI;

					return FastAtan(y / x) - Mathf.PI;
				}
				else
				{
					if (y > 0.0f)
						return -FastAtan(x / y) + Mathf.PI * 0.5f;

					return -FastAtan(x / y) - Mathf.PI * 0.5f;
				}
			}
			else
			{
				if (y > 0.0f)
					return Mathf.PI * 0.5f;
				if (y < 0.0f)
					return -Mathf.PI * 0.5f;
			}

			return 0.0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InterpolateRange(float from, float to, float min, float max, float alpha)
		{
			float range = max - min;
			if (range <= 0.0f)
				throw new ArgumentException($"{nameof(max)} must be greater than {nameof(min)}!");

			if (from < min) { from = min; } else if (from > max) { from = max; }
			if (to   < min) { to   = min; } else if (to   > max) { to   = max; }

			if (from == to)
				return from;

			float halfRange = range * 0.5f;

			float interpolatedValue;

			if (from < to)
			{
				float distance = to - from;
				if (distance <= halfRange)
				{
					interpolatedValue = Mathf.Lerp(from, to, alpha);
				}
				else
				{
					interpolatedValue = Mathf.Lerp(from + range, to, alpha);
					if (interpolatedValue > max)
					{
						interpolatedValue -= range;
					}
				}
			}
			else
			{
				float distance = from - to;
				if (distance <= halfRange)
				{
					interpolatedValue = Mathf.Lerp(from, to, alpha);
				}
				else
				{
					interpolatedValue = Mathf.Lerp(from - range, to, alpha);
					if (interpolatedValue <= min)
					{
						interpolatedValue += range;
					}
				}
			}

			return interpolatedValue;
		}
	}
}
