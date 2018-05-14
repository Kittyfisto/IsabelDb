using System.Threading;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.IntervalCollection
{
	[TestFixture]
	public sealed class IntervalCollectionCompabilityTest
		: AbstractCollectionCompatibilityTest
	{
		private int _lastKey;

		[SetUp]
		public void Setup()
		{
			_lastKey = 0;
		}

		#region Overrides of AbstractCollectionCompatibilityTest
		
		protected override ICollection<T> GetCollection<T>(IDatabase db, string name)
		{
			return db.GetIntervalCollection<int, T>(name);
		}

		protected override void Put<T>(ICollection<T> collection, T value)
		{
			((IIntervalCollection<int, T>)collection).Put(Interval.Create(Interlocked.Increment(ref _lastKey)), value);
		}

		#endregion
	}
}
