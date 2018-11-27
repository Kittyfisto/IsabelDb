using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace IsabelDb.Browser.Inspectors
{
	public sealed class DictionaryViewModel
		: AbstractCollectionViewModel
	{
		private readonly ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>> _rows;
		private DataGridCellInfo _selectedCell;

		public static DictionaryViewModel Create<TKey, TValue>(IDictionary<TKey, TValue> collection)
		{
			var rows = new ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>>(
			                                                                                  collection.GetAll().Take(100).Select(CreatePair)
			                                                                                 );
			return new DictionaryViewModel(collection, rows);
		}

		private static KeyValuePair<ValueViewModel, ValueViewModel> CreatePair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
		{
			return new KeyValuePair<ValueViewModel, ValueViewModel>(new ValueViewModel(pair.Key),
			                                                        new ValueViewModel(pair.Value));
		}

		public IEnumerable<KeyValuePair<ValueViewModel, ValueViewModel>> Rows => _rows;

		public DataGridCellInfo SelectedCell
		{
			get { return _selectedCell; }
			set
			{
				if (value == _selectedCell)
					return;

				_selectedCell = value;
				EmitPropertyChanged();

				SelectedRow = UnpackSelectedCell(value);
			}
		}

		private object UnpackSelectedCell(DataGridCellInfo value)
		{
			if (value.Item is KeyValuePair<ValueViewModel, ValueViewModel> pair)
			{
				if (value.Column.DisplayIndex == 0)
					return pair.Key;

				return pair.Value;
			}

			return null;
		}

		private DictionaryViewModel(ICollection collection,
		                                     ObservableCollection<KeyValuePair<ValueViewModel, ValueViewModel>> rows)
			: base(collection)
		{
			_rows = rows;
		}

		#region Overrides of AbstractInspectionViewModel

		protected override void DisplayRows(long startIndex, long count)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}