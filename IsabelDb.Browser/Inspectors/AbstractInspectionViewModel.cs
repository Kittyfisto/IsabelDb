namespace IsabelDb.Browser.Inspectors
{
	public abstract class AbstractInspectionViewModel
		: ICollectionInspectionViewModel
	{
		private readonly ICollection _collection;
		private readonly long _count;

		public AbstractInspectionViewModel(ICollection collection)
		{
			_collection = collection;
			_count = collection.Count();
		}

		public long Count => _count;
	}
}