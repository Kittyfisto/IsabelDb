using NUnit.Framework;

namespace IsabelDb.Test.Collections.Bag
{
	[TestFixture]
	public sealed class BagCompatibilityTest
		: AbstractCollectionCompatibilityTest
	{
		#region Overrides of AbstractCollectionCompatibilityTest
		
		protected override ICollection<T> GetCollection<T>(IDatabase db, string name)
		{
			return db.GetOrCreateBag<T>(name);
		}

		protected override void Put<T>(ICollection<T> collection, T value)
		{
			((IBag<T>)collection).Put(value);
		}

		#endregion
	}
}
