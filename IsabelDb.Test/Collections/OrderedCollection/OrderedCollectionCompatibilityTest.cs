using System.Threading;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.OrderedCollection
{
	public sealed class OrderedCollectionCompatibilityTest
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
			return db.GetOrCreateOrderedCollection<int, T>(name);
		}

		protected override void Put<T>(ICollection<T> collection, T value)
		{
			((IOrderedCollection<int, T>)collection).Put(Interlocked.Increment(ref _lastKey), value);
		}

		#endregion
	}
}
