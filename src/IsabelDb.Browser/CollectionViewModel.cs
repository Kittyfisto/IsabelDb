namespace IsabelDb.Browser
{
	public sealed class CollectionViewModel
	{
		private readonly ICollection _collection;

		public CollectionViewModel(ICollection collection)
		{
			_collection = collection;
		}

		public string Name => _collection.Name;

		public CollectionType CollectionType => _collection.Type;

		public ICollection Collection => _collection;
	}
}