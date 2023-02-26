namespace Fusion.KCC
{
	using System.Runtime.CompilerServices;
	using UnityEngine;

	public static unsafe partial class KCCNetworkUtility
	{
		// CONSTANTS

		public const int WORD_COUNT_BOOL    = 1;
		public const int WORD_COUNT_INT     = 1;
		public const int WORD_COUNT_FLOAT   = 1;
		public const int WORD_COUNT_VECTOR3 = 3;

		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBool(int* ptr)
		{
			return *ptr != 0 ? true : false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBool(int* ptr, bool value)
		{
			*ptr = value == true ? 1 : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt(int* ptr)
		{
			return *ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt(int* ptr, int value)
		{
			*ptr = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadFloat(int* ptr, float accuracy = 0.0f)
		{
			if (accuracy <= 0.0f)
			{
				return *(float*)ptr;
			}
			else
			{
				return (*ptr) * accuracy;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteFloat(int* ptr, float value, float inverseAccuracy = 0.0f)
		{
			if (inverseAccuracy <= 0.0f)
			{
				*(float*)ptr = value;
			}
			else
			{
				*ptr = value < 0.0f ? (int)((value * inverseAccuracy) - 0.5f) : (int)((value * inverseAccuracy) + 0.5f);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ReadVector3(int* ptr, float accuracy = 0.0f)
		{
			if (accuracy <= 0.0f)
			{
				Vector3 value;
				value.x = *(float*)(ptr);
				value.y = *(float*)(ptr + 1);
				value.z = *(float*)(ptr + 2);
				return value;
			}
			else
			{
				Vector3 value;
				value.x = (*(ptr + 0)) * accuracy;
				value.y = (*(ptr + 1)) * accuracy;
				value.z = (*(ptr + 2)) * accuracy;
				return value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteVector3(int* ptr, Vector3 value, float inverseAccuracy = 0.0f)
		{
			if (inverseAccuracy <= 0.0f)
			{
				*(float*)(ptr)     = value.x;
				*(float*)(ptr + 1) = value.y;
				*(float*)(ptr + 2) = value.z;
			}
			else
			{
				*(ptr + 0) = value.x < 0.0f ? (int)((value.x * inverseAccuracy) - 0.5f) : (int)((value.x * inverseAccuracy) + 0.5f);
				*(ptr + 1) = value.y < 0.0f ? (int)((value.y * inverseAccuracy) - 0.5f) : (int)((value.y * inverseAccuracy) + 0.5f);
				*(ptr + 2) = value.z < 0.0f ? (int)((value.z * inverseAccuracy) - 0.5f) : (int)((value.z * inverseAccuracy) + 0.5f);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static KCCNetworkID ReadNetworkID(int* ptr)
		{
			KCCNetworkID networkID = new KCCNetworkID();
			networkID.A = *(ptr + 0);
			networkID.B = *(ptr + 1);
			networkID.C = *(ptr + 2);
			networkID.D = *(ptr + 3);
			return networkID;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteNetworkID(int* ptr, KCCNetworkID networkID)
		{
			*(ptr + 0) = networkID.A;
			*(ptr + 1) = networkID.B;
			*(ptr + 2) = networkID.C;
			*(ptr + 3) = networkID.D;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InterpolateRange(float from, float to, float min, float max, float alpha) => KCCMathUtility.InterpolateRange(from, to, min, max, alpha);
	}
}
