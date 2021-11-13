using NUnit.Framework;

namespace IsabelDb.Test.Collections.HashSet
{
	[TestFixture]
	public sealed class HashSetCompatibilityTest
		: AbstractCollectionCompatibilityTest
	{
		#region Overrides of AbstractCollectionCompatibilityTest

		protected override ICollection<T> GetCollection<T>(IDatabase db, string name)
		{
			return db.GetOrCreateHashSet<T>(name);
		}

		protected override void Put<T>(ICollection<T> collection, T value)
		{
			((IHashSet<T>) collection).Add(value);
		}

		#endregion
	}
}
