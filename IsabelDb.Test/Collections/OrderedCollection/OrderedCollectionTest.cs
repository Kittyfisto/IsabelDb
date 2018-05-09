using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.OrderedCollection
{
	[TestFixture]
	public sealed class OrderedCollectionTest
		: AbstractCollectionTest<IOrderedCollection<int, string>>
	{
		private int _nextKey;

		[SetUp]
		public void Setup()
		{
			_nextKey = 0;
		}

		#region Overrides of AbstractCollectionTest<IOrderedCollection<int,string>>

		protected override IOrderedCollection<int, string> GetCollection(Database db, string name)
		{
			return db.GetOrderedCollection<int, string>(name);
		}

		protected override void Put(IOrderedCollection<int, string> collection, string value)
		{
			collection.Put(Interlocked.Increment(ref _nextKey), value);
		}

		protected override void PutMany(IOrderedCollection<int, string> collection, params string[] values)
		{
			var pairs = new List<KeyValuePair<int, string>>(values.Length);
			foreach (var value in values)
			{
				pairs.Add(new KeyValuePair<int, string>(Interlocked.Increment(ref _nextKey), value));
			}
			collection.PutMany(pairs);
		}

		#endregion
	}
}
