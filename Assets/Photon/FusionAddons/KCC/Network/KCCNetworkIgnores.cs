namespace Fusion.KCC
{
	using System.Collections.Generic;

	public sealed unsafe class KCCNetworkIgnores : KCCNetworkProperty<KCCNetworkContext>
	{
		// PRIVATE MEMBERS

		private int _maxCount;

		// CONSTRUCTORS

		public KCCNetworkIgnores(KCCNetworkContext context, int maxCount) : base(context, 1 + maxCount * 4)
		{
			_maxCount = maxCount;
		}

		// KCCNetworkProperty INTERFACE

		public override void Read(int* ptr)
		{
			KCCData       data   = Context.Data;
			NetworkRunner runner = Context.KCC.Runner;

			data.Ignores.Clear(false);

			int  count     = *ptr;
			int* ignorePtr = ptr + 1;

			for (int i = 0; i < count; ++i)
			{
				KCCNetworkID networkID = KCCNetworkUtility.ReadNetworkID(ignorePtr);
				ignorePtr += 4;

				if (networkID.IsValid == true)
				{
					data.Ignores.Add(KCCNetworkID.GetNetworkObject(runner, networkID), networkID);
				}
			}
		}

		public override void Write(int* ptr)
		{
			KCCData data = Context.Data;

			int  ignoreCount = 0;
			int* ignorePtr   = ptr + 1;

			List<KCCIgnore> ignores = data.Ignores.All;
			for (int i = 0, count = ignores.Count; i < count; ++i)
			{
				KCCIgnore ignore = ignores[i];
				if (ignore.NetworkID.IsValid == false)
					continue;

				KCCNetworkUtility.WriteNetworkID(ignorePtr, ignore.NetworkID);
				ignorePtr += 4;

				++ignoreCount;

				if (ignoreCount >= _maxCount)
					break;
			}

			*ptr = ignoreCount;
		}

		public override void Interpolate(InterpolationData interpolationData)
		{
		}
	}
}
