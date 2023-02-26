namespace Fusion.KCC
{
	using System.Collections.Generic;

	public sealed unsafe class KCCNetworkModifiers : KCCNetworkProperty<KCCNetworkContext>
	{
		// PRIVATE MEMBERS

		private int _maxCount;

		// CONSTRUCTORS

		public KCCNetworkModifiers(KCCNetworkContext context, int maxCount) : base(context, 1 + maxCount * 4)
		{
			_maxCount = maxCount;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			KCCData       data   = Context.Data;
			NetworkRunner runner = Context.KCC.Runner;

			data.Modifiers.Clear(false);

			int  count       = *ptr;
			int* modifierPtr = ptr + 1;

			for (int i = 0; i < count; ++i)
			{
				KCCNetworkID networkID = KCCNetworkUtility.ReadNetworkID(modifierPtr);
				modifierPtr += 4;

				if (networkID.IsValid == true)
				{
					data.Modifiers.Add(KCCNetworkID.GetNetworkObject(runner, networkID), networkID);
				}
			}
		}

		public override void Write(int* ptr)
		{
			KCCData data = Context.Data;

			int  modifierCount = 0;
			int* modifierPtr   = ptr + 1;

			List<KCCModifier> modifiers = data.Modifiers.All;
			for (int i = 0, count = modifiers.Count; i < count; ++i)
			{
				KCCModifier modifier = modifiers[i];
				if (modifier.NetworkID.IsValid == false)
					continue;

				KCCNetworkUtility.WriteNetworkID(modifierPtr, modifier.NetworkID);
				modifierPtr += 4;

				++modifierCount;

				if (modifierCount >= _maxCount)
					break;
			}

			*ptr = modifierCount;
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
		}
	}
}
