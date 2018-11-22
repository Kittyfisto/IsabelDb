using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IsabelDb.Browser
{
	public sealed class BagInspectorViewModel<T>
		: AbstractInspectorViewModel
	{
		private readonly ObservableCollection<ValueViewModel<T>> _rows;
		private readonly IBag<T> _collection;

		public BagInspectorViewModel(IBag<T> collection)
			: base(collection)
		{
			_collection = collection;
			_rows = new ObservableCollection<ValueViewModel<T>>(collection.GetAllValues().Take(100).Select(x => new ValueViewModel<T>(x)));
		}

		public IEnumerable<IValueViewModel> Values => _rows;
	}
}
