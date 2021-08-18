using Fusion;
using UnityEngine;

namespace rwby
{
	/// <summary>
	/// Our custom definition of an INetworkStruct. Keep in mind that
	/// * bool does not work (C# does not define a consistent size on different platforms)
	/// * Must be a top-level struct (cannot be a nested class)
	/// * Stick to primitive types and structs
	/// * Size is not an issue since only modified data is serialized, but things that change often should be compact (e.g. button states)
	/// </summary>
	public struct NetworkInputData : INetworkInput
	{
		public const uint BUTTON_JUMP = 1 << 0;
		public const uint BUTTON_BLOCK = 1 << 1;
		public const uint BUTTON_LIGHT_ATTACK = 1 << 2;
		public const uint BUTTON_HEAVY_ATTACK = 1 << 3;
		public const uint BUTTON_DASH = 1 << 4;
		public const uint BUTTON_LOCK_ON = 1 << 5;
		public const uint BUTTON_SHOOT = 1 << 6;
		
		public const uint BUTTON_DPAD_UP = 1 << 7;
		public const uint BUTTON_DPAD_DOWN = 1 << 8;
		public const uint BUTTON_DPAD_LEFT = 1 << 9;
		public const uint BUTTON_DPAD_RIGHT = 1 << 10;
		
		public const uint BUTTON_TAUNT = 1 << 11;

		public const uint BUTTON_ABILITY_ONE = 1 << 12;
		public const uint BUTTON_ABILITY_TWO = 1 << 13;
		public const uint BUTTON_ABILITY_THREE = 1 << 14;
		public const uint BUTTON_ABILITY_FOUR = 1 << 15;

		public uint Buttons;
		public Vector2 movement;
		public Vector3 forward;
		public Vector3 right;

		public bool IsUp(uint button)
		{
			return IsDown(button) == false;
		}

		public bool IsDown(uint button)
		{
			return (Buttons & button) == button;
		}
	}
}