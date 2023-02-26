namespace Fusion.KCC
{
	public unsafe interface IKCCNetworkProperty
	{
		int WordCount { get; }

		void Read(int* ptr);
		void Write(int* ptr);
		void Interpolate(InterpolationData interpolationData);
	}

	public unsafe abstract class KCCNetworkProperty<TContext> : IKCCNetworkProperty where TContext : class
	{
		// PUBLIC MEMBERS

		public readonly TContext Context;
		public readonly int      WordCount;

		// CONSTRUCTORS

		public KCCNetworkProperty(TContext context, int wordCount)
		{
			Context   = context;
			WordCount = wordCount;
		}

		// IKCCNetworkProperty INTERFACE

		int IKCCNetworkProperty.WordCount => WordCount;

		public abstract void Read(int* ptr);
		public abstract void Write(int* ptr);
		public abstract void Interpolate(InterpolationData interpolationData);
	}
}
