using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IsabelDb.Browser.Inspectors
{
	/// <summary>
	/// Inspector for a collection which holds only values (and no keys).
	/// </summary>
	public sealed class ValueCollectionInspectionViewModel
		: AbstractInspectionViewModel
	{
		private readonly ObservableCollection<ValueViewModel> _rows;

		public static ValueCollectionInspectionViewModel Create<T>(ICollection<T> collection)
		{
			var rows = new ObservableCollection<ValueViewModel>(collection.GetAllValues().Take(100).Select(x => new ValueViewModel(x)));
			return new ValueCollectionInspectionViewModel(collection, rows);
		}

		private ValueCollectionInspectionViewModel(ICollection collection, ObservableCollection<ValueViewModel> rows)
			: base(collection)
		{
			_rows = rows;
		}

		public bool HasValues => _rows.Count > 0;
		public IEnumerable<IValueViewModel> Values => _rows;
	}
}
