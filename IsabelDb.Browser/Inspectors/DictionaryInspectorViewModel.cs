using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IsabelDb.Browser.Inspectors
{
	public sealed class DictionaryInspectorViewModel
		: AbstractInspectionViewModel
	{
		private readonly ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>> _rows;

		public static DictionaryInspectorViewModel Create<TKey, TValue>(IDictionary<TKey, TValue> collection)
		{
			var rows = new ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>>(
			                                                                                  collection.GetAll().Take(100).Select(CreatePair)
			                                                                                 );
			return new DictionaryInspectorViewModel(collection, rows);
		}

		private static KeyValuePair<ValueViewModel, ValueViewModel> CreatePair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		{
			return new KeyValuePair<ValueViewModel, ValueViewModel>(new ValueViewModel(pair.Key),
			                                                        new ValueViewModel(pair.Value));
		}

		private DictionaryInspectorViewModel(ICollection collection,
		                                     ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>> rows)
			: base(collection)
		{
			_rows = rows;
		}
	}
}