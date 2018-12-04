using System.Threading;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.MultiValueDictionary
{
	[TestFixture]
	public sealed class MultiValueDictionaryCompatibilityTest
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
			return db.GetOrCreateMultiValueDictionary<int, T>(name);
		}

		protected override void Put<T>(ICollection<T> collection, T value)
		{
			((IMultiValueDictionary<int, T>)collection).Put(Interlocked.Increment(ref _lastKey), value);
		}

		#endregion
	}
}
