namespace IsabelDb.Browser
{
	public abstract class AbstractInspectorViewModel
		: ICollectionInspectorViewModel
	{
		private readonly ICollection _collection;
		private readonly long _count;

		public AbstractInspectorViewModel(ICollection collection)
		{
			_collection = collection;
			_count = collection.Count();
		}

		public long Count => _count;
	}
}