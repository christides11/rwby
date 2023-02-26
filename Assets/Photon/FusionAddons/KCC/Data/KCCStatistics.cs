namespace Fusion.KCC
{
	/// <summary>
	/// Statistics for tracking KCC.
	/// </summary>
	public sealed class KCCStatistics
	{
		// PUBLIC MEMBERS

		public int OverlapQueries;
		public int RaycastQueries;
		public int ShapecastQueries;

		// PUBLIC METHODS

		public void Reset()
		{
			OverlapQueries   = default;
			RaycastQueries   = default;
			ShapecastQueries = default;
		}
	}
}
